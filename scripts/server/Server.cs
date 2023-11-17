using System;
using System.Threading;
using System.Collections.Generic;
using Godot;
using static Godot.GD;
using static Godot.MultiplayerApi;
using MsgPack.Serialization;

public partial class Server : Node {
	public override void _Ready() {
		int peers, port;
		GetServerArguments(out Global.Worlds, out port, out peers);
		Global.PlayersData = new Dictionary<long, Global.PlayerDataStruct>();
		
		// start UPNP
		Thread t = new Thread(UpnpOpenPort);
		t.Start(port);

		// server setup
		CreateServer(port, peers);
		
		// signals
		Multiplayer.PeerConnected += _OnPeerConnected;
		Multiplayer.PeerDisconnected += _OnPeerDisconnected;
	}

	//---------------------------------------------------------------------------------//
	#region | funcs

	void GetServerArguments(out string[] world, out int port, out int peers) {
		// default values
		world = Global.Worlds;
		port = Global.DEFAULT_PORT;
		peers = Global.MAX_PEERS;

		// cli args
		string[] args = new string[3];
		foreach (var arg in OS.GetCmdlineArgs()) {
			if (arg.Contains("=")) {
				string[] subArgs = arg.Split('=');
				bool error = false;

				switch(subArgs[0]) {
					case ("world"):
						error = false;
						if (subArgs[1] == "Rotation") {
							world = new string[] { "Cave", "CaveShort", "Loop" };
						} else {
							world[0] = subArgs[1];
						}
						break;
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

		Print($"{port}\t{peers}");
	}

	void UpnpOpenPort(Object portObj) {
		int port = (int) portObj;

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

	void CreateServer(int port, int peers) {
		var peer = new ENetMultiplayerPeer();
		peer.CreateServer(port, peers);
		Multiplayer.MultiplayerPeer = peer;
	}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | rpc

	[Rpc] void Client_Setup(byte[] serializedPlayerData) {}
	[Rpc] void Client_NewPlayer(long id, string username, Color color) {}
	[Rpc] void Client_PlayerLeft(long id, string gameState) {}

	[Rpc(RpcMode.AnyPeer)] void Server_NewPlayerData(string username, Color color) {
		Global.PlayersData.TryAdd(Multiplayer.GetRemoteSenderId(), new Global.PlayerDataStruct(username, color));
		Rpc(nameof(Client_NewPlayer), Multiplayer.GetRemoteSenderId(), username, color);
	}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | signals

	void _OnPeerConnected(long id) {
		var serializer = MessagePackSerializer.Get<Dictionary<long, Global.PlayerDataStruct>>();
		var serializedPlayerData = serializer.PackSingleObject(Global.PlayersData);

		RpcId(id, nameof(Client_Setup), serializedPlayerData);

		Print($"player {id} connected");
	}

	void _OnPeerDisconnected(long id) {
		Global.PlayersData.Remove(id);

		Rpc(nameof(Client_PlayerLeft), id, Global.GameState);

		if (Global.GameState == "Ingame") {
			GetNode($"{Global.WORLD_PATH}/{id}").QueueFree();
		}

		if (Multiplayer.GetPeers().Length == 0) {
			Global.GameState = "Lobby";
			var world = GetNodeOrNull(Global.WORLD_PATH);
			if (world != null) {
				world.QueueFree();
			}

			Multiplayer.MultiplayerPeer.RefuseNewConnections = false;
		}

		Print($"player {id} disconnected");
	}

	#endregion
}
