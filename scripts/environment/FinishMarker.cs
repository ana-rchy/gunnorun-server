using Godot;
using System;

public partial class FinishMarker : Node {
	private void _OnPlayerEntered(Node2D player) {
		GetNode<MatchManager>("/root/Server/MatchManager").Rpc("Client_PlayerWon", long.Parse(player.Name));
	}
}
