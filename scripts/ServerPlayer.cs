using System;
using Godot;

public partial class ServerPlayer : CharacterBody2D {
    double Timer;
    public Vector2 PuppetPosition { get; set; }

    public override void _PhysicsProcess(double delta) {
        if (Timer >= Global.TICK_RATE) {
            var tween = CreateTween();
            tween.TweenProperty(this, "global_position", PuppetPosition, Global.TICK_RATE);
            Timer -= Global.TICK_RATE;
        }
        
        Timer += delta;
    }
}