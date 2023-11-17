using System;
using System.Collections.Generic;
using Godot;
using static Godot.MultiplayerApi;

public partial class LobbyManager : Node {
    //---------------------------------------------------------------------------------//
    #region | funcs

    bool CheckReadiness() {
        bool allReady = true;
        foreach (var player in Global.PlayersData.Values) {
            if (!player.ReadyStatus) allReady = false;
        }

        if (allReady) {
            StartGame();
        }

        return allReady;
    }
    
    void StartGame() {
        EmitSignal(SignalName.GameStarted, Global.Worlds[Global.WorldsIndex],
                new List<long>(Global.PlayersData.Keys).ToArray());

        Global.GameState = "Ingame";
        Multiplayer.MultiplayerPeer.RefuseNewConnections = true;

        Rpc(nameof(Client_StartGame), Global.Worlds[Global.WorldsIndex % Global.Worlds.Length]);
        Global.WorldsIndex++;
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc] void Client_UpdateStatus(long id, bool ready) {}
    [Rpc] void Client_StartGame(string worldName) {}

    [Rpc(RpcMode.AnyPeer)] void Server_UpdateStatus(bool ready) {
        var player = Global.PlayersData[Multiplayer.GetRemoteSenderId()];
        player.ReadyStatus = ready;
        Global.PlayersData[Multiplayer.GetRemoteSenderId()] = player;

        if (!CheckReadiness()) {
            Rpc(nameof(Client_UpdateStatus), Multiplayer.GetRemoteSenderId(), ready);
        }
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    [Signal] public delegate void GameStartedEventHandler(string worldName, long[] playerIDs);

    #endregion
}
