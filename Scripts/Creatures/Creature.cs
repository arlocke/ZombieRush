using Godot;
using System.Collections.Generic;

public enum TeamType {
    Zombies, //will have integer variable which will be team number, theoretically infinite team numbers per team type
    Humans,
}

public class Creature:KinematicBody2D {     //Stats
    [Export]
    public float hp = 5;
    [Export]
    public float maxHp = 5;
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
    public Sprite bodySprite;
    [Export]
    public Texture corpseTexture;
    [Export]
    public PackedScene corpseRef;
    public bool facingRight;

    //Team data
    [Export]
    public TeamType teamType;
    [Export]
    public int teamFaction; //faction inside of teamType, potentially refactor to string
    //Navigation
    [Export]
    public NavigationAgent2D navAgent;
    public List<Vector2> path;
    public override void _Ready() {
        animTree = GetNode<AnimationTree>("AnimationTreeBody");
        animStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        animPlayer = GetNode<AnimationPlayer>("AnimationPlayerBody");
        body = GetNode<Node2D>("Body");
        bodySprite = GetNode<Sprite>("Body/BodySprite");
        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        navAgent.Connect("velocity_computed", this, "Move");
        navAgent.MaxSpeed = moveSpeed;
    }
    public virtual void TakeDamage(float takenDamage) {
        hp = Mathf.Clamp(hp - takenDamage, 0, maxHp);
        if(hp <= 0) {
            Die();
        }
    }
    public virtual void Heal(float healAmount) {
        hp = Mathf.Clamp(hp + healAmount, 0, maxHp);
    }
    public virtual void Die() {
        Node2D newCorpse = corpseRef.Instance<Corpse>();
        newCorpse.GetNode<Sprite>("Sprite").Texture = corpseTexture;
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
        if(vel != null) {
            MoveAndSlide(vel);
        }
    }
    public void MoveOnPath(float dt) {
        if(!navAgent.IsNavigationFinished()) {
            Vector2 moveDir = GlobalPosition.DirectionTo(navAgent.GetNextLocation());
            Vector2 vel = moveDir * moveSpeed;
            navAgent.SetVelocity(vel);
        }
    }

    public Rect2 GetSpriteRectWorld(Vector2 pos) {
        if(pos.IsEqualApprox(Vector2.Zero))
            pos = GlobalPosition;
        return new Rect2(pos - bodySprite.GetRect().Size / 2 + new Vector2(0, bodySprite.Offset.y), bodySprite.GetRect().Size);
    }
}