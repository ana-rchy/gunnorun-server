using System;
using System.Collections.Generic;
using Godot;

public partial class Checkpoints : Node {
    public static Dictionary<long, List<Node>> PlayersUnpassedCheckpoints { get; private set; } = new Dictionary<long, List<Node>>();

    public override void _Ready() {
        var allCheckpoints = new List<Node>(FindChildren("*", "Area2D"));
        foreach (var playerID in Global.PlayersData.Keys) {
            PlayersUnpassedCheckpoints.TryAdd(playerID, allCheckpoints);
        }

        foreach (Area2D checkpoint in allCheckpoints) {
            checkpoint.BodyEntered += (Node2D player) =>
                PlayersUnpassedCheckpoints[long.Parse(player.Name)].Remove(checkpoint);
        }
    }

    //---------------------------------------------------------------------------------//
    #region | funcs

    void RefreshCheckpoints(long playerID) {
        PlayersUnpassedCheckpoints[playerID] = new List<Node>(FindChildren("*", "Area2D"));
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    void _OnLapPassed(long playerID, int lapCount, int maxLaps) {
        RefreshCheckpoints(playerID);
    }

    #endregion
}