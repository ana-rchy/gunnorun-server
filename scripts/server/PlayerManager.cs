using System;
using System.Collections.Generic;
using Godot;
using static Godot.GD;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using MsgPack.Serialization;

public partial class PlayerManager : Node {
    [Export(PropertyHint.File)] string _playerScene;

    public override void _Ready() {
        Paths.AddNodePath("PLAYER_MANAGER", GetPath());
    }

    double _tickTimer;
    public override void _Process(double delta) {
        _tickTimer += delta;

        if (_tickTimer > Global.TICK_RATE) {
            _tickTimer -= Global.TICK_RATE;
            Dictionary<long, Vector2> playerPositions = new Dictionary<long, Vector2>();

            foreach (var kvp in Global.PlayersData) {
                var id = kvp.Key;
                var player = GetNodeOrNull<Node2D>($"{Paths.GetNodePath("WORLD")}/{id}");
                if (player != null) {
                    playerPositions.TryAdd(id, player.GlobalPosition);
                }
            }
            
            // update puppets
            var serializer = MessagePackSerializer.Get<Dictionary<long, Vector2>>();
            byte[] playerPositionsSerialized = serializer.PackSingleObject(playerPositions);
            Rpc(nameof(Client_UpdatePuppetPositions), playerPositionsSerialized);
        }
    }

    //---------------------------------------------------------------------------------//
    #region | funcs

    void CreateNewServerPlayer(long id) {
        var newPlayer = Load<PackedScene>(_playerScene).Instantiate();

        newPlayer.Name = id.ToString();

        this.GetNodeConst("WORLD").CallDeferred("add_child", newPlayer);
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}
    [Rpc] void Client_WeaponShot(long id, string name, float rotation, float range) {}
    [Rpc] void Client_Intangibility(float time) {}
    [Rpc] void Client_PlayerHPChanged(long id, int newHP) {}
    [Rpc] void Client_PlayerFrameChanged(long id, int frame) {}
    [Rpc] void Client_LapChanged(int lap, int maxLaps) {}
    [Rpc] void Client_PlayerOnGround(long id, bool onGround, float xVel) {}

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<ServerPlayer>($"{Paths.GetNodePath("WORLD")}/{Multiplayer.GetRemoteSenderId()}");
        player.PuppetPosition = position;
    }

    [Rpc(RpcMode.AnyPeer)] void Server_WeaponShot(string name, float rotation, float range) {
        Rpc(nameof(Client_WeaponShot), Multiplayer.GetRemoteSenderId(), name, rotation, range);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_Intangibility(long id, float time) {
        RpcId(id, nameof(Client_Intangibility), time);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerHPChanged(long id, int newHP) {
        Rpc(nameof(Client_PlayerHPChanged), id, newHP);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerFrameChanged(int frame) {
        Rpc(nameof(Client_PlayerFrameChanged), Multiplayer.GetRemoteSenderId(), frame);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerOnGround(bool onGround, float xVel) {
        Rpc(nameof(Client_PlayerOnGround), Multiplayer.GetRemoteSenderId(), onGround, xVel);
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    public void _OnLapPassed(long playerID, int lapCount, int maxLaps) {
        RpcId(playerID, nameof(Client_LapChanged), lapCount, maxLaps);
    }

    void _OnWorldLoaded() {
        foreach (var id in Global.PlayersData.Keys) {
            CreateNewServerPlayer(id);
        }
    }

    #endregion
}