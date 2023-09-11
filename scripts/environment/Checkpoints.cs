using System;
using System.Collections.Generic;
using Godot;

public partial class Checkpoints : Node {
    public override void _Ready() {
        var allCheckpoints = FindChildren("*", "Area2D");
        foreach (var playerID in Global.PlayersData.Keys) {
            var newPlayerData = Global.PlayersData[playerID];
            newPlayerData.UnpassedCheckpoints = allCheckpoints.Duplicate();
            Global.PlayersData[playerID] = newPlayerData;
        }

        foreach (Area2D checkpoint in allCheckpoints) {
            checkpoint.BodyEntered += (Node2D player) => Global.PlayersData[long.Parse(player.Name)].UnpassedCheckpoints.Remove(checkpoint);
        }
    }
}