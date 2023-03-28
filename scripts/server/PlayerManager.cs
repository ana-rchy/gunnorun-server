using Godot;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using System;
using System.Collections.Generic;
using MsgPack.Serialization;

public partial class PlayerManager : Node {
    Server Server;

    public override void _Ready() {
        Server = GetNode<Server>("/root/Server");
    }

    //---------------------------------------------------------------------------------//
    #region | tick rate loop

    double TickTimer;
    public override void _Process(double delta) {
        TickTimer += delta;

        if (TickTimer > Global.TICK_RATE) {
            TickTimer = 0f;
            Dictionary<long, Vector2> playerPositions = new Dictionary<long, Vector2>();
            Dictionary<long, Vector2> playerVelocities = new Dictionary<long, Vector2>();

            foreach (var kvp in Server.PlayersData) {
                var id = kvp.Key;
                var player = GetNode<RigidBody2D>(Global.WORLD_PATH + kvp.Key);
                playerPositions.TryAdd(id, player.GlobalPosition);
                playerVelocities.TryAdd(id, player.LinearVelocity);
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

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<PuppetPlayer>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.PuppetPosition = position;
    }

    #endregion
}