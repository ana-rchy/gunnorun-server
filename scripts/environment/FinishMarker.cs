using Godot;
using System;

public partial class FinishMarker : Node {
    public override void _Ready() {
        PlayerWon += this.GetNodeConst<InGame>("IN_GAME_STATE")._OnPlayerWon;
        PlayerWon += this.GetNodeConst<LevelTimer>("LEVEL_TIMER")._OnPlayerWon;
    }

    //---------------------------------------------------------------------------------//
    #region | signals

    [Signal] public delegate void PlayerWonEventHandler(long id, float time);

    void _OnPlayerEntered(Node2D player) {
        if (Checkpoints.PlayersUnpassedCheckpoints[long.Parse(player.Name)].Count == 0) {
            EmitSignal(SignalName.PlayerWon, long.Parse(player.Name), LevelTimer.Time);
        }
    }

    #endregion
}
