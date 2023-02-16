using Godot;
using Godot.Collections;
using System.Linq;
using System.Collections.Generic;

public class RoomExit : Area2D
{
    [Export(PropertyHint.File, "*.tscn")]
    string destination;
    RoomManager destinationLoaded;
    List<Player> overlappedPlayers = new List<Player>();
    public override void _Ready()
    {
        Connect("body_entered", this, "OnBodyEntered");
        Connect("body_exited", this, "OnBodyExited");
    }
    public virtual void OnBodyEntered(CollisionObject2D other)
    {
        if (other != null && other.IsInGroup("Players"))
        {
            Player p = (Player)other;
            if (!overlappedPlayers.Contains(p))
                overlappedPlayers.Add(p);
            if (Creature.CreatureListsAreEqual(p.cam.targets, overlappedPlayers.Cast<Creature>().ToList()))
            {
                LoadRoom(p);
            }
        }
    }
    public virtual void OnBodyExited(CollisionObject2D other)
    {
        if (other != null && other.IsInGroup("Players"))
        {
            Player p = (Player)other;
            overlappedPlayers.Remove(p);
        }
    }
    public void LoadRoom(Player p)
    {
        LevelManager lm = GetTree().Root.GetNode<LevelManager>("LevelManager");

        //Hide Visible Rooms
        foreach (KeyValuePair<string, RoomManager> roomPair in lm.instancedRooms)
        {
            if (roomPair.Key != destination)
            {       //If this room isn't the next room that is getting loaded, yeet it out of the tree hierarchy
                GetTree().Root.CallDeferred("remove_child", roomPair.Value);
            }
        }
        if (!IsInstanceValid(destinationLoaded))
        {
            if (lm.instancedRooms.ContainsKey(destination))
            { //Check if the room has already been instanced, but not by this exit
                destinationLoaded = lm.instancedRooms[destination];
            }
            else
            {
                destinationLoaded = ResourceLoader.Load<PackedScene>(destination).Instance() as RoomManager;
            }
        }
        GetTree().Root.CallDeferred("add_child", destinationLoaded);    //Add the destination room to the hierarchy to load it
        Node2D playersYSort = destinationLoaded.GetNode<Node2D>("Navigation2D/MasterYSort/Players");
        for (int i = 0; i < overlappedPlayers.Count; i++)
        {   //Move all of the players to the new destination room
            Player currPlayer = overlappedPlayers[i];
            if (!IsInstanceValid(currPlayer)) continue;
            currPlayer.GetParent().CallDeferred("remove_child", currPlayer);
            playersYSort.CallDeferred("add_child", currPlayer);
        }
    }
}
