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

            // update client's player
            foreach (var kvp in playerVelocities) {
                RpcId(kvp.Key, nameof(Client_ReconciliatePlayer), playerPositions[kvp.Key], kvp.Value);
            }
        }
    }
    
    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}
    [Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_ReconciliatePlayer(Vector2 position, Vector2 velocity) {}

    [Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_Shoot(Vector2 velocityDirection) {
        var player = GetNode<Player>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.Shoot(velocityDirection);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_WeaponSwitch(int currentWeaponIndex) {
        var player = GetNode<Player>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.CurrentWeapon = player.Weapons[currentWeaponIndex];
    }

    [Rpc(RpcMode.AnyPeer)] void Server_Reload() {
        var player = GetNode<Player>(Global.WORLD_PATH + Multiplayer.GetRemoteSenderId().ToString());
        player.Reload();
    }

    #endregion
}