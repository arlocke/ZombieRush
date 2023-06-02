using Godot;
using Godot.Collections;
using System.Linq;
using System.Collections.Generic;

public partial class RoomExit:Area2D {
    [Export(PropertyHint.File, "*.tscn")]
    public string destination;
    public RoomManager currentRoom;
    public RoomManager destinationLoaded;
    public List<Player> overlappedPlayers = new List<Player>();
    [Export]
    public bool locked;
    public override void _Ready() {
        Connect("body_entered", new Callable(this, "OnBodyEntered"));
        Connect("body_exited", new Callable(this, "OnBodyExited"));

        currentRoom = GetTree().GetFirstNodeInGroup("Rooms") as RoomManager;
    }
    public virtual void OnBodyEntered(CollisionObject2D other) {
        if(other != null && other.IsInGroup("Players")) {
            Player p = (Player)other;
            if(overlappedPlayers.Contains(p))
                return;
            else
                overlappedPlayers.Add(p);
            if(!locked && !currentRoom.locked && Creature.CreatureListsAreEqual(p.cam.targets, overlappedPlayers.Cast<Creature>().ToList())) {
                LoadRoom(p);
            }
        }
    }
    public virtual void OnBodyExited(CollisionObject2D other) {
        if(other != null && other.IsInGroup("Players")) {
            Player p = (Player)other;
            overlappedPlayers.Remove(p);
        }
    }
    public void LoadRoom(Player p) {
        LevelManager lm = GetTree().Root.GetNode<LevelManager>("LevelManager");
        int thisExitSortingIndex = 0;

        Array<Node> currExits = GetTree().GetNodesInGroup("Exits");
        List<RoomExit> currExitsToDest = new List<RoomExit>();
        string originRoomPath = (GetTree().GetNodesInGroup("Rooms")[0] as RoomManager).SceneFilePath;
        foreach(Node n in currExits) {   //Find all exits that go to the same destination room as this one
            RoomExit e = n as RoomExit;
            if(e.destination == destination)
                currExitsToDest.Add(e);
        }
        if(currExitsToDest.Count > 1) {  //If there are multiple exits going to the same room, figure out this exit's position relative to the others
            thisExitSortingIndex = SortExitsByPos(currExitsToDest).IndexOf(this);
        }

        //Hide instanced rooms that aren't the destination
        foreach(KeyValuePair<string, RoomManager> roomPair in lm.instancedRooms) {
            if(roomPair.Key != destination) {       //If this room isn't the next room that is getting loaded, yeet it out of the tree hierarchy
                GetTree().Root.CallDeferred("remove_child", roomPair.Value);
            }
        }
        if(!IsInstanceValid(destinationLoaded)) {
            if(lm.instancedRooms.ContainsKey(destination)) { //Check if the room has already been instanced, but not by this exit
                destinationLoaded = lm.instancedRooms[destination];
            } else {
                destinationLoaded = ResourceLoader.Load<PackedScene>(destination).Instantiate() as RoomManager;
            }
        }
        GetTree().Root.CallDeferred("add_child", destinationLoaded);    //Add the destination room to the hierarchy to load it
        Node2D playersYSort = destinationLoaded.GetNode<Node2D>("Navigation2D/MasterYSort/Players");

        for(int i = 0; i < overlappedPlayers.Count; i++) {   //Move all of the players to the new destination room
            Player currPlayer = overlappedPlayers[i];
            if(!IsInstanceValid(currPlayer)) continue;
            currPlayer.GetParent().CallDeferred("remove_child", currPlayer);
            playersYSort.CallDeferred("add_child", currPlayer);
            currPlayer.CallDeferred("MoveToExit", originRoomPath, thisExitSortingIndex);
        }
        overlappedPlayers.Clear();
    }

    public static List<RoomExit> SortExitsByPos(List<RoomExit> l) {
        List<RoomExit> newList = new List<RoomExit>(l);
        newList.Sort(delegate (RoomExit a, RoomExit b) {
            int comp = a.Position.X.CompareTo(b.Position.X);
            return (comp == 0 ? a.Position.Y.CompareTo(b.Position.Y) : comp);
        });
        return newList;
    }
}
