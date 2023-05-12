using Godot;
using System.Collections.Generic;

public enum TeamType {
    Zombies, //will have integer variable which will be team number, theoretically infinite team numbers per team type
    Humans,
}
public enum AnimType {
    SpriteSheet,
    Skeleton
}
public enum BodyPart {
    Head,
    Chest,
    Pelvis,
    ArmUpperL,
    ArmUpperR,
    ArmForeL,
    ArmForeR,
    HandL,
    HandR,
    ThighL,
    ThighR,
    ShinL,
    ShinR,
    FoorL,
    FootR
}

public partial class Creature:CharacterBody2D {

    //Stats
    [Export]
    public string charName;
    [Export]
    public float hp = 5;
    [Export]
    public float maxHp = 5;
    [Export]
    public int dexterity = 1;
    [Export]
    public float moveSpeed;
    [Export]
    public float meleeAttackDamage; //base melee damage when unequipped, or added to melee weapons 
    [Export]
    public float rangedAttackDamage; //base ranged damage added to ranged weapons
    public AnimationTree animTree;
    public AnimationPlayer animPlayer;
    public AnimationNodeStateMachinePlayback animStateMachine;
    public Node2D body;
    [Export]
    public Texture2D corpseTexture;
    [Export]
    public PackedScene corpseRef;
    public Vector2 movementDirection;
    public bool facingRight = true;

    //Team data
    [Export]
    public TeamType teamType;
    [Export]
    public int teamFaction; //faction inside of teamType, potentially refactor to string
    //Navigation
    [Export]
    public NavigationAgent2D navAgent;
    public List<Vector2> path;
    [Export]
    public AnimType animType = AnimType.SpriteSheet;
    public override void _Ready() {
        animTree = GetNode<AnimationTree>("AnimationTreeBody");
        animStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        animPlayer = GetNode<AnimationPlayer>("AnimationPlayerBody");
        body = GetNode<Node2D>("Body");
        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        navAgent.Connect("velocity_computed", new Callable(this, "Move"));
        navAgent.MaxSpeed = moveSpeed;
    }
    ///<returns> Whether or not the creature died from the damage </returns>
    public virtual bool TakeDamage(float takenDamage) {
        hp = Mathf.Clamp(hp - takenDamage, 0, maxHp);
        if(hp <= 0) {
            Die();
            return true;
        }
        return false;
    }
    public virtual void Heal(float healAmount) {
        hp = Mathf.Clamp(hp + healAmount, 0, maxHp);
    }
    public virtual void Die() {
        Node2D newCorpse = corpseRef.Instantiate<Corpse>();
        newCorpse.GetNode<Sprite2D>("Sprite2D").Texture = corpseTexture;
        newCorpse.Position = Position;
        if(!facingRight)
            newCorpse.Scale = new Vector2(-1, 1);
        GetParent().AddChild(newCorpse);
        QueueFree();
    }
    //Gets fired when this creature sees another creature
    public virtual void Alert(Creature other) {

    }
    public bool Friendly(Creature other) {
        return other != null && other.teamType == teamType && other.teamFaction == teamFaction;
    }
    public void Move(Vector2 vel) {
        if(!vel.IsZeroApprox()) {
            Velocity = vel;
            MoveAndSlide();
            if(vel.X != 0)
                facingRight = vel.X > 0;
        }
    }
    public void MoveOnPath(float dt) {
        if(!navAgent.IsNavigationFinished()) {
            movementDirection = GlobalPosition.DirectionTo(navAgent.GetNextPathPosition());
            Vector2 vel = movementDirection * moveSpeed;
            if(navAgent.AvoidanceEnabled)
                navAgent.SetVelocity(vel);
            else
                Move(vel);
        }
    }
    public virtual Rect2 GetSpriteRectWorld(Vector2 pos) {
        if(pos.IsEqualApprox(Vector2.Zero))
            pos = GlobalPosition;
        return new Rect2(GlobalPosition, GlobalPosition);
    }
    public float DstTaxi(Node2D other) {
        return Mathf.Abs(GlobalPosition.X - other.GlobalPosition.X) + Mathf.Abs(GlobalPosition.Y - other.GlobalPosition.Y);
    }
    public static bool CreatureListsAreEqual(List<Creature> l1, List<Creature> l2) {
        if(l1.Count != l2.Count)
            return false;
        foreach(Creature c in l1) {
            if(!l2.Contains(c))
                return false;
        }
        return true;
    }
}