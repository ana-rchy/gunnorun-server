using System;
using System.Collections.Generic;
using Godot;
using static Godot.GD;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using MsgPack.Serialization;

public partial class PlayerManager : Node {
    double TickTimer;
    public override void _Process(double delta) {
        TickTimer += delta;

        if (TickTimer > Global.TICK_RATE) {
            TickTimer -= Global.TICK_RATE;
            Dictionary<long, Vector2> playerPositions = new Dictionary<long, Vector2>();

            foreach (var kvp in Global.PlayersData) {
                var id = kvp.Key;
                var player = GetNodeOrNull<Node2D>(Global.WORLD_PATH + id);
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
        var newPlayer = Load<PackedScene>("res://scenes/player/Player.tscn").Instantiate();

        newPlayer.Name = id.ToString();

        GetNode(Global.WORLD_PATH).CallDeferred("add_child", newPlayer);
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}
    [Rpc] void Client_TracerShot(long id, float rotation, float range) {}
    [Rpc] void Client_Intangibility(float time) {}
    [Rpc] void Client_PlayerHPChanged(long id, int newHP) {}
    [Rpc] void Client_PlayerFrameChanged(long id, int frame) {}
    [Rpc] void Client_LapChanged(int lap, int maxLaps) {}

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<ServerPlayer>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.PuppetPosition = position;
    }

    [Rpc(RpcMode.AnyPeer)] void Server_TracerShot(float rotation, float range) {
        Rpc(nameof(Client_TracerShot), Multiplayer.GetRemoteSenderId(), rotation, range);
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

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    void _OnWorldLoaded(long[] playerIDs) {
        foreach (var id in playerIDs) {
            CreateNewServerPlayer(id);
        }
    }

    public void _OnLapPassed(long playerID, int lapCount, int maxLaps) {
        RpcId(playerID, nameof(Client_LapChanged), lapCount, maxLaps);
    }

    #endregion
}