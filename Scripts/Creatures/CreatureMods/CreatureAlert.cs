using Godot;
using System;

public partial class CreatureAlert:Area2D {
    public override void _Ready() {
        Connect("body_entered", new Callable(this, "OnCreatureEntered"));
        ((AICreature)Owner).alertDst = (GetNode<CollisionShape2D>("CollisionShape2D").Shape as CircleShape2D).Radius;
    }
    public void OnCreatureEntered(Creature other) {
        if(other != null) {
            ((Creature)Owner).Alert(other);
        }
    }
}
