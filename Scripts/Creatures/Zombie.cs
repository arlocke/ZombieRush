using Godot;
using System.Collections.Generic;

public enum BehaviorState {
    Idle,
    Wandering,
    Chasing,
    Searching,
}

public class Zombie:Creature {
    [Export]
    public Creature targetCreature;
    [Export]
    public BehaviorState state;

    // Start is called before the first frame update
    public override void _Ready() {
        base._Ready();
    }

    public override void _PhysicsProcess(float dt) {
        switch(state) {
            case BehaviorState.Idle:
            //standing still jacking off, mashing head against wall etc...
            case BehaviorState.Wandering:
                //walking around n shit
                break;
            case BehaviorState.Chasing:
                if(targetCreature == null) {
                    state = BehaviorState.Wandering;
                    break;
                }
                navAgent.SetTargetLocation(targetCreature.Position);
                MoveOnPath(dt);
                break;
        }

    }
    public override void Alert(Creature other) {
        if(!Friendly(other)) {
            targetCreature = other;
            state = BehaviorState.Chasing;
        }
    }
}
