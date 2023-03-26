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

            foreach (var kvp in Server.PlayersData) {
                var id = kvp.Key;
                var position = GetNode<Node2D>(Global.WORLD_PATH + kvp.Key).GlobalPosition;
                playerPositions.TryAdd(id, position);
            }
            
            var serializer = MessagePackSerializer.Get<Dictionary<long, Vector2>>();
            byte[] playerPositionsSerialized = serializer.PackSingleObject(playerPositions);
            
            Rpc(nameof(Client_UpdatePuppetPositions), playerPositionsSerialized);
        }
    }
    
    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}

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