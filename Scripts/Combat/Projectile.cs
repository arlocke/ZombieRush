using Godot;
using System;

public enum AmmoType {
    Cal9mm,
    Cal762,
    Cal50ae,
    //more types for zombie sludge etc
}

public class Projectile:HitboxPiercing {
    [Export]
    public float speed;
    public float distanceRemaining;
    public AmmoType ammoType;

    public override void _PhysicsProcess(float dt) {
        Position += Transform.x * speed * dt;
        distanceRemaining -= speed;

        if(distanceRemaining <= 0) {
            QueueFree();
        }
    }
}
