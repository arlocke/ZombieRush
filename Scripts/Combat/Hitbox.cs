using Godot;
using System.Collections.Generic;

public class Hitbox:Area2D {
    public Creature origin;
    public List<Creature> hitCreatures = new List<Creature>();
    public float damage;
    public override void _Ready() {
        Connect("body_entered", this, "OnBodyEntered");
    }
    public virtual void OnBodyEntered(CollisionObject2D other) {
        if(other != null && other.IsInGroup("Creatures") && damage > 0) {
            Creature otherCreature = (Creature)other;
            if(otherCreature != null && !otherCreature.Friendly(origin) && !hitCreatures.Contains(otherCreature)) {
                float startingHp = otherCreature.hp;
                otherCreature.TakeDamage(damage); //Need to add team dynamic shit
                hitCreatures.Add(otherCreature);
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
    public void Disable() {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        hitCreatures.Clear();
    }
    public void Enable() {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
    }
}
