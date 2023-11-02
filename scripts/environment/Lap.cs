using System;
using System.Collections.Generic;
using Godot;

public partial class Lap : Node {
    [Export] int MaxLaps;
    public static Dictionary<long, byte> PlayersLapCounts { get; private set; } = new Dictionary<long, byte>();

    public override void _Ready() {
        LapPassed += GetNode<PlayerManager>(Global.SERVER_PATH + "PlayerManager")._OnLapPassed;
        PlayerWon += GetNode<MatchManager>(Global.SERVER_PATH + "MatchManager")._OnPlayerWon;

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
                //EmitSignal(SignalName.LapPassed, playerID);
                //GetNode<Checkpoints>("../Checkpoints").RefreshCheckpoints(playerID);

            } else {
                //GetNode<LevelTimer>(Global.WORLD_PATH + "LevelTimer").StopTimer();
                EmitSignal(SignalName.PlayerWon, playerID, LevelTimer.Time);
                //matchManager.Rpc(nameof(matchManager.Client_PlayerWon), id, levelTimer.Time);
            }

            EmitSignal(SignalName.LapPassed, PlayersLapCounts[playerID], MaxLaps);
            // var playerManager = GetNode<PlayerManager>(Global.SERVER_PATH + "PlayerManager");
            // playerManager.RpcId(playerID, nameof(playerManager.Client_LapChanged), PlayersLapCounts[playerID], MaxLaps);
        }
    }

    #endregion
}