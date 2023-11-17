using Godot;
using System;

public partial class World : Node {
    [Export] Timer _finishTimer;

    public override void _Ready() {
        Paths.AddNodePath("FINISH_TIMER", _finishTimer.GetPath());

        _finishTimer.Timeout += this.GetNodeConst<MatchManager>("MATCH_MANAGER")._OnFinishTimeout;
    }
}