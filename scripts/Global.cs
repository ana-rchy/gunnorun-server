using System;
using System.Collections.Generic;
using Godot;

public static class Global {
    public struct PlayerDataStruct {
        public PlayerDataStruct(string username, Color color) {
            Username = username;
            Color = color;

            ReadyStatus = false;
        }

        public string Username;
        public Color Color;
        public bool ReadyStatus;
    }

    public const int DEFAULT_PORT = 29999;
    public const int MAX_PEERS = 8;
    public const float TICK_RATE = 1 / 60f;
    public const string WORLD_PATH = "/root/World";
    public const string SERVER_PATH = "/root/Server";
    
    public static Dictionary<long, PlayerDataStruct> PlayersData;

    public static string GameState { get; set; } = "Lobby";
    public static string[] Worlds = new string[] { "Cave" }; // keep public so it can be used as an out param
    public static int WorldsIndex = 0;
}