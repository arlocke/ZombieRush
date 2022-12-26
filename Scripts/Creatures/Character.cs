using Godot;
using System;

public struct ItemSlot {
    public Item handR;
    public Item handL;
    public ItemSlot(Item iR, Item iL) {
        handR = iR;
        handL = iL;
    }
    public override string ToString() {
        return (handR != null ? handR.ToString() : "Empty") + ", " + (handL != null ? handL.ToString() : "Empty");
    }
}

public class Character:Creature {

    //Animation Data
    [Export]
    public bool holding = false; //all this affects is idle animation
    [Export]
    public bool canMove;
    [Export]
    public bool canAim;
    [Export]
    public bool actionable; //Can't use any inputs
    public Vector2 lookDirR;
    public Vector2 lookDirL;

    //Children
    public Node2D shoulderR;
    public Node2D shoulderL;
    public Node2D armR;
    public Node2D armL;
    public Node2D handRSocket;
    public Node2D handLSocket;
    public Node2D face;
    public Sprite eyes;
    public AnimationTree eyesAnimTree;
    public AnimationNodeStateMachinePlayback eyesAnimStateMachine;
    public AnimationPlayer animPlayerArms;
    public Sprite bodySprite;

    //Movement
    public float movementVelocity; //not a vector
    public bool dashing;
    [Export]
    public float movementFriction;
    [Export]
    public float dashSpeed;
    [Export]
    public float maxCancelDashSpeed; //the fastest you can go before you can start walking again

    //Inventory
    public ItemSlotType activeSlot;
    public ItemSlotType lastActiveSlot;
    public Item[] inventory = new Item[9];
    [Export]
    public System.Collections.Generic.Dictionary<ItemSlotType, ItemSlot> activeItems = new System.Collections.Generic.Dictionary<ItemSlotType, ItemSlot>();


    public override void _Ready() {
        base._Ready();
    }

}
