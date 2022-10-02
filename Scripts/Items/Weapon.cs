using Godot;
using System;

public class Weapon:Node2D {
    [Export]
    public string itemName;
    public bool facingRight = true;
    [Export]
    public int tier;
    [Export]
    public float damage;
    [Export]
    public float attackRate;
    public AnimationTree animTree;
    public AnimationNodeStateMachinePlayback animStateMachine;
    public Node2D hand1Socket;
    public Node2D hand2Socket;
    [Export]
    public float itemSize = 1;
    public Creature holder;
    public override void _Ready() {
        animTree = GetNode<AnimationTree>("AnimationTree");
        animStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        animTree.Set("parameters/ActionBack/TimeScale/scale", Mathf.Max(1 / attackRate, 5));
        animTree.Set("parameters/ActionForward/TimeScale/scale", Mathf.Max(1 / attackRate, 5));
        SetFacingRight(true);
        hand1Socket = GetNode<Node2D>("Hand1Socket");
        hand2Socket = GetNode<Node2D>("Hand2Socket");

    }
    public virtual void SetFacingRight(bool r) {
        if(r != facingRight) {
            facingRight = r;
            ShowBehindParent = facingRight;
            //ZIndex = facingRight ? -1 : 1;
            //animTree.Set("parameters/Idle/BlendSpace1D/blend_position", facingRight ? 1 : -1);
        }
    }
    public virtual void Attack() {

    }
    public virtual void ReleaseAttack() {

    }
}
