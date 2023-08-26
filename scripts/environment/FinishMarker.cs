using Godot;
using System;

public partial class FinishMarker : Node {
	private void _OnPlayerEntered(Node2D player) {
		MatchManager matchManager = GetNode<MatchManager>(Global.SERVER_PATH + "MatchManager");
		long id = long.Parse(player.Name);
		LevelTimer levelTimer = GetNode<LevelTimer>(Global.WORLD_PATH + "LevelTimer");
		levelTimer.StopTimer();

		matchManager.Rpc("Client_PlayerWon", id, levelTimer.Time);
	}
}
