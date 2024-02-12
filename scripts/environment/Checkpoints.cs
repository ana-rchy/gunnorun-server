using System;
using System.Collections.Generic;
using Godot;

public partial class Checkpoints : Node {
    public static Dictionary<long, List<Node>> PlayersUnpassedCheckpoints { get; private set; }

    public override void _Ready() {
        PlayersUnpassedCheckpoints = new(); // needed to reset the dictionary every map

        var allCheckpoints = GetAllCheckpoints();
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

    // state-unpure (dependent on world)
    List<Node> GetAllCheckpoints() {
        return new List<Node>(FindChildren("*", "Area2D"));
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    void _OnLapPassed(long playerID, int lapCount, int maxLaps) {
        PlayersUnpassedCheckpoints[playerID] = GetAllCheckpoints();
    }

    #endregion
}
