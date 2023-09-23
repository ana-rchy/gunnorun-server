using System;
using System.Collections.Generic;
using Godot;

public partial class Checkpoints : Node {
    public override void _Ready() {
        var allCheckpoints = FindChildren("*", "Area2D");
        foreach (var playerID in Global.PlayersData.Keys) {
            Global.PlayersUnpassedCheckpoints.Add(playerID, allCheckpoints);
        }

        foreach (Area2D checkpoint in allCheckpoints) {
            checkpoint.BodyEntered += (Node2D player) => Global.PlayersUnpassedCheckpoints[long.Parse(player.Name)].Remove(checkpoint);
        }
    }
}