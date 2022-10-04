using Godot;
using System;

public enum ItemSlotType {
    Primary,
    Secondary,
    Utility
}
public class Item:Node2D {
    [Export]
    public string itemName;
    [Export]
    public float itemSize = 1;
    [Export]
    public int tier;
    [Export]
    public ItemSlotType slotType;
    public Creature holder;
    public bool facingRight = true;
    public Node2D hand1Socket;
    public Node2D hand2Socket;

    public override void _Ready() {
        SetFacingRight(true);
        hand1Socket = GetNode<Node2D>("Hand1Socket");
        hand2Socket = GetNode<Node2D>("Hand2Socket");
    }
    public virtual void SetFacingRight(bool r) {
        if(r != facingRight) {
            facingRight = r;
            ShowBehindParent = facingRight;
        }
    }
    public virtual void Use() {         //Shoot for guns, attack for melee, use for utility

    }
    public virtual void CancelUse() {

    }
    public virtual void AltUse() {      //Zoom for primary guns

    }
    public virtual void CancelAltUse() {

    }
    public virtual void Custom() {

    }
}
