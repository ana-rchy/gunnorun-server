using System;
using System.Collections.Generic;
using Godot;

public static class Global {
    public const int DEFAULT_PORT = 29999;
    public const int MAX_PEERS = 8;
    public const float TICK_RATE = 1 / 60f;
    public const string WORLD_PATH = "/root/World/";
    public const string SERVER_PATH = "/root/Server/";

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
    public static Dictionary<long, PlayerDataStruct> PlayersData;

    public static string GameState = "Lobby";
    public static string CurrentWorld = "Cave";
}