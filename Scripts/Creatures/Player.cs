using Godot;
using Godot.Collections;
using System.Collections.Generic;

public enum InputDeviceType {
    MouseKeyboard,
    Controller,
}
public struct ItemSlot {
    public Item handR;
    public Item handL;
    public ItemSlot(Item iR, Item iL) {
        handR = iR;
        handL = iL;
    }
}


public class Player:Creature {
    //Input Info
    public InputDeviceType inputDeviceType;

    //Player info
    [Export]
    public byte playerNum;
    //Interaction Data
    public List<Interactable> currentInteractables = new List<Interactable>();
    public Interactable closestInteractable;
    public Label interactableText;

    //Animation Data
    [Export]
    public bool holding = false; //all this affects is idle animation
    [Export]
    public bool canMove;
    [Export]
    public bool canAim;
    [Export]
    public bool actionable;
    public Vector2 lookDirR;
    public Vector2 lookDirL;

    //Movement variables
    //public float moveSpeed = 1f;
    public float collisionOffset = 0.05f; // Distance from rigidbody to check for collisions

    //Children
    Node2D shoulderR;
    Node2D shoulderL;
    public Node2D armR;
    public Node2D armL;
    public Node2D handRSocket;
    public Node2D handLSocket;
    public Node2D face;
    public Sprite eyes;
    public AnimationTree eyesAnimTree;
    public AnimationNodeStateMachinePlayback eyesAnimStateMachine;
    public AnimationPlayer animPlayerArms;

    //Aiming
    [Export]
    public Vector2 lookDirection;

    //Movement
    [Export]
    Vector2 movementInput;
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
    [Export]
    public System.Collections.Generic.Dictionary<ItemSlotType, ItemSlot> activeItems = new System.Collections.Generic.Dictionary<ItemSlotType, ItemSlot>();
    public Godot.Collections.Dictionary<AmmoType, int> heldAmmo = new Godot.Collections.Dictionary<AmmoType, int>();
    [Export]
    public Godot.Collections.Dictionary<AmmoType, int> maxAmmo = new Godot.Collections.Dictionary<AmmoType, int>();
    public List<int> heldAmmoList;

    //UI
    public GUIGamePlayer playerGUI;
    public CameraGame cam;

    //Prefabs
    [Export]
    public PackedScene pickupRef;

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
        shoulderL = GetNode<Node2D>("Body/ShoulderL");
        shoulderR = GetNode<Node2D>("Body/ShoulderR");
        armL = shoulderL.GetNode<Node2D>("ArmL");
        armR = shoulderR.GetNode<Node2D>("ArmR");
        handLSocket = armL.GetNode<Node2D>("UpperArmL/ForearmL/HandLSocket");
        handRSocket = armR.GetNode<Node2D>("UpperArmR/ForearmR/HandRSocket");
        interactableText = GetNode<Label>("InteractableText");
        face = GetNode<Node2D>("Body/Face");
        eyes = face.GetNode<Sprite>("Eyes");
        eyesAnimTree = GetNode<AnimationTree>("Body/Face/EyesAnimationTree");
        eyesAnimStateMachine = (AnimationNodeStateMachinePlayback)eyesAnimTree.Get("parameters/playback");
        animPlayerArms = armR.GetNode<AnimationPlayer>("AnimationPlayerArms");
        holding = false;
        canAim = true;
        canMove = true;
        actionable = true;
    }

    public override void _PhysicsProcess(float dt) {
        UpdateAim();
        bool walking = false;
        if(inputDeviceType == InputDeviceType.Controller) { //for now
            Vector2 leftStick = new Vector2(Input.GetJoyAxis(playerNum - 2, (int)JoystickList.AnalogLx), Input.GetJoyAxis(playerNum - 2, (int)JoystickList.AnalogLy));
            if(leftStick.LengthSquared() > 0.5f) {
                movementInput = leftStick;
            } else {
                movementInput = Vector2.Zero;
            }
        }
        if(movementInput != Vector2.Zero && CanManeuver() && (!dashing || Mathf.Abs(movementInput.Angle() - movementDirection.Angle()) > 0.5f)) {
            Vector2 vel = cam.AllowedVel(this, movementInput.Normalized() * moveSpeed, dt);
            walking = !vel.IsEqualApprox(Vector2.Zero) && !MoveAndSlide(vel).IsEqualApprox(Vector2.Zero);
            if(dashing) {
                dashing = false;
                movementVelocity = 0;
                movementDirection = Vector2.Zero;
            }
        }
        if(dashing) {
            movementVelocity -= movementFriction * dt;
            if(movementVelocity <= 0) {
                dashing = false;
                movementVelocity = 0;
                movementDirection = Vector2.Zero;
            } else {
                Vector2 vel = cam.AllowedVel(this, movementDirection * movementVelocity, dt);
                if(!vel.IsEqualApprox(Vector2.Zero))
                    MoveAndSlide(vel);
            }
        }
        handRSocket.ShowBehindParent = facingRight;
        bodySprite.Scale = new Vector2(facingRight ? 1 : -1, 1);
        face.Scale = new Vector2(facingRight ? 1 : -1, 1);
        string animState = walking ? "Walk" : "Idle";
        animStateMachine.Travel(animState);
        animTree.Set("parameters/" + animState + "/blend_position", holding ? 1 : 0);
        armR.Visible = holding;
        armL.Visible = holding;

        if(GD.Randf() < dt / 5f) {
            eyesAnimStateMachine.Travel("Blink");
        }

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

    public bool CanManeuver() {
        return movementVelocity < maxCancelDashSpeed;
    }

    public void Dash() {
        if(CanManeuver()) {
            dashing = true;
            movementDirection = movementInput.Normalized();
            movementVelocity = dashSpeed;
        }
    }
    /// <param name="r">Check the Right hand item if true, or check the Left hand item if false. Defaults to Right.</param>
    public bool ActiveItemValid(bool r = true) {
        return activeItems.ContainsKey(activeSlot) &&
        (r ? IsInstanceValid(activeItems[activeSlot].handR) : IsInstanceValid(activeItems[activeSlot].handL));
    }
    public void EquipItem(Item i) {
        if(IsInstanceValid(i)) {
            if(IsInstanceValid(i.GetParent()))
                i.GetParent().RemoveChild(i);

            bool equipHandR = !(i.slotType == ItemSlotType.Secondary && i is Gun);
            if(activeItems.ContainsKey(i.slotType)) {
                DropItem(equipHandR ? activeItems[i.slotType].handR : activeItems[i.slotType].handL);
            } else
                activeItems.Add(i.slotType, new ItemSlot(null, null));
            ItemSlot newSlot;
            if(activeItems.TryGetValue(i.slotType, out newSlot)) {
                if(equipHandR) {
                    newSlot.handR = i;
                    handRSocket.AddChild(i);
                } else {
                    newSlot.handL = i;
                    handLSocket.AddChild(i);
                }
                activeItems[i.slotType] = newSlot;
            }
            ChangeItemSlot(i.slotType);
            i.SetFacingRight(!facingRight); //The gun only updates facing right if it's different which creates issues. Duct tape to force updates. Improve?
            i.SetFacingRight(facingRight);
            armR.Visible = true;
            armL.Visible = true;
            i.Setup(this);
            i.holder = this;
            i.Rotation = 0;
            i.Position = i.hand1Socket.Position * -1;
            //something for 2 handed guns
            holding = true;
            bodySprite.FlipH = false;
        }
    }
    public void DropItem(Item i) {
        if(IsInstanceValid(i)) {
            i.GlobalRotation = armR.GlobalRotation;
            if(armR.Scale.x < 0)
                i.GetNode<Sprite>("Sprite").FlipH = true;
            i.ZIndex = 0;
            i.Visible = true;
            i.CancelUse();
            i.holder = null;
            Pickup newPickup = pickupRef.Instance<Pickup>();
            GetParent().GetParent().GetNode<YSort>("Pickups").AddChild(newPickup);
            newPickup.GlobalPosition = i.GlobalPosition;
            if(IsInstanceValid(i.GetParent()))
                i.GetParent().RemoveChild(i);
            newPickup.AddChild(i);
            newPickup.payload = i;
            newPickup.interactionName = i.itemName;
            newPickup.DropRandomDirection(false, -armR.Position.y);
            holding = false;
        }
    }
    public void SwapWeapons() {
        ChangeItemSlot(activeSlot == ItemSlotType.Primary ? ItemSlotType.Secondary : ItemSlotType.Primary);
    }
    public void ChangeItemSlot(ItemSlotType st) {
        ItemSlot oldSlot;
        if(activeItems.TryGetValue(activeSlot, out oldSlot)) {
            DeactivateItem(oldSlot.handR);
            DeactivateItem(oldSlot.handL);
        }
        ItemSlot newSlot;
        if(activeItems.TryGetValue(st, out newSlot)) {
            ActivateItem(newSlot.handR);
            ActivateItem(newSlot.handL);
            holding = true;
        } else {
            holding = false;
        }
        activeSlot = st;
    }
    public void ActivateItem(Item i) {
        if(IsInstanceValid(i)) {
            i.Visible = true;
        }
    }
    public void DeactivateItem(Item i) {
        if(IsInstanceValid(i)) {
            i.Visible = false;
            i.CancelUse();
        }
    }
    public void EnableMeleeHitboxes(bool rightHand) {
        if(activeItems.ContainsKey(ItemSlotType.Secondary)) {
            WeaponMelee w = (rightHand ? activeItems[ItemSlotType.Secondary].handR : activeItems[ItemSlotType.Secondary].handL) as WeaponMelee;
            if(IsInstanceValid(w))
                w.EnableHitboxes();
        }
    }
    public void DisableMeleeHitboxes(bool rightHand) {
        if(activeItems.ContainsKey(ItemSlotType.Secondary)) {
            WeaponMelee w = (rightHand ? activeItems[ItemSlotType.Secondary].handR : activeItems[ItemSlotType.Secondary].handL) as WeaponMelee;
            if(IsInstanceValid(w))
                w.DisableHitboxes();
        }
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
    public override void Heal(float healAmount) {
        base.Heal(healAmount);
        playerGUI.UpdateHP();
    }
    public override bool TakeDamage(float takenDamage) {
        bool dead = base.TakeDamage(takenDamage);
        playerGUI.UpdateHP();
        return dead;
    }
    public override void Die() {
        base.Die();
    }
    public Vector2 GetDirectionToInput(bool right = true) {
        Vector2 lookDir = new Vector2();
        if(inputDeviceType == InputDeviceType.MouseKeyboard) {
            Vector2 mousePos = GetGlobalMousePosition();
            lookDir = mousePos - (right ? armR : armL).GlobalPosition;
        } else {
            Vector2 rightStick = new Vector2(Input.GetJoyAxis(playerNum - 2, (int)JoystickList.AnalogRx), Input.GetJoyAxis(playerNum - 2, (int)JoystickList.AnalogRy));
            if(rightStick.LengthSquared() > 0.5f) {
                lookDir = rightStick;
            } else {
                lookDir = movementInput;
            }
        }
        return lookDir;
    }
    public void UpdateAim() {
        lookDirR = GetDirectionToInput(true);
        lookDirL = GetDirectionToInput(false);
        //ARMS/ITEMS
        if(canAim)
            UpdateArmDirection(true);
        UpdateArmDirection(false);
        //EYES
        Vector2 eyeDir;
        float lookDirDeadzone = inputDeviceType == InputDeviceType.MouseKeyboard ? 64 : 0.25f;
        if(lookDirR.LengthSquared() < lookDirDeadzone)
            eyeDir = new Vector2();
        else
            eyeDir = new Vector2(lookDirR.x * (facingRight ? 1 : -1), -lookDirR.y).Normalized();
        eyesAnimTree.Set("parameters/Idle/blend_position", eyeDir);
        eyesAnimTree.Set("parameters/Blink/blend_position", eyeDir);
    }
    public void UpdateArmDirection(bool right, bool forceAngleToInput = false) {
        Vector2 lD = right ? lookDirR : lookDirL;
        if(!lD.IsEqualApprox(Vector2.Zero)) {
            bool thisFacingRight = holding ? lD.x >= 0 : (movementInput.x != 0 ? movementInput.x > 0 : facingRight);
            bool flip = false;
            if(right && facingRight != thisFacingRight) {
                facingRight = thisFacingRight;
                flip = true;
            }
            if(holding) {
                float angle = Mathf.Atan((lD.y) / (lD.x));
                Node2D currArm = right ? armR : armL;
                bool angleToInput = true;
                ItemSlot currSl;
                if(activeItems.TryGetValue(activeSlot, out currSl)) {
                    Item currItem = right ? currSl.handR : currSl.handL;
                    if(IsInstanceValid(currItem)) {
                        currItem.SetFacingRight(thisFacingRight);
                        if(!forceAngleToInput && currItem is WeaponMelee) {
                            currArm.RotationDegrees = Mathf.Lerp(currArm.RotationDegrees * (flip ? -1 : 1), thisFacingRight ? 125 : -125, 0.1f);
                            Node2D currUpperArm = currArm.GetNode<Node2D>("UpperArm" + (right ? "R" : "L"));
                            currUpperArm.RotationDegrees = Mathf.Lerp(currUpperArm.RotationDegrees, 0, 0.1f);
                            angleToInput = false;
                        }
                    }
                }
                if(angleToInput)
                    currArm.Rotation = angle;
                currArm.Scale = new Vector2((thisFacingRight ? 1 : -1), 1);
                currArm.GetParent<Node2D>().ShowBehindParent = facingRight != right;
            }
        }
    }
    public override void _Input(InputEvent ie) {
        base._Input(ie);
        if(ie.IsActionPressed("move_right_p" + playerNum)) {                 //MOVEMENT
            movementInput.x = 1;
        } else if(ie.IsActionReleased("move_right_p" + playerNum)) {
            if(movementInput.x > 0)
                movementInput.x = 0;
        } else if(ie.IsActionPressed("move_left_p" + playerNum)) {
            movementInput.x = -1;
        } else if(ie.IsActionReleased("move_left_p" + playerNum)) {
            if(movementInput.x < 0)
                movementInput.x = 0;
        } else if(ie.IsActionPressed("move_up_p" + playerNum)) {
            movementInput.y = -1;
        } else if(ie.IsActionReleased("move_up_p" + playerNum)) {
            if(movementInput.y < 0)
                movementInput.y = 0;
        } else if(ie.IsActionPressed("move_down_p" + playerNum)) {
            movementInput.y = 1;
        } else if(ie.IsActionReleased("move_down_p" + playerNum)) {
            if(movementInput.y > 0)
                movementInput.y = 0;
        } else if(ie.IsActionPressed("dash_p" + playerNum)) {
            Dash();
        } else if(ie.IsActionPressed("interact_p" + playerNum)) {
            if(IsInstanceValid(closestInteractable)) {
                closestInteractable.Interact(this);
            }
        } else if(playerNum == 1 && ie.IsActionPressed("swap_weapons_p" + playerNum)) {
            SwapWeapons();
        } else if(playerNum != 1 && ie.IsActionPressed("look_up_p" + playerNum)) {
            lookDirection.y = -100;
        } else if(playerNum != 1 && ie.IsActionPressed("look_down_p" + playerNum)) {
            lookDirection.y = 100;
        } else if(playerNum != 1 && ie.IsActionPressed("look_right_p" + playerNum)) {
            lookDirection.x = 100;
        } else if(playerNum != 1 && ie.IsActionPressed("look_left_p" + playerNum)) {
            lookDirection.x = -100;
        } else if(ie.IsActionPressed("item_use_p" + playerNum)) {              //Use/Attack 1
            if(ActiveItemValid())
                activeItems[activeSlot].handR.Use();
        } else if(ie.IsActionReleased("item_use_p" + playerNum)) {
            if(ActiveItemValid())
                activeItems[activeSlot].handR.CancelUse();
        } else if(ie.IsActionPressed("item_use2_p" + playerNum)) {             //Use/Attack 2
            if(ActiveItemValid(false))
                activeItems[activeSlot].handL.Use();
            else if(ActiveItemValid())
                activeItems[activeSlot].handR.AltUse();
        } else if(ie.IsActionReleased("item_use2_p" + playerNum)) {
            if(ActiveItemValid(false))
                activeItems[activeSlot].handL.CancelUse();
        } else if(ie.IsActionPressed("item_custom_p" + playerNum)) {
            if(ActiveItemValid())
                activeItems[activeSlot].handR.Custom();
            if(ActiveItemValid(false))
                activeItems[activeSlot].handL.Custom();
        } else if(ie.IsActionPressed("add_hp_p" + playerNum)) {                 //Add HP TEST
            Heal(3);
        } else if(ie.IsActionPressed("remove_hp_p" + playerNum)) {                 //Remove HP TEST
            TakeDamage(2);
        }
    }
}