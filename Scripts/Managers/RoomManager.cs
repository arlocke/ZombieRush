using Godot;
using System;

public class RoomManager : Node2D
{
    public override void _Ready()
    {
        LevelManager lm = GetTree().Root.GetNode<LevelManager>("LevelManager");
        lm.instancedRooms.Add(Filename, this);
    }
}
