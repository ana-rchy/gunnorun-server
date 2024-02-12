using System;
using System.Collections.Generic;
using Godot;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using MsgPack.Serialization;

public partial class InGame : State {
    [Export(PropertyHint.Dir)] string _worldDir;
    [Export] Timer _finishTimer;

    public override void _Ready() {
        Paths.AddNodePath("IN_GAME_STATE", GetPath());

        Multiplayer.PeerDisconnected += _OnPeerDisconnected;
    }

    public override void Enter(Dictionary<string, object> message = null) {
        Multiplayer.MultiplayerPeer.RefuseNewConnections = true;
    }

    public override void Update(double delta) {
        SendPlayerPositions(GetPlayerPositions());
    }

    //---------------------------------------------------------------------------------//
    #region | funcs

    // state-unpure
    Dictionary<long, Vector2> GetPlayerPositions() {
        Dictionary<long, Vector2> playerPositions = new();

        foreach (var kvp in Global.PlayersData) {
            var id = kvp.Key;
            var player = GetNodeOrNull<Node2D>($"{Paths.GetNodePath("WORLD")}/{id}");
            if (player != null) {
                playerPositions.TryAdd(id, player.GlobalPosition);
            }
        }

        return playerPositions;
    }

    string GetRandomWorld() {
        Random rand = new();
        string[] worlds = DirAccess.GetFilesAt(_worldDir);

        return worlds[rand.Next(worlds.Length)].Replace(".tscn", "");
    }

    // side-effects
    void SendPlayerPositions(Dictionary<long, Vector2> playerPositions) {
        var serializer = MessagePackSerializer.Get<Dictionary<long, Vector2>>();
        byte[] playerPositionsSerialized = serializer.PackSingleObject(playerPositions);
        Rpc(nameof(Client_UpdatePuppetPositions), playerPositionsSerialized);
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}
    [Rpc] void Client_WeaponShot(long id, string name, float rotation, float range) {}
    [Rpc] void Client_Intangibility(long id, float time) {}
    [Rpc] void Client_PlayerHPChanged(long id, int newHP) {}
    [Rpc] void Client_PlayerFrameChanged(long id, int frame) {}
    [Rpc] void Client_LapChanged(int lap, int maxLaps) {}

    [Rpc] void Client_PlayerWon(long id, double time) {}
    [Rpc] void Client_LoadWorld(string worldPath) {}

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<ServerPlayer>($"{Paths.GetNodePath("WORLD")}/{Multiplayer.GetRemoteSenderId()}");
        player.PuppetPosition = position;
    }

    [Rpc(RpcMode.AnyPeer)] void Server_WeaponShot(string name, float rotation, float range) {
        Rpc(nameof(Client_WeaponShot), Multiplayer.GetRemoteSenderId(), name, rotation, range);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_Intangibility(long id, float time) {
        Rpc(nameof(Client_Intangibility), id, time);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerHPChanged(long id, int newHP) {
        Rpc(nameof(Client_PlayerHPChanged), id, newHP);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerFrameChanged(int frame) {
        Rpc(nameof(Client_PlayerFrameChanged), Multiplayer.GetRemoteSenderId(), frame);
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    public void _OnLapPassed(long playerID, int lapCount, int maxLaps) {
        RpcId(playerID, nameof(Client_LapChanged), lapCount, maxLaps);
    }


    public void _OnPlayerWon(long id, float time) {
        _finishTimer.Start();
        Rpc(nameof(Client_PlayerWon), id, time);
    }

    void _OnPeerDisconnected(long id) {
        if (!IsActiveState()) return;

        GetNode($"{Paths.GetNodePath("WORLD")}/{id}").QueueFree();

        if (Multiplayer.GetPeers().Length == 0) {
            var world = this.GetNodeConst("WORLD");
            if (world != null) {
                world.QueueFree();
            }

            Multiplayer.MultiplayerPeer.RefuseNewConnections = false;
            StateMachine.ChangeState("InLobby");
        }
    }

    void _OnFinishTimeout() {
        var world = Global.CurrentWorld == "Random" ? GetRandomWorld() : Global.CurrentWorld;

        Rpc(nameof(Client_LoadWorld), world);
        StateMachine.ChangeState("LoadingWorld", new() {{ "world", world }} );
    }

    #endregion
}
