using System;
using System.Collections.Generic;
using Godot;

public partial class StateMachine : Node {
	[Export] State _inLobby;

	public State CurrentState { get; private set; }

	public override void _Ready() {
		State.StateMachine = this;
		CurrentState = _inLobby;
	}

	public override void _Process(double _) {
		CurrentState.Update();
	}

	public void ChangeState(string state, Dictionary<string, object> message = null) {
		var stateNode = GetNodeOrNull<State>(state);
		if (stateNode == null) {
			Console.WriteLine($"server state {state} doesnt exist");
			return;
		}

		CurrentState = stateNode;
		CurrentState.Enter(message);
	}
}