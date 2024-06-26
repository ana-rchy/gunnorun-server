using System;
using System.Threading;
using System.Collections.Generic;
using Godot;
using static Godot.GD;
using static Godot.MultiplayerApi;

public partial class Server : Node {
	public override void _Ready() {
		int peers, port;
		GetServerArguments(out Global.CurrentWorld, out port, out peers);
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

	// pure
	void GetServerArguments(out string world, out int port, out int peers) {
		// default values
		world = Global.CurrentWorld;
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
						world = subArgs[1];
						break;
					case ("port"):
						error = !int.TryParse(subArgs[1], out port);
						break;
					case ("players"):
						error = !int.TryParse(subArgs[1], out peers);
						break;
				}

				if (error) {
					PushError("enter an actual value");
					GetTree().Quit();
				}
			}
		}

		Print($"{port}\t{peers}");
	}

	// side-effects
	void UpnpOpenPort(Object portObj) {
		int port = (int) portObj;

		var upnp = new Upnp();
		if (upnp.Discover() == 0 && upnp.GetGateway().IsValidGateway()) {
			var error = upnp.AddPortMapping(port, port, "", "UDP");
			var error2 = upnp.AddPortMapping(port, port, "", "TCP");
			if (error == 0 && error2 == 0) {
				Print("UPNP success");
			} else {
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
	#region | signals

	void _OnPeerConnected(long id) {
		Print($"player {id} connected");
	}

	void _OnPeerDisconnected(long id) {
		Global.PlayersData.Remove(id);

		Print($"player {id} disconnected");
	}

	#endregion
}
