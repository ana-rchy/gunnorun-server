using Godot;
using System;
using System.Threading.Tasks;

public static class ExtensionMethods {
    public static async Task Sleep(this Node node, float time) {
        await node.ToSignal(node.GetTree().CreateTimer(time), "timeout");
    }
}