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
    public float moveSpeed;
    public float meleeAttack; //base melee damage when unequipped, or added to melee weapons 
    public float rangedAttack; //base ranged damage added to ranged weapons
    public AnimationTree animTree;
    public AnimationPlayer animPlayer;
    public AnimationNodeStateMachinePlayback animStateMachine;
    public Node2D body;
    public Sprite bodySprite;
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
        animTree = GetNode<AnimationTree>("AnimationTree");
        animStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        body = GetNode<Node2D>("Body");
        bodySprite = GetNode<Sprite>("Body/BodySprite");
        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        navAgent.Connect("velocity_computed", this, "Move");
        navAgent.MaxSpeed = moveSpeed;
    }
    public void TakeDamage(float takenDamage) {
        hp -= takenDamage;
        if(hp <= 0) {
            Die();
        }
    }
    public void Die() {
        //play death anim, destroy gameobject, spawn corpse sprite
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
}