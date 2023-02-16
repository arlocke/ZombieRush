using Godot;
using System.Collections.Generic;

public class LevelManager : Node
{
    public Dictionary<string, RoomManager> instancedRooms = new Dictionary<string, RoomManager>();
}
