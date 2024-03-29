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
public partial class AICreature:Creature {
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
    [Export]
    public float stateTimer;
    [Export]
    public float moveSpeedWalk;
    //COSMETIC
    [Export]
    public Godot.Collections.Dictionary<NodePath, Godot.Collections.Array<NodePath>> cosmeticPartDependents = new Godot.Collections.Dictionary<NodePath, Godot.Collections.Array<NodePath>>();

    public override void _Ready() {
        base._Ready();
        //Assuming dex cap for reaction time is 20. Randomize a little so that no zombie will have the same reaction time.
        reactionTime = (1 - (dexterity / 20)) + (float)GD.RandRange(0.1f, 0.3f);
        coordinator = Owner.GetNode<AICoordinator>("AICoordinator");
        animTree.Set("parameters/Idle/TimeSeek/seek_request", GD.Randf());
        RandomizeBodyVariations();
    }
    public override void _PhysicsProcess(double dt) {
        base._PhysicsProcess(dt);
        UpdateReactions((float)dt);
        switch(state) {
            case BehaviorState.Idle:
                //standing still jacking off, mashing head against wall etc...
                stateTimer -= (float)dt;
                animStateMachine.Travel("Idle");
                if(stateTimer < 0)
                    Wander();
                break;
            case BehaviorState.Wandering:
                animStateMachine.Travel("Move");
                MoveWander((float)dt);
                break;
            case BehaviorState.Chasing:
                animStateMachine.Travel("Move");
                if(!IsInstanceValid(targetCreature)) {
                    UpdateTargets();
                }
                if(TargetInRange()) {
                    StartMeleeAttack();
                }
                MoveOnPath((float)dt);
                break;
            case BehaviorState.Attacking:
                break;
        }
        body.Scale = new Vector2(facingRight ? 1 : -1, 1);
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
        movementDirection = GD.Randf() > 0.5f ? (GD.Randf() > 0.5f ? new Vector2(0, 1) : new Vector2(0, -1)) : (GD.Randf() > 0.5f ? new Vector2(1, 0) : new Vector2(-1, 0));
        stateTimer = (float)GD.RandRange(1, 5);
    }
    public void MoveWander(float dt) {
        stateTimer -= dt;
        if(movementDirection.X != 0)
            facingRight = movementDirection.X > 0;
        if(MoveAndCollide(movementDirection * moveSpeedWalk * dt) != null || stateTimer < 0) {
            if(GD.Randf() > 0.5f) {  //Continue wandering
                movementDirection = movementDirection.Rotated((Mathf.Pi / 2) * Mathf.Ceil((float)GD.RandRange(0, 3)));
                stateTimer = (float)GD.RandRange(1, 5);
            } else {                  //Idle
                SetState(BehaviorState.Idle);
                stateTimer = (float)GD.RandRange(1, 5);
            }
        }
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
        animStateMachine.Start("Attack");
    }
    public void FinishMeleeAttack() {
        if(IsInstanceValid(targetCreature) && Position.DistanceSquaredTo(targetCreature.Position) < meleeAttackRangeSqr) {
            if(targetCreature.TakeDamage(meleeAttackDamage)) {
                UpdateTargets();
            }
        } else {
            SetState(BehaviorState.Chasing);
            animStateMachine.Travel("Idle");
        }
    }
    public void SetState(BehaviorState s) {
        switch(s) {
            case BehaviorState.Chasing:
                animTree.Set("parameters/Move/TimeScale/scale", moveSpeed / moveSpeedWalk);
                break;
            case BehaviorState.Wandering:
                animTree.Set("parameters/Move/TimeScale/scale", 1);
                break;
        }
        state = s;
    }
    public void RandomizeBodyVariations() {
        if(animType == AnimType.Skeleton) {
            Godot.Collections.Array<Node> spriteNodes = body.GetNode<Node2D>("Sprites").GetChildren();
            foreach(NodePath np in cosmeticPartDependents.Keys) {
                if(np.ToString().Contains("Chest") || np.ToString().Contains("Pelvis"))
                    RandomizeDependentPartVariations(np, null, 2, spriteNodes);
            }
            foreach(Node n in spriteNodes) {
                if(n is Sprite2D) {
                    Sprite2D s = n as Sprite2D;
                    if(s.RegionEnabled) {
                        s.RegionRect = new Rect2(
                            new Vector2(s.RegionRect.Position.X + (GD.Randi() % 3) * (s.RegionRect.Size.X + 1),
                                        s.RegionRect.Position.Y + (s.Name == "Head" ? (GD.Randi() % 2) * (s.RegionRect.Size.Y + 1) : 0)),
                        s.RegionRect.Size);
                    }
                }
            }
        }
    }
    public void RandomizeDependentPartVariations(NodePath np, Sprite2D prereq, uint prereqChoice, Godot.Collections.Array<Node> spriteNodes) {
        Sprite2D s = GetNode(np) as Sprite2D;
        if(IsInstanceValid(s) && s.RegionEnabled) {
            uint xChoice;
            if(IsInstanceValid(prereq) && (prereq.Name == "Chest" || prereq.Name == "Pelvis")) {
                xChoice = GD.Randi() % 2 == 0 ? 0 : prereqChoice;
            } else {
                xChoice = GD.Randi() % (prereqChoice + 1);
            }
            s.RegionRect = new Rect2(
                new Vector2(s.RegionRect.Position.X + xChoice * (s.RegionRect.Size.X + 1),
                            s.RegionRect.Position.Y + (s.Name == "Head" ? (GD.Randi() % 2) * (s.RegionRect.Size.Y + 1) : 0)),
            s.RegionRect.Size);
            spriteNodes.Remove(s);
            if(cosmeticPartDependents.ContainsKey(np)) {
                foreach(NodePath depNP in cosmeticPartDependents[np]) {
                    RandomizeDependentPartVariations(depNP, s, xChoice, spriteNodes);
                }
                //cosmeticPartDependents.Remove(np);
            }
        }
    }
}
