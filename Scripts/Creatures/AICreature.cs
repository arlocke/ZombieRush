using Godot;
using System.Collections.Generic;
public enum Action {
    UpdateChase
}
public struct Reaction {
    public Action act;
    public float timer;
    public Reaction(Action a, float t) {
        act = a;
        timer = t;
    }
}
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
public class AICreature:Creature {
    //AI
    [Export]
    public AICoordinator coordinator;
    [Export]
    public Creature targetCreature;
    public List<Creature> alertToCreatures = new List<Creature>();
    [Export]
    public BehaviorState state;
    [Export]
    public int intelligence = 1;
    public float reactionTime;
    public LinkedList<Reaction> reactions = new LinkedList<Reaction>();
    public float alertDst;
    //ATTACK
    [Export]
    public float meleeAttackDelay;
    [Export]
    public float meleeAttackRangeSqr;   //Distance Squared
    [Export]
    public float rangedAttackDelay;
    [Export]
    public float rangedAttackRangeSqr;
    public float attackTimer;
    Vector2[] pathPoints;

    public override void _Ready() {
        base._Ready();
        //Assuming dex cap for reaction time is 20. Randomize a little so that no zombie will have the same reaction time.
        reactionTime = (1 - (dexterity / 20)) + (float)GD.RandRange(0.1f, 0.3f);
        coordinator = Owner.GetNode<AICoordinator>("AICoordinator");
        Connect("path_changed",)
    }
    public override void _PhysicsProcess(float dt) {
        base._PhysicsProcess(dt);
        UpdateReactions(dt);
        switch(state) {
            case BehaviorState.Idle:
            //standing still jacking off, mashing head against wall etc...
            case BehaviorState.Wandering:
                //walking around n shit
                break;
            case BehaviorState.Chasing:
                if(TargetInRange()) {
                    StartMeleeAttack();
                }
                MoveOnPath(dt);
                break;
            case BehaviorState.Attacking:
                break;
        }
    }
    public void UpdateReactions(float dt) {
        if(reactions.Count == 0) return;
        Reaction r1 = reactions.First.Value;
        r1.timer -= dt;
        reactions.RemoveFirst();
        reactions.AddFirst(r1);
        while(reactions.Count > 0 && reactions.First.Value.timer <= 0) {
            TakeAction(reactions.First.Value.act);
            reactions.RemoveFirst();
        }
    }
    public override void Alert(Creature other) {
        if(IsInstanceValid(other) && !Friendly(other) && state != BehaviorState.DoNothing) {
            alertToCreatures.Add(other);
            if(!coordinator.directedCreatures.Contains(this))
                coordinator.directedCreatures.Add(this);
        }
    }
    public void UpdateTargets() {
        //Remove any targets that are invalid or too far away.
        for(int i = 0; i < alertToCreatures.Count;) {
            Creature c = alertToCreatures[i];
            if(!IsInstanceValid(c) || DstTaxi(c) > alertDst * 3f) {
                alertToCreatures.RemoveAt(i);
            } else
                i++;
        }
        //Find the closest target and chase them
        switch(alertToCreatures.Count) {
            case 0:
                Wander();
                break;
            case 1:
                if(alertToCreatures[0] != targetCreature)
                    coordinator.StartChase(this, alertToCreatures[0], targetCreature);
                break;
            default:
                Creature closestTarget = alertToCreatures[0];
                float closestDst = DstTaxi(closestTarget);
                for(int i = 1; i < alertToCreatures.Count; i++) {
                    Creature c = alertToCreatures[i];
                    float d = DstTaxi(c);
                    if(d < closestDst) {
                        closestDst = d;
                        closestTarget = c;
                    }
                }
                if(closestTarget != targetCreature)
                    coordinator.StartChase(this, closestTarget, targetCreature);
                break;
        }
    }
    public void Chase(Creature c) {
        targetCreature = c;
        SetState(BehaviorState.Chasing);
    }
    public void Wander() {
        SetState(BehaviorState.Wandering);
        coordinator.directedCreatures.Remove(this);
    }
    public void AddReaction(Action a) {
        reactions.AddLast(new Reaction(a, reactionTime - (reactions.Count > 0 ? reactions.First.Value.timer : 0)));
    }
    public void TakeAction(Action a) {

    }
    public bool TargetInRange() {
        return IsInstanceValid(targetCreature) && Position.DistanceSquaredTo(targetCreature.Position) < meleeAttackRangeSqr;
    }
    public void StartMeleeAttack() {
        SetState(BehaviorState.Attacking);
        animStateMachine.Travel("Attack");
    }
    public void FinishMeleeAttack() {
        if(IsInstanceValid(targetCreature) && Position.DistanceSquaredTo(targetCreature.Position) < meleeAttackRangeSqr) {
            targetCreature.TakeDamage(meleeAttackDamage);
        } else {
            SetState(BehaviorState.Chasing);
            animStateMachine.Travel("Idle");
        }
    }
    public void SetState(BehaviorState s) {
        state = s;
    }
    public override void _Draw() {
        base._Draw();
        Vector2[] p = navAgent.GetNavPath();
        if(p.Length > 1) {
            DrawPolyline(p, Colors.Red, 2);
        }
    }
    public void UpdatePathPoints() {

    }
}
