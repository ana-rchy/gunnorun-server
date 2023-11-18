using System;
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

	string GetRandomWorld() {
		Random rand = new();
		string[] worlds = DirAccess.GetFilesAt(_worldDir);

		return worlds[rand.Next(worlds.Length)].Replace(".tscn", "");
	}

	void LoadWorld(string worldName) {
		if (this.GetNodeConst("WORLD") != null) {
			this.GetNodeConst("WORLD").Free();
		}

		var world = Load<PackedScene>($"{_worldDir}/{worldName}.tscn").Instantiate();
		GetNode("/root").AddChild(world);

		CallDeferred("emit_signal", SignalName.WorldLoaded); // add puppet players
	}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | rpc

	[Rpc] void Client_PlayerWon(long id, double time) {}
	[Rpc] void Client_LoadWorld(string worldPath) {}

	#endregion

	//---------------------------------------------------------------------------------//
	#region | signals

	[Signal] public delegate void WorldLoadedEventHandler();

	public void _OnPlayerWon(long id, float time) {
		_finishTimer.Start();
		Rpc(nameof(Client_PlayerWon), id, time);
	}

	void _OnGameStarted() {
		var world = Global.CurrentWorld == "Random" ? GetRandomWorld() : Global.CurrentWorld;

		LoadWorld(world);
		Rpc(nameof(Client_LoadWorld), world);
	}

	void _OnFinishTimeout() {
		var world = Global.CurrentWorld == "Random" ? GetRandomWorld() : Global.CurrentWorld;

		LoadWorld(world);
		Rpc(nameof(Client_LoadWorld), world);
	}

	#endregion
}
