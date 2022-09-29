using Godot;
using System.Collections.Generic;

public enum BehaviorState {
    Idle,
    Wandering,
    Chasing,
    Searching,
    Attacking,
    DoNothing,
}
public enum AttackType {
    Melee,
    Ranged,
}

public class Zombie:Creature {
    [Export]
    public Creature targetCreature;
    [Export]
    public BehaviorState state;
    [Export]
    public float meleeAttackDelay;
    [Export]
    public float meleeAttackRangeSqr;   //Distance Squared
    [Export]
    public float rangedAttackDelay;
    [Export]
    public float rangedAttackRangeSqr;
    public float attackTimer;

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
                if(!IsInstanceValid(targetCreature)) {
                    state = BehaviorState.Wandering;
                    break;
                }
                if(TargetInRange()) {
                    StartMeleeAttack();
                }
                navAgent.SetTargetLocation(targetCreature.Position);
                MoveOnPath(dt);
                break;
            case BehaviorState.Attacking:
                break;
        }

    }
    public override void Alert(Creature other) {
        if(IsInstanceValid(other) && !Friendly(other) && state != BehaviorState.DoNothing) {
            targetCreature = other;
            state = BehaviorState.Chasing;
        }
    }
    public bool TargetInRange() {
        return IsInstanceValid(targetCreature) && Position.DistanceSquaredTo(targetCreature.Position) < meleeAttackRangeSqr;
    }
    public void StartMeleeAttack() {
        state = BehaviorState.Attacking;
        animStateMachine.Travel("Attack");
    }
    public void FinishMeleeAttack() {
        if(IsInstanceValid(targetCreature) && Position.DistanceSquaredTo(targetCreature.Position) < meleeAttackRangeSqr) {
            targetCreature.TakeDamage(meleeAttackDamage);
        } else {
            state = BehaviorState.Chasing;
            animStateMachine.Travel("Idle");
        }
    }
}
