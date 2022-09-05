using Godot;
using System;

public class Hitbox:Area2D {
    public Creature origin;
    public float damage;
    public override void _Ready() {
        Connect("body_entered", this, "OnBodyEntered");
    }
    public void OnBodyEntered(CollisionObject2D other) {
        if(other != null && other.IsInGroup("Creatures") && damage > 0) {
            Creature otherCreature = (Creature)other;
            if(otherCreature != null && !otherCreature.Friendly(origin)) {
                float startingHp = otherCreature.hp;
                otherCreature.TakeDamage(damage); //Need to add team dynamic shit
                RemoveDamage(startingHp);
            }
        }
    }
    public void RemoveDamage(float dmg) {
        damage -= dmg;
        if(damage <= 0) {
            QueueFree();
        }
    }

    public void AddDamage(float dmg) {

    }
}
