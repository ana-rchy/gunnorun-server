using System;
using System.Collections.Generic;
using Godot;

public partial class Lap : Node {
    [Export] int MaxLaps;
    public static Dictionary<long, byte> PlayersLapCounts = new Dictionary<long, byte>();

    public override void _Ready() {
        foreach (var playerID in Global.PlayersData.Keys) {
            PlayersLapCounts.Add(playerID, 1);
        }
    }

    void _OnPlayerEntered(Node player) {
        var playerID = long.Parse(player.Name);

        if (Checkpoints.PlayersUnpassedCheckpoints[playerID].Count == 0) {
            if (PlayersLapCounts[playerID] < MaxLaps) {
                PlayersLapCounts[playerID]++;
                GetNode<Checkpoints>("../Checkpoints").RefreshCheckpoints(playerID);

            } else {
                var matchManager = GetNode<MatchManager>(Global.SERVER_PATH + "MatchManager");
                var id = long.Parse(player.Name);
                var levelTimer = GetNode<LevelTimer>(Global.WORLD_PATH + "LevelTimer");
                levelTimer.StopTimer();

                matchManager.Rpc(nameof(matchManager.Client_PlayerWon), id, levelTimer.Time);
            }

            var playerManager = GetNode<PlayerManager>(Global.SERVER_PATH + "PlayerManager");
            playerManager.RpcId(playerID, nameof(playerManager.Client_LapChanged), PlayersLapCounts[playerID], MaxLaps);
        }
    }
}