using Godot;
using System;

public partial class LevelTimer : Node {
    public double Time;

    public override void _Process(double delta) {
        Time += delta;
    }

    //---------------------------------------------------------------------------------//
    #region | funcs

    public void StopTimer() {
        SetProcess(false);
    }

    #endregion
}