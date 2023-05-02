using Godot;
using System.Collections.Generic;

public partial class Hitbox:Area2D {
    public Creature origin;
    public List<Creature> hitCreatures = new List<Creature>();
    public float damage;
    public override void _Ready() {
        //Connect("body_entered", new Callable(this, "OnBodyEntered"));
        BodyEntered += OnBodyEntered;
    }
    public virtual void OnBodyEntered(Node2D other) {
        if(IsInstanceValid(other) && other.IsInGroup("Creatures") && damage > 0) {
            Creature otherCreature = (Creature)other;
            if(IsInstanceValid(otherCreature) && !otherCreature.Friendly(origin) && !hitCreatures.Contains(otherCreature)) {
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
        CollisionShape2D coll = GetNode<CollisionShape2D>("CollisionShape2D");
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        CollisionMask = 0;
        hitCreatures.Clear();
    }
    public void Enable() {
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        CollisionMask = 1;
    }
}
