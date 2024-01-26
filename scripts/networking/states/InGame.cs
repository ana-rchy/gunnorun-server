using System;
using System.Collections.Generic;
using Godot;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using MsgPack.Serialization;

public partial class InGame : State {
	double _tickTimer;

    public override void Update(double delta) {
        _tickTimer = (_tickTimer + delta) % Global.TICK_RATE;

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

	//---------------------------------------------------------------------------------//
    #region | rpc

	[Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}

	[Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
        var player = GetNode<ServerPlayer>($"{Paths.GetNodePath("WORLD")}/{Multiplayer.GetRemoteSenderId()}");
        player.PuppetPosition = position;
    }

	#endregion
}