using System;
using System.Collections.Generic;
using Godot;
using static Godot.MultiplayerApi;
using static Godot.MultiplayerPeer;
using MsgPack.Serialization;

public partial class InGame : State {
	[Export(PropertyHint.Dir)] string _worldDir;
	[Export] Timer _finishTimer;

	double _tickTimer;

	public override void _Ready() {
		Paths.AddNodePath("IN_GAME_STATE", GetPath());
	}

    public override void Update(double delta) {
        _tickTimer = (_tickTimer + delta) % Global.TICK_RATE;

		Dictionary<long, Vector2> playerPositions = new Dictionary<long, Vector2>();

		foreach (var kvp in Global.PlayersData) {
			var id = kvp.Key;
			var player = GetNodeOrNull<Node2D>($"{Paths.GetNodePath("WORLD")}/{id}");
			if (player != null) {
				playerPositions.TryAdd(id, player.GlobalPosition);
			}
		}
		
		// update puppets
		var serializer = MessagePackSerializer.Get<Dictionary<long, Vector2>>();
		byte[] playerPositionsSerialized = serializer.PackSingleObject(playerPositions);
		Rpc(nameof(Client_UpdatePuppetPositions), playerPositionsSerialized);
    }

	//---------------------------------------------------------------------------------//
    #region | funcs

	string GetRandomWorld() {
		Random rand = new();
		string[] worlds = DirAccess.GetFilesAt(_worldDir);

		return worlds[rand.Next(worlds.Length)].Replace(".tscn", "");
	}

	#endregion

	//---------------------------------------------------------------------------------//
    #region | rpc

	[Rpc(TransferMode = TransferModeEnum.UnreliableOrdered)] void Client_UpdatePuppetPositions(byte[] puppetPositionsSerialized) {}
	[Rpc] void Client_WeaponShot(long id, string name, float rotation, float range) {}
    [Rpc] void Client_Intangibility(float time) {}
    [Rpc] void Client_PlayerHPChanged(long id, int newHP) {}
	[Rpc] void Client_PlayerFrameChanged(long id, int frame) {}
    [Rpc] void Client_PlayerOnGround(long id, bool onGround, float xVel) {}
	[Rpc] void Client_LapChanged(int lap, int maxLaps) {}

	[Rpc] void Client_PlayerWon(long id, double time) {}
	[Rpc] void Client_LoadWorld(string worldPath) {}

	[Rpc(RpcMode.AnyPeer, TransferMode = TransferModeEnum.UnreliableOrdered)] void Server_UpdatePlayerPosition(Vector2 position) {
		if (!IsActiveState()) return;
        
        var player = GetNode<ServerPlayer>($"{Paths.GetNodePath("WORLD")}/{Multiplayer.GetRemoteSenderId()}");
        player.PuppetPosition = position;
    }

	[Rpc(RpcMode.AnyPeer)] void Server_WeaponShot(string name, float rotation, float range) {
		if (!IsActiveState()) return;
        
        Rpc(nameof(Client_WeaponShot), Multiplayer.GetRemoteSenderId(), name, rotation, range);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_Intangibility(long id, float time) {
		if (!IsActiveState()) return;
        
        RpcId(id, nameof(Client_Intangibility), time);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerHPChanged(long id, int newHP) {
		if (!IsActiveState()) return;
        
        Rpc(nameof(Client_PlayerHPChanged), id, newHP);
    }

	[Rpc(RpcMode.AnyPeer)] void Server_PlayerFrameChanged(int frame) {
		if (!IsActiveState()) return;
        
        Rpc(nameof(Client_PlayerFrameChanged), Multiplayer.GetRemoteSenderId(), frame);
    }

    [Rpc(RpcMode.AnyPeer)] void Server_PlayerOnGround(bool onGround, float xVel) {
		if (!IsActiveState()) return;
        
        Rpc(nameof(Client_PlayerOnGround), Multiplayer.GetRemoteSenderId(), onGround, xVel);
    }

	#endregion

	//---------------------------------------------------------------------------------//
    #region | signals

	public void _OnLapPassed(long playerID, int lapCount, int maxLaps) {
		if (!IsActiveState()) return;
        
        RpcId(playerID, nameof(Client_LapChanged), lapCount, maxLaps);
    }


	public void _OnPlayerWon(long id, float time) {
		_finishTimer.Start();
		Rpc(nameof(Client_PlayerWon), id, time);
	}

	void _OnFinishTimeout() {
		var world = Global.CurrentWorld == "Random" ? GetRandomWorld() : Global.CurrentWorld;

		Rpc(nameof(Client_LoadWorld), world);
		StateMachine.ChangeState("LoadingWorld", new() {{ "world", world }} );
	}

	#endregion
}