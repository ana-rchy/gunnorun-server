using Godot;
using static Godot.MultiplayerApi;
using System;
using System.Collections.Generic;

public partial class PlayerManager : Node {
    const float TICK_RATE = 1 / 60f;

    Server Server;

    public override void _Ready() {
        Server = GetNode<Server>("/root/Server");
    }

    //---------------------------------------------------------------------------------//
    #region | tick rate loop

    double TickTimer;
    public override void _Process(double delta) {
        TickTimer += delta;

        if (TickTimer > TICK_RATE) {
            TickTimer = 0f;
            Dictionary<long, Vector2> playerPositions;

            foreach (var kvp in Server.PlayersData) {
                var id = kvp.Key;
                var position = GetNode<Node2D>("/root/world/" + kvp.Key).GlobalPosition;
                playerPositions.TryAdd(id, position);
            }
            
            Rpc(nameof(Client_UpdatePuppetPositions), new Godot.Collections.Dictionary(playerPositions));
        }
    }
    
    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc] void Client_UpdatePuppetPositions(Godot.Collections.Dictionary<long, Vector2> puppetPositions) {}

    [Rpc(RpcMode.AnyPeer)] void Server_Shoot(Vector2 velocityDirection) {
        var player = GetNode<Player>("/root/World/" + Multiplayer.GetRemoteSenderId().ToString());
        player.Shoot(velocityDirection);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_WeaponSwitch(int currentWeaponIndex) {
        var player = GetNode<Player>("/root/World/" + Multiplayer.GetRemoteSenderId().ToString());
        player.CurrentWeapon = player.Weapons[currentWeaponIndex];
    }

    [Rpc(RpcMode.AnyPeer)] void Server_Reload() {
        var player = GetNode<Player>("/root/World/" + Multiplayer.GetRemoteSenderId().ToString());
        player.Reload();
    }

    #endregion
}