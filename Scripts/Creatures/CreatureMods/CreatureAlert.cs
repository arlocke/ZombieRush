using Godot;
using System;

public class CreatureAlert:Area2D {
    public override void _Ready() {
        Connect("body_entered", this, "OnCreatureEntered");
    }
    public void OnCreatureEntered(Creature other) {
        if(other != null) {
            ((Creature)Owner).Alert(other);
        }
    }
}
