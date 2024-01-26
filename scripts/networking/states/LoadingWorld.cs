using System;
using System.Collections.Generic;
using Godot;

public partial class LoadingWorld : State {
	[Export(PropertyHint.Dir)] string _worldDir;
	[Export(PropertyHint.File)] string _playerScene;

	public override void Enter(Dictionary<string, object> message) {
		var world = GD.Load<PackedScene>($"{_worldDir}/{message["world"]}.tscn").Instantiate();
		GetNode("/root").AddChild(world);
	}

	public override void Update(double _) {
		if (this.GetNodeConst("WORLD") != null) {
			AddServerPlayers();
			StateMachine.ChangeState("InGame");
		}
	}

	//---------------------------------------------------------------------------------//
    #region | funcs

	void AddServerPlayers() {
		foreach (var id in Global.PlayersData.Keys) {
			CreateNewServerPlayer(id);
		}
	}

	void CreateNewServerPlayer(long id) {
        var newPlayer = GD.Load<PackedScene>(_playerScene).Instantiate();

        newPlayer.Name = id.ToString();

        this.GetNodeConst("WORLD").CallDeferred("add_child", newPlayer);
    }

	#endregion
}