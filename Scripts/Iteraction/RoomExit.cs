using Godot;
using Godot.Collections;
using System.Linq;
using System.Collections.Generic;

public class RoomExit : Area2D
{
    [Export]
    PackedScene destination;
    Node2D destinationLoaded;
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
        // for (int i = 0; i < overlappedPlayers.Count; i++)
        // {
        //     overlappedPlayers[i].GetParent().RemoveChild(overlappedPlayers[i]);
        // }
        //Hide Visible Rooms
        Array visibleRooms = GetTree().GetNodesInGroup("Rooms");
        foreach (Node2D r in visibleRooms)
        {
            if (r.Visible)
                r.Visible = false;
            r.PropagateCall("set_disabled", new Array(true), true);
        }

        if (!IsInstanceValid(destinationLoaded))
        {
            destinationLoaded = destination.Instance() as Node2D;
            GetTree().Root.AddChild(destinationLoaded);
        }
        destinationLoaded.Visible = true;
        destinationLoaded.PropagateCall("set_disabled", new Array(false), true);
        Node2D playersYSort = destinationLoaded.GetNode<Node2D>("Navigation2D/MasterYSort/Players");
        for (int i = 0; i < overlappedPlayers.Count; i++)
        {
            Player currPlayer = overlappedPlayers[i];
            if (!IsInstanceValid(currPlayer)) continue;
            currPlayer.GetParent().CallDeferred("remove_child", currPlayer);
            playersYSort.CallDeferred("add_child", currPlayer);
            currPlayer.Visible = true;
        }
    }
}
