using Godot;
using System;

public partial class FinishMarker : Node {
	public override void _Ready() {
		PlayerWon += GetNode<MatchManager>(Global.SERVER_PATH + "MatchManager")._OnPlayerWon;
	}

	//---------------------------------------------------------------------------------//
    #region | signals

	[Signal] public delegate void PlayerWonEventHandler(long id, float time);

	void _OnPlayerEntered(Node2D player) {
		if (Checkpoints.PlayersUnpassedCheckpoints[long.Parse(player.Name)].Count == 0) {
			//var matchManager = GetNode<MatchManager>(Global.SERVER_PATH + "MatchManager");
			//GetNode<LevelTimer>(Global.WORLD_PATH + "LevelTimer").StopTimer();
			
			EmitSignal(SignalName.PlayerWon, long.Parse(player.Name), LevelTimer.Time);
			//matchManager.Rpc(nameof(matchManager.Client_PlayerWon), id, levelTimer.Time);
		}
	}

	#endregion
}
