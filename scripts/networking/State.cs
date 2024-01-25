using System;
using System.Collections.Generic;
using Godot;

public partial class State : Node {
	public static StateMachine StateMachine { protected get; set; }

	public virtual void Enter(Dictionary<string, object> message = null) {}
	public virtual void Update() {}

	protected bool IsActiveState() {
		return StateMachine.CurrentState == this;
	}
}