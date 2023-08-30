using Godot;
using System;

public partial class FinishMarker : Node {
	private void _OnPlayerEntered(Node2D player) {
		var matchManager = GetNode<MatchManager>(Global.SERVER_PATH + "MatchManager");
		var id = long.Parse(player.Name);
		var levelTimer = GetNode<LevelTimer>(Global.WORLD_PATH + "LevelTimer");
		levelTimer.StopTimer();

		matchManager.Rpc(nameof(matchManager.Client_PlayerWon), id, levelTimer.Time);
	}
}