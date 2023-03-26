using Godot;
using static Godot.GD;
using static Godot.MultiplayerApi;
using System;
using System.Collections.Generic;
using System.Threading;
using MsgPack.Serialization;

public partial class Server : Node {
    public struct PlayerDataStruct {
        public PlayerDataStruct(string username, Color color) {
            Username = username;
            Color = color;
        }

        public string Username;
        public Color Color;
    }

	const int DEFAULT_PORT = 29999;
    const int MAX_PEERS = 99;
    const string WORLD_PATH = "/root/World/";

    string CurrentWorldName = "Cave";
    public Dictionary<long, PlayerDataStruct> PlayersData;

	public override void _Ready() {
        int peers, port;
        GetServerArguments(out port, out peers);
        PlayersData = new Dictionary<long, PlayerDataStruct>();
		
		// start UPNP
        Thread t = new Thread(UpnpOpenPort);
        t.Start(port);

		// server setup
        CreateServer(port, peers);
        SetCurrentWorld(CurrentWorldName);

        // signals
		Multiplayer.PeerConnected += _OnPeerConnected;
        Multiplayer.PeerDisconnected += _OnPeerDisconnected;
	}

	//---------------------------------------------------------------------------------//
    #region | funcs
    
    void CreateServer(int port, int peers) {
        var peer = new ENetMultiplayerPeer();
        peer.CreateServer(port, peers);
        Multiplayer.MultiplayerPeer = peer;
    }

    void SetCurrentWorld(string worldName) {
        CurrentWorldName = worldName;
        var worldScene = Load<PackedScene>("res://scenes/worlds/" + CurrentWorldName + ".tscn").Instantiate();
        GetNode("/root").CallDeferred("add_child", worldScene);
    }

    void GetServerArguments(out int port, out int peers) {
        // default values
        port = DEFAULT_PORT;
        peers = MAX_PEERS;

        // cli args
        string[] args = new string[2];
        foreach (var arg in OS.GetCmdlineArgs()) {
            if (arg.Contains("=")) {
                string[] subArgs = arg.Split('=');
                bool error = false;

                switch(subArgs[0]) {
                    case ("port"):
                        error = !int.TryParse(subArgs[1], out port);
                        break;
                    case ("players"):
                        error = !int.TryParse(subArgs[1], out peers);
                        break;
                }

                if (error) {
                    PushError("enter an actual numerical value");
                    GetTree().Quit();
                }
            }
        }

        Print(port);
        Print(peers);
    }

    void UpnpOpenPort(Object obj) {
		int port = (int) obj;

        var upnp = new Upnp();
        if (upnp.Discover() == 0 && upnp.GetGateway().IsValidGateway()) {
            var error = upnp.AddPortMapping(port, port, "", "UDP");
            var error2 = upnp.AddPortMapping(port, port, "", "TCP");
            if (error == 0 && error2 == 0) {
                Print("UPNP success");
            } else {
                upnp.DeletePortMapping(port, "UDP");
                upnp.DeletePortMapping(port, "TCP");
                Print("UPNP failed: Failed to add a port mapping (Is the port already forwarded? Port already in use?)");
            }
        } else {
            Print("UPNP failed: UPNP not enabled");
        }
    }

    #endregion

	//---------------------------------------------------------------------------------//
    #region | signals

	void _OnPeerConnected(long id) {
        // new client setup
        var serializer = MessagePackSerializer.Get<Dictionary<long, PlayerDataStruct>>();
        byte[] serializedData = serializer.PackSingleObject(PlayersData);
        RpcId(id, nameof(Client_Setup), CurrentWorldName, serializedData);

        // add server-side player node
        var newPlayer = Load<PackedScene>("res://scenes/Player.tscn").Instantiate();
        newPlayer.Name = id.ToString();
        GetNode(WORLD_PATH).CallDeferred("add_child", newPlayer);


        Print("player ", id, " connected");
	}   

    private void _OnPeerDisconnected(long id) {
        PlayersData.Remove(id);

        // update other clients
        Rpc("Client_PlayerDisconnected", id);

        // remove server-side player node
        GetNode(WORLD_PATH + id.ToString()).QueueFree();


        Print("player ", id, " disconnected");
    }

	#endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc] void Client_Setup(string worldName) {}
    [Rpc] void Client_NewPlayer(long id, string username, Color playerColor) {}
    [Rpc] void Client_PlayerDisconnected(long id) {}

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerData(string username, Color playerColor) {
        PlayersData.TryAdd(Multiplayer.GetRemoteSenderId(), new PlayerDataStruct(username, playerColor));

        Rpc("Client_NewPlayer", Multiplayer.GetRemoteSenderId(), username, playerColor);
    }

    #endregion
}
