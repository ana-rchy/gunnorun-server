using System;
using System.Threading.Tasks;
using Godot;

public static class ExtensionMethods {
    public static async Task Sleep(this Node node, float time) {
        var msTime = time * 1000f;
        await Task.Delay((int) msTime);
    }
}