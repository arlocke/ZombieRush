using Godot;
using System;

public enum RoomType {
    Passthrough,
    Locked,
    PVP,
}

public partial class RoomManager:Node2D {
    [Export]
    public RoomType type;
    [Export]
    public bool locked = false;

    public override void _Ready() {
        LevelManager lm = GetTree().Root.GetNode<LevelManager>("LevelManager");
        lm.instancedRooms.Add(SceneFilePath, this);

        switch(type) {
            case RoomType.Locked:
                locked = true;
                Godot.Collections.Array<Node> zombs = GetTree().GetNodesInGroup("Zombies");
                foreach(Node n in zombs) {
                    if(IsInstanceValid(n) && n is Creature) {
                        (n as Creature).CreatureDied += CheckForRoomUnlock;
                    }
                }
                break;
            case RoomType.PVP:
                locked = true;
                break;
        }
    }
    public void CheckForRoomUnlock(Creature c) {
        switch(type) {
            case RoomType.Locked:
                //Check if there are any other enemies alive
                Godot.Collections.Array<Node> zombs = GetTree().GetNodesInGroup("Zombies");
                foreach(Node n in zombs) {
                    if(IsInstanceValid(n) && n is Creature && n != c) {
                        return;
                    }
                }
                locked = false;
                break;
            case RoomType.PVP:
                //Check if there are any other enemies alive
                int survivingFaction = -1;
                Godot.Collections.Array<Node> players = GetTree().GetNodesInGroup("Players");
                foreach(Node n in players) {
                    Creature currentCreature = n as Creature;
                    if(IsInstanceValid(currentCreature) && n != c) {
                        if(survivingFaction == -1) {
                            survivingFaction = currentCreature.teamFaction;
                        } else if(currentCreature.teamFaction != survivingFaction) {  //If the surviving faction is already set and the current creature is in a different one, there are two teams still alive
                            return;
                        }
                    }
                }
                locked = false;
                break;
        }
    }
    public void SetupPlayerForRoom(Player p) {
        if(IsInstanceValid(p) && type == RoomType.PVP) {
            p.teamFaction = p.playerNum;
            p.CreatureDied += CheckForRoomUnlock;
        }
    }
}
