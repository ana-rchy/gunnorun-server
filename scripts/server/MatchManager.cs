using Godot;
using System;

public partial class MatchManager : Node {
	[Rpc] void Client_PlayerWon(long id, double time) {}
}
