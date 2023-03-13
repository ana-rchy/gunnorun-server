using Godot;
using static Godot.GD;
using System;
using System.Collections.Generic;
using System.Threading;
using MsgPack.Serialization;

public partial class Server : Node {
	private const int DEFAULT_PORT = 29999;
    private const int MAX_PEERS = 99;
    private const string WORLD_PATH = "/root/World/";

    private string CurrentWorldName = "AlphaArena";
    public List<long> PlayerIDs;

	public override void _Ready() {
        int peers, port;
        GetServerArguments(out port, out peers);
		
		// start UPNP
        Thread t = new Thread(UpnpOpenPort);
        t.Start(port);

		// server setup
        CreateServer(port, peers);
        SetCurrentWorld("AlphaArena");

        // signals
		Multiplayer.PeerConnected += _OnPeerConnected;
        Multiplayer.PeerDisconnected += _OnPeerDisconnected;

        // etc
        PlayerIDs = new List<long>();
	}

	//---------------------------------------------------------------------------------//
    #region | funcs
    
    private void CreateServer(int port, int peers) {
        var peer = new ENetMultiplayerPeer();
        peer.CreateServer(port, peers);
        Multiplayer.MultiplayerPeer = peer;
    }

    private void SetCurrentWorld(string worldName) {
        CurrentWorldName = worldName;
        var worldScene = Load<PackedScene>("res://game/scenes/worlds/" + CurrentWorldName + ".tscn").Instantiate();
        GetNode("/root").AddChild(worldScene);
    }

    private void GetServerArguments(out int port, out int peers) {
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

    private void UpnpOpenPort(Object obj) {
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

	private void _OnPeerConnected(long id) {
		PlayerIDs.Add(id);

        // new client setup + update other clients
        var serializer = MessagePackSerializer.Get<List<long>>();
        byte[] serializedIDs = serializer.PackSingleObject(PlayerIDs);
        RpcId(id, "RPC_PlayerSetup", CurrentWorldName, id, serializedIDs);

        // add server-side player node
        var newPlayerInstance = Load<PackedScene>("res://game/scenes/player/Player.tscn").Instantiate();
        newPlayerInstance.Name = id.ToString();
        newPlayerInstance.SetMultiplayerAuthority((int) id);
        GetNode(WORLD_PATH).AddChild(newPlayerInstance);


        Print("player ", id, " connected");
	}   

    private void _OnPeerDisconnected(long id) {
        PlayerIDs.Remove(id);

        // update other clients
        Rpc("RPC_PlayerDisconnected", id);

        // remove server-side player node
        GetNode(WORLD_PATH + id.ToString()).QueueFree();


        Print("player ", id, " disconnected");
    }

	#endregion
}
