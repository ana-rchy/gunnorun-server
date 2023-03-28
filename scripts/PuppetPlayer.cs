using Godot;
using System;
using static Godot.GD;

public partial class PuppetPlayer : CharacterBody2D {
    double Timer;
    public Vector2 PuppetPosition;

    public override void _PhysicsProcess(double delta) {
        if (Timer >= Global.TICK_RATE) {
            var tween = CreateTween();
            tween.TweenProperty(this, "global_position", PuppetPosition, Global.TICK_RATE);
            Timer -= Global.TICK_RATE;
        }
        
        Timer += delta;
    }
}