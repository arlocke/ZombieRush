using Godot;
using System.Collections.Generic;

public class AutoPickup:Pickup {
    List<Player> overlappingPlayers = new List<Player>();
    public override void _PhysicsProcess(float dt) {
        base._PhysicsProcess(dt);
        foreach(Player p in overlappingPlayers) {
            if(Interact(p))
                return;
        }
    }
    public override void OnCreatureEntered(Creature other) {
        if(other != null && other.IsInGroup("Players")) {
            Interact((Player)other);
            overlappingPlayers.Add((Player)other);
        }
    }
    public override void OnCreatureExited(Creature other) {
        if(other != null && other.IsInGroup("Players")) {
            overlappingPlayers.Remove((Player)other);
        }
    }
}
