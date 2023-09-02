using Godot;
using static Godot.GD;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using System;
using System.Collections.Generic;
using MsgPack.Serialization;

public partial class PlayerManager : Node {
    //---------------------------------------------------------------------------------//
    #region | tick rate loop

    double TickTimer;
    public override void _Process(double delta) {
        TickTimer += delta;

        if (TickTimer > Global.TICK_RATE) {
            TickTimer -= Global.TICK_RATE;
            Dictionary<long, Vector2> playerPositions = new Dictionary<long, Vector2>();

            foreach (var kvp in Global.PlayersData) {
                var id = kvp.Key;
                var player = GetNodeOrNull<Node2D>(Global.WORLD_PATH + id);
                if (player != null) playerPositions.TryAdd(id, player.GlobalPosition);
            }
            
            // update puppets
            var serializer = MessagePackSerializer.Get<Dictionary<long, Vector2>>();
            byte[] playerPositionsSerialized = serializer.PackSingleObject(playerPositions);
            Rpc(nameof(Client_UpdatePuppetPositions), playerPositionsSerialized);
        }
    }
    
    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}
    [Rpc] void Client_PlayerHit(long id, int damage) {}
    [Rpc] void Client_TracerShot(long id, float rotation, float range) {}
    [Rpc] void Client_PlayerFrameChanged(long id, sbyte direction, byte frame) {}

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<ServerPlayer>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.PuppetPosition = position;
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerHit(long id, int damage) {
        Rpc(nameof(Client_PlayerHit), id, damage);
    }
    
    [Rpc(RpcMode.AnyPeer)] void Server_TracerShot(float rotation, float range) {
        Rpc(nameof(Client_TracerShot), Multiplayer.GetRemoteSenderId(), rotation, range);
    }

    [Rpc(RpcMode.AnyPeer)] public void Server_PlayerFrameChanged(sbyte direction, byte frame) {
        Rpc(nameof(Client_PlayerFrameChanged), Multiplayer.GetRemoteSenderId(), direction, frame);
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | funcs

    public void CreateNewServerPlayer(long id) {
        var newPlayer = Load<PackedScene>("res://scenes/player/Player.tscn").Instantiate();

        newPlayer.Name = id.ToString();

        GetNode(Global.WORLD_PATH).CallDeferred("add_child", newPlayer);
    }

    #endregion
}