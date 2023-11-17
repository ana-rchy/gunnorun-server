using System;
using System.Collections.Generic;
using Godot;

public static class Paths {
    static Dictionary<string, string> NodePaths = new() {
        { "SERVER", "/root/Server" },
        { "WORLD", "/root/World" },
        { "LOBBY_MANAGER", "/root/Server/LobbyManager"},
        { "PLAYER_MANAGER", "/root/Server/PlayerManager" }
    };

    public static void AddNodePath(string name, string path) {
        NodePaths.TryAdd(name, path);
    }

    public static T GetNodeConst<T>(this Node node, string name) where T : Node {
        if (!NodePaths.ContainsKey(name)) {
            return null;
        }
        
        return node.GetNodeOrNull<T>(NodePaths[name]);
    }

    public static Node GetNodeConst(this Node node, string name) {
        return node.GetNodeConst<Node>(name);
    }

    public static string GetNodePath(string name) {
        return NodePaths[name];
    }
}