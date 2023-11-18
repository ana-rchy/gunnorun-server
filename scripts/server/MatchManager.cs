using System;
using System.Collections.Generic;
using Godot;
using static Godot.GD;

public partial class MatchManager : Node {
	[Export(PropertyHint.Dir)] string _worldDir;
	[Export] Timer _finishTimer;

	public override void _Ready() {
		Paths.AddNodePath("MATCH_MANAGER", GetPath());
	}

	//---------------------------------------------------------------------------------//
	#region | funcs

	void LoadWorld(string worldName) {
		var world = Load<PackedScene>($"{_worldDir}/{worldName}.tscn").Instantiate();
		GetNode("/root").CallDeferred("add_child", world);

		CallDeferred("emit_signal", SignalName.WorldLoaded); // add puppet players
	}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | rpc

	[Rpc] void Client_PlayerWon(long id, double time) {}
	[Rpc] void Client_LoadWorld(string worldName) {}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | signals

	[Signal] public delegate void WorldLoadedEventHandler();

	public void _OnGameStarted() {
		var worldName = Global.Worlds[Global.WorldsIndex % Global.Worlds.Length];
		Global.WorldsIndex++;

		LoadWorld(worldName);
		Rpc(nameof(Client_LoadWorld), worldName);
	}

	public void _OnPlayerWon(long id, float time) {
		_finishTimer.Start();
		Rpc(nameof(Client_PlayerWon), id, time);
	}

	public void _OnFinishTimeout() {
		var worldName = Global.Worlds[Global.WorldsIndex % Global.Worlds.Length];
		Global.WorldsIndex++;

		LoadWorld(worldName);
		Rpc(nameof(Client_LoadWorld), worldName);
	}

	#endregion
}
