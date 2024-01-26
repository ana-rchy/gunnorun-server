using System;
using System.Collections.Generic;
using Godot;
using static Godot.MultiplayerApi;

public partial class InLobby : State {
    [Export(PropertyHint.Dir)] string _worldDir;

    //---------------------------------------------------------------------------------//
    #region | funcs

    // pure
    static bool CheckReadiness() {
        bool allReady = true;
        foreach (var player in Global.PlayersData.Values) {
            if (!player.ReadyStatus) {
                allReady = false;
            }
        }

        return allReady;
    }

    static string GetRandomWorld(string worldDir) {
		Random rand = new();
		string[] worlds = DirAccess.GetFilesAt(worldDir);

		return worlds[rand.Next(worlds.Length)].Replace(".tscn", "");
	}

    // unpure
    void UpdatePlayerStatus(long playerID, bool ready) {
        var player = Global.PlayersData[playerID];
        player.ReadyStatus = ready;
        Global.PlayersData[playerID] = player;
    }    

	void LoadWorld(string worldName) {
		if (this.GetNodeConst("WORLD") != null) {
			this.GetNodeConst("WORLD").Free();
		}

		worldName = worldName.Replace(".remap", "");

		var world = GD.Load<PackedScene>($"{_worldDir}/{worldName}.tscn").Instantiate();
		GetNode("/root").AddChild(world);
	}

    void StartGame() {
        EmitSignal(SignalName.GameStarted);
        Global.GameState = "Ingame";

        var world = Global.CurrentWorld == "Random" ? GetRandomWorld(_worldDir) : Global.CurrentWorld;
        Rpc(nameof(Client_StartGame), world);

        StateMachine.ChangeState("LoadingWorld", new() {{ "world", world }} );
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | rpc

    [Rpc] void Client_UpdateStatus(long id, bool ready) {}
    [Rpc] void Client_StartGame(string worldName) {}

    [Rpc(RpcMode.AnyPeer)] void Server_UpdateStatus(bool ready) {
        if (!IsActiveState()) return;

        UpdatePlayerStatus(Multiplayer.GetRemoteSenderId(), ready);

        if (!CheckReadiness()) {
            Rpc(nameof(Client_UpdateStatus), Multiplayer.GetRemoteSenderId(), ready);
        } else {
            StartGame();
        }
    }

    #endregion

    //---------------------------------------------------------------------------------//
    #region | signals

    [Signal] public delegate void GameStartedEventHandler();

    #endregion
}
