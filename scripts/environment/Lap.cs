using System;
using System.Collections.Generic;
using Godot;

public partial class Lap : Node {
    [Export] int _maxLaps;
    Dictionary<long, byte> _playersLapCounts = new Dictionary<long, byte>();

    public override void _Ready() {
        LapPassed += this.GetNodeConst<InGame>("IN_GAME_STATE")._OnLapPassed;
        PlayerWon += this.GetNodeConst<InGame>("IN_GAME_STATE")._OnPlayerWon;
        PlayerWon += this.GetNodeConst<LevelTimer>("LEVEL_TIMER")._OnPlayerWon;

        _playersLapCounts = new Dictionary<long, byte>();

        foreach (var playerID in Global.PlayersData.Keys) {
            _playersLapCounts.Add(playerID, 1);
        }
    }

    //---------------------------------------------------------------------------------//
    #region | signals

    [Signal] public delegate void LapPassedEventHandler(long playerID, int lapCount, int maxLaps);
    [Signal] public delegate void PlayerWonEventHandler(long id, float time);

    void _OnPlayerEntered(Node player) {
        var playerID = long.Parse(player.Name);

        if (Checkpoints.PlayersUnpassedCheckpoints[playerID].Count == 0) {
            if (_playersLapCounts[playerID] < _maxLaps) {
                _playersLapCounts[playerID]++;
            } else {
                EmitSignal(SignalName.PlayerWon, playerID, LevelTimer.Time);
            }

            EmitSignal(SignalName.LapPassed, playerID, _playersLapCounts[playerID], _maxLaps);
        }
    }

    #endregion
}
