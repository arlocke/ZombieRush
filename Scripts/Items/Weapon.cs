using Godot;
using System;

public class Weapon:Item {
    [Export]
    public float damage;
    [Export]
    public float attackRate;
    public AnimationTree animTree;
    public AnimationNodeStateMachinePlayback animStateMachine;
    public override void _Ready() {
        base._Ready();
        animTree = GetNode<AnimationTree>("AnimationTree");
        animStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");
        animTree.Set("parameters/ActionBack/TimeScale/scale", Mathf.Max(1 / attackRate, 5));
        animTree.Set("parameters/ActionForward/TimeScale/scale", Mathf.Max(1 / attackRate, 5));
    }
}
