using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class Player:Creature {
    //Interaction Data
    public List<Interactable> currentInteractables = new List<Interactable>();
    public Interactable closestInteractable;
    public Label interactableText;

    //Animation Data
    [Export]
    public bool holding = false; //all this affects is idle animation

    //Movement variables
    //public float moveSpeed = 1f;
    public float collisionOffset = 0.05f; // Distance from rigidbody to check for collisions

    //Children
    public Sprite arm;
    public Node2D hand1Socket;
    public Gun heldGun;

    //Inventory
    public Godot.Collections.Dictionary<AmmoType, int> heldAmmo;
    [Export]
    public Godot.Collections.Dictionary<AmmoType, int> maxAmmo;
    public List<int> heldAmmoList;
    [Export]
    Vector2 movementInput;

    //Prefabs
    [Export]
    public PackedScene pickupGunRef;

    // Start is called before the first frame update
    public override void _Ready() {
        base._Ready();
        heldAmmo = new Godot.Collections.Dictionary<AmmoType, int>() {
            { AmmoType.Cal50ae, 10 },
            { AmmoType.Cal762, 100 },
            { AmmoType.Cal9mm, 30 },
        };
        maxAmmo = new Godot.Collections.Dictionary<AmmoType, int>() {
            { AmmoType.Cal50ae, 50 },
            { AmmoType.Cal762, 100 },
            { AmmoType.Cal9mm, 150 },
        };

        heldAmmoList = new List<int>(heldAmmo.Values);
        arm = GetNode<Sprite>("Body/Arm");
        hand1Socket = arm.GetNode<Node2D>("Hand1Socket");
        interactableText = GetNode<Label>("InteractableText");
        holding = heldGun != null;
    }

    public override void _PhysicsProcess(float dt) {
        bool walking = false;
        if(movementInput != Vector2.Zero) {
            walking = MoveAndSlide(movementInput * moveSpeed) != Vector2.Zero;

            //Changing direction of walk anim when unequipped
            if(!holding && movementInput.x != 0) {
                facingRight = movementInput.x > 0;
                bodySprite.FlipH = !facingRight;
            }
        }
        string animState = walking ? "Walk" : "Idle";
        animStateMachine.Travel(animState);
        animTree.Set("parameters/" + animState + "/blend_position", new Vector2(facingRight ? 1 : -1, holding ? 1 : 0));
        arm.Visible = holding;

        if(holding)
            UpdateAim();

        if(currentInteractables.Count > 0) {
            closestInteractable = currentInteractables[0];
            float closestDst = Position.DistanceSquaredTo(closestInteractable.Position);
            closestInteractable.canInteract = true;

            for(int i = 1; i < currentInteractables.Count; i++) {
                float dst = Position.DistanceSquaredTo(currentInteractables[i].Position);

                if(dst < closestDst) {
                    closestDst = dst;
                    closestInteractable.canInteract = false;
                    closestInteractable = currentInteractables[i];
                    closestInteractable.canInteract = true;
                } else {
                    currentInteractables[i].canInteract = false;
                }
            }
            interactableText.Visible = true;
            interactableText.Text = "Pick up " + closestInteractable.interactionName;

        } else {
            interactableText.Visible = false;
            closestInteractable = null;
        }
    }

    public void EquipGun(Gun g) {
        if(g != null) {
            if(heldGun != null) {
                g.GetNode<Sprite>("Sprite").FlipH = heldGun.GetNode<Sprite>("Sprite").FlipH;
                g.ZIndex = heldGun.ZIndex;
                DropGun(heldGun);
            } else {
                g.GetNode<Sprite>("Sprite").FlipH = false;
            }
            g.SetFacingRight(facingRight);
            arm.Visible = true;
            heldGun = g;
            heldGun.holder = this;
            if(heldGun.GetParent() != null)
                heldGun.GetParent().RemoveChild(heldGun);
            arm.AddChild(heldGun);
            heldGun.Rotation = 0;
            heldGun.Position = hand1Socket.Position + heldGun.hand1Socket.Position * -1;
            heldGun.SetFacingRight(facingRight);
            //something for 2 handed guns
            holding = true;
            bodySprite.FlipH = false;
        }
    }

    public void DropGun(Gun g) {
        PickupGun newGunPickup = pickupGunRef.Instance<PickupGun>();
        GetParent().GetParent().GetNode<YSort>("Pickups").AddChild(newGunPickup);
        newGunPickup.GlobalPosition = g.GlobalPosition;
        arm.RemoveChild(g);
        newGunPickup.AddChild(g);
        g.GlobalRotation = arm.GlobalRotation;
        if(arm.Scale.x < 0)
            g.GetNode<Sprite>("Sprite").FlipH = true;
        g.ZIndex = 0;
        g.ReleaseTrigger();
        g.holder = null;
        newGunPickup.payload = g;
        newGunPickup.interactionName = g.itemName;
        newGunPickup.DropRandomDirection(false, -arm.Position.y);
        holding = false;
    }
    public int AddAmmo(AmmoType t, int amount) {
        int remainder = Mathf.Max(amount - (maxAmmo[t] - heldAmmo[t]), 0);

        if(remainder < amount) {
            heldAmmo[t] += amount - remainder;
        }
        heldAmmoList = new List<int>(heldAmmo.Values);
        return remainder;
    }
    public int RemoveAmmo(AmmoType t, int amount) {
        int amtRemoved = Mathf.Min(amount, heldAmmo[t]);
        heldAmmo[t] -= amtRemoved;
        heldAmmoList = new List<int>(heldAmmo.Values);
        return amtRemoved;
    }

    public int CheckAmmo(AmmoType t) {
        return heldAmmo[t];
    }
    public void UpdateAim() {
        Vector2 mousePos = GetGlobalMousePosition();
        facingRight = mousePos.x >= arm.GlobalPosition.x;
        float angle = Mathf.Atan((mousePos.y - arm.GlobalPosition.y) / (mousePos.x - arm.GlobalPosition.x));
        arm.Rotation = angle;
        arm.Scale = new Vector2((facingRight ? 1 : -1), 1);
        arm.ShowBehindParent = !facingRight;
        if(heldGun != null) {
            heldGun.SetFacingRight(facingRight);
        }
    }

    public override void _Input(InputEvent ie) {
        base._Input(ie);
        if(ie.IsActionPressed("move_right")) {                 //MOVEMENT
            movementInput.x = 1;
        } else if(ie.IsActionReleased("move_right")) {
            if(movementInput.x == 1)
                movementInput.x = 0;
        } else if(ie.IsActionPressed("move_left")) {
            movementInput.x = -1;
        } else if(ie.IsActionReleased("move_left")) {
            if(movementInput.x == -1)
                movementInput.x = 0;
        } else if(ie.IsActionPressed("move_up")) {
            movementInput.y = -1;
        } else if(ie.IsActionReleased("move_up")) {
            if(movementInput.y == -1)
                movementInput.y = 0;
        } else if(ie.IsActionPressed("move_down")) {
            movementInput.y = 1;
        } else if(ie.IsActionReleased("move_down")) {
            if(movementInput.y == 1)
                movementInput.y = 0;
        } else if(ie.IsActionPressed("interact")) {
            if(closestInteractable != null) {
                closestInteractable.Interact(this);
            }
        } else if(holding && heldGun != null) {
            if(ie.IsActionPressed("shoot")) {
                heldGun.PullTrigger();
            } else if(ie.IsActionReleased("shoot")) {
                heldGun.ReleaseTrigger();
            } else if(ie.IsActionPressed("reload")) {
                if(heldGun.currentMagSize < heldGun.magMaxSize) {
                    int amt = RemoveAmmo(heldGun.ammoType, heldGun.magMaxSize - heldGun.currentMagSize);
                    //reload animation
                    heldGun.FinishReload(heldGun.currentMagSize + amt); //implement ammo at some point
                }
            }
        }
    }
}