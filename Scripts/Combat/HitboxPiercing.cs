using Godot;
using System;

public partial class HitboxPiercing:Hitbox {
    public override void OnBodyEntered(Node2D other) {
        if(IsInstanceValid(other) && other.IsInGroup("Creatures") && damage > 0) {
            Creature otherCreature = other as Creature;
            if(IsInstanceValid(otherCreature) && !otherCreature.Friendly(origin)) {
                float startingHp = otherCreature.hp;
                otherCreature.TakeDamage(damage); //Need to add team dynamic shit
                RemoveDamage(startingHp);
            }
        }
    }
}
