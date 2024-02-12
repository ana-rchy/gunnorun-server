using System;
using System.Collections.Generic;
using Godot;
using static Godot.MultiplayerApi;
using MsgPack.Serialization;

public partial class InLobby : State {
    [Export(PropertyHint.Dir)] string _worldDir;

    public override void _Ready() {
        Multiplayer.PeerConnected += _OnPeerConnected;
    }

    //---------------------------------------------------------------------------------//
    #region | funcs

    // pure
    static bool CheckReadiness(IEnumerable<Global.PlayerDataStruct> playerData) {
        bool allReady = true;
        foreach (var player in playerData) {
            if (!player.ReadyStatus) {
                allReady = false;
            }
        }

        return allReady;
    }

    // state-unpure
    static string GetRandomWorld(string worldDir) {
	Random rand = new();
        string[] worlds = DirAccess.GetFilesAt(worldDir);

        return worlds[rand.Next(worlds.Length)].Replace(".tscn", "");
    }

    // side-effects
    void UpdatePlayerStatus(long playerID, bool ready) {
        var player = Global.PlayersData[playerID];
        player.ReadyStatus = ready;
        Global.PlayersData[playerID] = player;
    }    

    void StartGame() {
        EmitSignal(SignalName.GameStarted);

        var world = Global.CurrentWorld == "Random" ? GetRandomWorld(_worldDir) : Global.CurrentWorld;
        Rpc(nameof(Client_StartGame), world);

        StateMachine.ChangeState("LoadingWorld", new() {{ "world", world }} );
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc] void Client_Setup(byte[] serializedPlayerData) {}
    [Rpc] void Client_NewPlayer(long id, string username, Color color) {}
    [Rpc] void Client_UpdateStatus(long id, bool ready) {}
    [Rpc] void Client_StartGame(string worldName) {}

    [Rpc(RpcMode.AnyPeer)] void Server_NewPlayerData(string username, Color color) {
        Global.PlayersData.TryAdd(Multiplayer.GetRemoteSenderId(), new Global.PlayerDataStruct(username, color));
        Rpc(nameof(Client_NewPlayer), Multiplayer.GetRemoteSenderId(), username, color);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_UpdateStatus(bool ready) {
        UpdatePlayerStatus(Multiplayer.GetRemoteSenderId(), ready);

        if (!CheckReadiness(Global.PlayersData.Values)) {
            Rpc(nameof(Client_UpdateStatus), Multiplayer.GetRemoteSenderId(), ready);
        } else {
            StartGame();
        }
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    [Signal] public delegate void GameStartedEventHandler();

    void _OnPeerConnected(long id) {
        if (!IsActiveState()) return;

        var serializer = MessagePackSerializer.Get<Dictionary<long, Global.PlayerDataStruct>>();
        var serializedPlayerData = serializer.PackSingleObject(Global.PlayersData);

        RpcId(id, nameof(Client_Setup), serializedPlayerData);
    }

    #endregion
}
