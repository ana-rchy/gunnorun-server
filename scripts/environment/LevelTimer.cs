using Godot;
using System;

public partial class LevelTimer : Node {
    public static double Time { get; private set; }

    public override void _Ready() {
        Time = 0;
    }

    public override void _Process(double delta) {
        Time += delta;
    }

    //---------------------------------------------------------------------------------//
    #region | signals

    public void _OnPlayerWon(long id, float time) {
        SetProcess(false);
        Time = 0;
    }

    #endregion
}