using System;
using System.Collections.Generic;
using Godot;

public partial class Lap : Node {
    [Export] int MaxLaps;
    public static Dictionary<long, byte> PlayersLapCounts { get; private set; } = new Dictionary<long, byte>();

    public override void _Ready() {
        LapPassed += GetNode<PlayerManager>($"{Global.SERVER_PATH}/PlayerManager")._OnLapPassed;
        PlayerWon += GetNode<MatchManager>($"{Global.SERVER_PATH}/MatchManager")._OnPlayerWon;

        foreach (var playerID in Global.PlayersData.Keys) {
            PlayersLapCounts.Add(playerID, 1);
        }
    }

    //---------------------------------------------------------------------------------//
    #region | signals

    [Signal] public delegate void LapPassedEventHandler(long playerID, int lapCount, int maxLaps);
    [Signal] public delegate void PlayerWonEventHandler(long id, float time);

    void _OnPlayerEntered(Node player) {
        var playerID = long.Parse(player.Name);

        if (Checkpoints.PlayersUnpassedCheckpoints[playerID].Count == 0) {
            if (PlayersLapCounts[playerID] < MaxLaps) {
                PlayersLapCounts[playerID]++;
            } else {
                EmitSignal(SignalName.PlayerWon, playerID, LevelTimer.Time);
            }

            EmitSignal(SignalName.LapPassed, PlayersLapCounts[playerID], MaxLaps);
        }
    }

    #endregion
}