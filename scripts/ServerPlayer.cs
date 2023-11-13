using System;
using Godot;

public partial class ServerPlayer : CharacterBody2D {
    public Vector2 PuppetPosition { get; set; }
    double _timer;

    public override void _PhysicsProcess(double delta) {
        if (_timer >= Global.TICK_RATE) {
            var tween = CreateTween();
            tween.TweenProperty(this, "global_position", PuppetPosition, Global.TICK_RATE);
            _timer -= Global.TICK_RATE;
        }
        
        _timer += delta;
    }
}