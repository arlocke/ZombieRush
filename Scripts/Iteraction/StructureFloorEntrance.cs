using Godot;
using System;

public enum Direction
{
    Down,
    Up,
    Left,
    Right
}

public partial class StructureFloorEntrance : Area2D
{
    public Structure parentStructure;
    [Export]
    public int floor;
    [Export]
    public Direction direction;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        parentStructure = GetParent() as Structure;
        Connect("body_entered", new Callable(this, "OnBodyEntered"));
        Connect("body_exited", new Callable(this, "OnBodyExited"));
    }
    public virtual void OnBodyEntered(CollisionObject2D other)
    {
        if (other != null && other.IsInGroup("Players"))
        {
            parentStructure.EnterPlayer(other as Player, floor);
        }
    }
    public virtual void OnBodyExited(CollisionObject2D other)
    {
        if (other != null && other.IsInGroup("Players"))
        {
            if ((direction == Direction.Down && other.GlobalPosition.Y > GlobalPosition.Y) || (direction == Direction.Up && other.GlobalPosition.Y < GlobalPosition.Y) ||
                (direction == Direction.Right && other.GlobalPosition.X > GlobalPosition.X) || (direction == Direction.Left && other.GlobalPosition.X > GlobalPosition.X))
                parentStructure.ExitPlayer(other as Player, floor);
        }
    }
}
