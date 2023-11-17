using System;
using System.Collections.Generic;
using Godot;
using static Godot.GD;

public partial class MatchManager : Node {
	[Export(PropertyHint.Dir)] string _worldDir;

	public override void _Ready() {
		Paths.AddNodePath("MATCH_MANAGER", GetPath());
	}

	//---------------------------------------------------------------------------------//
	#region | funcs

	void LoadWorld(string worldName) {
		var world = Load<PackedScene>($"{_worldDir}/{Global.Worlds[Global.WorldsIndex]}.tscn").Instantiate();
		GetNode("/root").CallDeferred("add_child", world);

		CallDeferred("emit_signal", SignalName.WorldLoaded);
	}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | rpc

	[Rpc] void Client_PlayerWon(long id, double time, string nextWorld) {}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | signals

	[Signal] public delegate void WorldLoadedEventHandler(long[] playerIDs);

	public void _OnPlayerWon(long id, float time) {
		this.GetNodeConst<Timer>("FINISH_TIMER").Start();
		Rpc(nameof(Client_PlayerWon), id, time, Global.Worlds);
	}

	public void _OnNewRace() {
		LoadWorld(Global.Worlds[Global.WorldsIndex % Global.Worlds.Length]);
		Global.WorldsIndex++;
	}

	#endregion
}
