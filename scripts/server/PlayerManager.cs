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
                var player = GetNode<Node2D>(Global.WORLD_PATH + id);
                playerPositions.TryAdd(id, player.GlobalPosition);
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
    [Rpc] void Client_RemovePlayer(long id) {}

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<ServerPlayer>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.PuppetPosition = position;
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | funcs

    public void CreateNewServerPlayer(long id) {
        var newPlayer = Load<PackedScene>("res://scenes/player/Player.tscn").Instantiate();

        newPlayer.Name = id.ToString();

        GetNode(Global.WORLD_PATH).CallDeferred("add_child", newPlayer);
    }

    public void RemovePlayer(long id) {
        GetNode(Global.WORLD_PATH + id).QueueFree();

        Rpc(nameof(Client_RemovePlayer), id);
    }

    #endregion
}