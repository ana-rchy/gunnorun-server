using Godot;
using System;

public partial class MatchManager : Node {
	[Rpc] public void Client_PlayerWon(long id, double time) {}
}
