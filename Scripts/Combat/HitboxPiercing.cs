using Godot;
using System;

public class HitboxPiercing:Hitbox {
    public override void OnBodyEntered(CollisionObject2D other) {
        if(other != null && other.IsInGroup("Creatures") && damage > 0) {
            Creature otherCreature = other as Creature;
            if(otherCreature != null && !otherCreature.Friendly(origin)) {
                float startingHp = otherCreature.hp;
                otherCreature.TakeDamage(damage); //Need to add team dynamic shit
                RemoveDamage(startingHp);
            }
        }
    }
}
