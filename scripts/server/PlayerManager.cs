using Godot;
using static Godot.MultiplayerApi;
using System;

public partial class PlayerManager : Node {
    [Rpc(RpcMode.AnyPeer)] void RPC_HandleShoot(Vector2 velocityDirection) {
        var player = GetNode<Player>("/root/World/" + Multiplayer.GetRemoteSenderId().ToString());
        player.SetVelocity(velocityDirection);
    }

    [Rpc(RpcMode.AnyPeer)] void RPC_HandleWeaponSwitch(int currentWeaponIndex) {
        var player = GetNode<Player>("/root/World/" + Multiplayer.GetRemoteSenderId().ToString());
        player.CurrentWeapon = player.Weapons[currentWeaponIndex];
    }

    [Rpc(RpcMode.AnyPeer)] void RPC_HandleReload() {
        // TODO: figure out how tf to anti-tamper reload/refires
    }
}