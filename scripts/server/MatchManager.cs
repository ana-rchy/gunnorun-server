using System;
using Godot;
using static Godot.GD;

public partial class MatchManager : Node {
	//---------------------------------------------------------------------------------//
	#region | rpc

	[Rpc] void Client_PlayerWon(long id, double time) {}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | signals

	[Signal] public delegate void WorldLoadedEventHandler(long[] playerIDs);

	void _OnGameStarted(string worldName, long[] playerIDs) {
		Global.CurrentWorld = worldName;
		var worldScene = Load<PackedScene>($"res://scenes/worlds/{Global.CurrentWorld}.tscn").Instantiate();
		GetNode("/root").CallDeferred("add_child", worldScene);

		CallDeferred("emit_signal", SignalName.WorldLoaded, playerIDs);
	}

	public void _OnPlayerWon(long id, float time) {
		Rpc(nameof(Client_PlayerWon), id, time);
	}

	#endregion
}
