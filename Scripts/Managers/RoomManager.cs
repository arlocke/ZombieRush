using Godot;
using System;

public partial class RoomManager:Node2D {
    public override void _Ready() {
        LevelManager lm = GetTree().Root.GetNode<LevelManager>("LevelManager");
        lm.instancedRooms.Add(SceneFilePath, this);
    }
}
