using Godot;
using Godot.Collections;
using System.Collections.Generic;

public enum InputDeviceType {
    MouseKeyboard,
    Controller,
}

public partial class Player:Character {
    //Input Info
    [Export]
    public InputDeviceType inputDeviceType;

    //Player info
    [Export]
    public byte playerNum;
    //Interaction Data
    public List<Interactable> currentInteractables = new List<Interactable>();
    public Interactable closestInteractable;
    public Label interactableText;

    //Movement variables
    //public float moveSpeed = 1f;
    public float collisionOffset = 0.05f; // Distance from rigidbody to check for collisions

    //Aiming
    [Export]
    public Vector2 lookDirection;

    //Movement
    [Export]
    Vector2 movementInput;

    //Inventory
    public Godot.Collections.Dictionary<AmmoType, int> heldAmmo = new Godot.Collections.Dictionary<AmmoType, int>();
    [Export]
    public Godot.Collections.Dictionary<AmmoType, int> maxAmmo = new Godot.Collections.Dictionary<AmmoType, int>();
    public List<int> heldAmmoList;

    //UI
    public GUIGamePlayer playerGUI;
    public CameraGame cam;

    //Managers
    public PlayerManager playerManager;

    //Prefabs
    [Export]
    public PackedScene pickupRef;
    [Export]
    public PackedScene playerMenuGUIRef;

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
        eyes = face.GetNode<Sprite2D>("Eyes");
        eyebrows = face.GetNode<Sprite2D>("Eyebrows");
        eyesAnimTree = GetNode<AnimationTree>("Body/Face/EyesAnimationTree");
        eyesAnimStateMachine = (AnimationNodeStateMachinePlayback)eyesAnimTree.Get("parameters/playback");
        animPlayerArms = armR.GetNode<AnimationPlayer>("AnimationPlayerArms");
        bodySprite = GetNode<Sprite2D>("Body/BodySprite");
        holding = false;
        canAim = true;
        canMove = true;
        actionable = true;
    }

    public override void _PhysicsProcess(double dt) {
        UpdateAim();
        bool walking = false;
        if(inputDeviceType == InputDeviceType.Controller) { //for now
            Vector2 leftStick = new Vector2(Input.GetJoyAxis(playerNum - 2, JoyAxis.LeftX), Input.GetJoyAxis(playerNum - 2, JoyAxis.LeftY));
            if(leftStick.LengthSquared() > 0.5f) {
                movementInput = leftStick;
            } else {
                movementInput = Vector2.Zero;
            }
        }
        if(movementInput != Vector2.Zero && CanManeuver() && (!dashing || Mathf.Abs(movementInput.Angle() - movementDirection.Angle()) > 0.5f)) {
            Velocity = cam.AllowedVel(this, movementInput.Normalized() * moveSpeed, (float)dt);
            MoveAndSlide();
            walking = !Velocity.IsEqualApprox(Vector2.Zero);
            if(dashing) {
                dashing = false;
                movementVelocity = 0;
                movementDirection = Vector2.Zero;
            }
        }
        if(dashing) {
            movementVelocity -= (float)(movementFriction * dt);
            if(movementVelocity <= 0) {
                dashing = false;
                movementVelocity = 0;
                movementDirection = Vector2.Zero;
            } else {
                Velocity = cam.AllowedVel(this, movementDirection * movementVelocity, (float)dt);
                if(!Velocity.IsEqualApprox(Vector2.Zero))
                    MoveAndSlide();
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

        if(GD.Randf() < dt / 5f && eyesAnimStateMachine.GetCurrentNode() != "Blink") {
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
    public void ResetActiveItems() {
        foreach(KeyValuePair<ItemSlotType, ItemSlot> itemSlot in activeItems) {
            if(IsInstanceValid(itemSlot.Value.handR)) {
                Item item = itemSlot.Value.handR;
                item.GetParent().RemoveChild(item);
                item.Visible = false;
                AttachItemToHand(item, true);
            }
            if(IsInstanceValid(itemSlot.Value.handL)) {
                Item item = itemSlot.Value.handL;
                item.GetParent().RemoveChild(item);
                item.Visible = false;
                AttachItemToHand(item, false);
            }
        }
    }
    public void AttachItemToHand(Item i, bool right = true) {
        (right ? handRSocket : handLSocket).AddChild(i);
        i.SetFacingRight(!facingRight); //The gun only updates facing right if it's different which creates issues. Duct tape to force updates. Improve?
        i.SetFacingRight(facingRight);
        i.Rotation = 0;
        i.Position = i.hand1Socket.Position * -1;
    }
    public void EquipItem(Item i) {
        if(IsInstanceValid(i)) {
            if(IsInstanceValid(i.GetParent()))
                i.GetParent().RemoveChild(i);

            bool equipHandR = !(i.slotType == ItemSlotType.Secondary && i is Gun);
            Node menuGUI = GetNodeOrNull("GUIGamePlayerMenu");
            bool menuExists = IsInstanceValid(menuGUI);
            if(activeItems.ContainsKey(i.slotType)) {
                DropItem(equipHandR ? activeItems[i.slotType].handR : activeItems[i.slotType].handL, !menuExists);
            } else
                activeItems.Add(i.slotType, new ItemSlot(null, null));
            ItemSlot newSlot;
            if(activeItems.TryGetValue(i.slotType, out newSlot)) {
                if(equipHandR) {
                    newSlot.handR = i;
                } else {
                    newSlot.handL = i;
                }
                activeItems[i.slotType] = newSlot;
            }
            i.Setup(this);
            if(menuExists) {
                menuGUI.GetNode<GUIPlayerEquipment>("TabContainer/Equipment").AddItemTile(i, (int)i.slotType * 2 + (!equipHandR ? 1 : 0));

            } else {
                AttachItemToHand(i, equipHandR);
                ChangeItemSlot(i.slotType);
                armR.Visible = true;
                armL.Visible = true;
                //something for 2 handed guns
                holding = true;
                bodySprite.FlipH = false;
            }
        }
    }
    public bool AddToInventory(Item i) {
        if(!IsInstanceValid(i)) return false;
        for(int index = 0; index < inventory.Length; index++) {
            if(!IsInstanceValid(inventory[index])) { //Empty slot
                inventory[index] = i;
                Node oldParent = i.GetParent();
                oldParent.RemoveChild(i);
                AddChild(i);
                if(oldParent is Pickup)
                    oldParent.QueueFree();
                i.Visible = false;
                Node menuGUI = GetNodeOrNull("GUIGamePlayerMenu");
                if(IsInstanceValid(menuGUI))
                    menuGUI.GetNode<GUIPlayerInventory>("TabContainer/Inventory").AddItemTile(i, index);
                return true;
            }
        }
        return false;
    }
    public void SwapInventorySlots(int from, int to) {
        Item i1 = inventory[from];
        Item i2 = inventory[to];
        inventory[to] = i1;
        inventory[from] = i2;
    }
    public bool DropFromInventory(int index) {
        if(index >= inventory.Length || !IsInstanceValid(inventory[index])) return false;
        DropItem(inventory[index], false);
        inventory[index] = null;
        return true;
    }
    public void SwapEquipmentSlots(int from, int to) {
        ItemSlot slotFrom, slotTo;
        Item itemFrom, itemTo;
        ItemSlotType fromSlotType = (ItemSlotType)Mathf.FloorToInt((float)from / 2);
        if(activeItems.TryGetValue(fromSlotType, out slotFrom)) {
            ItemSlotType toSlotType = (ItemSlotType)Mathf.FloorToInt((float)to / 2);
            if(fromSlotType != toSlotType) { //If the items aren't being swapped in the same slot
                if(!activeItems.TryGetValue(toSlotType, out slotTo)) {
                    activeItems.Add(toSlotType, new ItemSlot(null, null));
                    slotTo = activeItems[toSlotType];
                }
                bool fromRight = (from % 2 == 0);
                bool toRight = (to % 2 == 0);
                itemFrom = fromRight ? slotFrom.handR : slotFrom.handL;
                itemTo = toRight ? slotTo.handR : slotTo.handL;
                if(fromRight)
                    slotFrom.handR = itemTo;
                else
                    slotFrom.handL = itemTo;
                if(toRight)
                    slotTo.handR = itemFrom;
                else
                    slotTo.handL = itemFrom;
                activeItems[fromSlotType] = slotFrom;
                activeItems[toSlotType] = slotTo;
            } else {                                //If the items are in the same slot, and are just being swapped left to right
                itemFrom = slotFrom.handR;
                itemTo = slotFrom.handL;
                slotFrom.handR = itemTo;
                slotFrom.handL = itemFrom;
                activeItems[fromSlotType] = slotFrom;
            }
        }
    }
    public void DropFromEquipment(Item i, bool keepPosition = true) {
        ItemSlot s;
        if(activeItems.TryGetValue(i.slotType, out s)) {
            if(s.handR == i)
                s.handR = null;
            else if(s.handL == i)
                s.handL = null;
            activeItems[i.slotType] = s;
        }
        DropItem(i, keepPosition);
    }
    public void DropItem(Item i, bool keepPosition = true) {
        if(IsInstanceValid(i)) {
            float itemRot = i.GlobalRotation;
            if(armR.Scale.X < 0)
                i.GetNode<Sprite2D>("Sprite2D").FlipH = true;
            i.ZIndex = 0;
            i.Visible = true;
            i.CancelUse();
            i.holder = null;
            Pickup newPickup = pickupRef.Instantiate<Pickup>();
            GetParent().GetParent().GetNode<Node2D>("Pickups").AddChild(newPickup);
            newPickup.GlobalPosition = keepPosition ? i.GlobalPosition : GlobalPosition + (armR.Position * Vector2.Down);
            if(IsInstanceValid(i.GetParent()))
                i.GetParent().RemoveChild(i);
            newPickup.AddChild(i);
            newPickup.payload = i;
            newPickup.interactionName = i.itemName;
            newPickup.DropRandomDirection(false, -armR.Position.Y);
            i.GlobalRotation = itemRot;
            holding = false;
        }
    }
    public void SwapWeapons() {
        ChangeItemSlot(activeSlot == ItemSlotType.Primary ? ItemSlotType.Secondary : ItemSlotType.Primary);
    }
    public void ChangeItemSlot(ItemSlotType st) {
        CancelMeleeAttack();
        lastActiveSlot = activeSlot;
        ItemSlot oldSlot;
        if(activeItems.TryGetValue(activeSlot, out oldSlot)) {
            DeactivateItem(oldSlot.handR);
            DeactivateItem(oldSlot.handL);
        }
        ItemSlot newSlot;
        if(activeItems.TryGetValue(st, out newSlot) && (IsInstanceValid(newSlot.handR) || IsInstanceValid(newSlot.handL))) {
            ActivateItem(newSlot.handR);
            ActivateItem(newSlot.handL);
            holding = true;
            activeSlot = st;
        } else {
            holding = false;
            activeSlot = ItemSlotType.None;
        }
        armR.RotationDegrees = 0;
        armR.GetNode<Node2D>("UpperArmR").RotationDegrees = 0;
        armR.GetNode<Node2D>("UpperArmR/ForearmR").RotationDegrees = 0;
        armR.GetNode<Node2D>("UpperArmR/ForearmR/HandRSocket").RotationDegrees = 0;
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
    public void EnableMeleeHitboxes(bool rightHand = true) {
        ItemSlot currSlot;
        if(activeItems.TryGetValue(activeSlot, out currSlot)) {
            Item currItem = (rightHand ? currSlot.handR : currSlot.handL);
            if(IsInstanceValid(currItem) && currItem is WeaponMelee) {
                (currItem as WeaponMelee).EnableHitboxes();
            }
        }
    }
    public void DisableMeleeHitboxes(bool rightHand = true) {
        ItemSlot currSlot;
        if(activeItems.TryGetValue(activeSlot, out currSlot)) {
            Item currItem = (rightHand ? currSlot.handR : currSlot.handL);
            if(IsInstanceValid(currItem) && currItem is WeaponMelee) {
                (currItem as WeaponMelee).DisableHitboxes();
            }
        }
    }
    public void CancelMeleeAttack() {
        DisableMeleeHitboxes(true);
        DisableMeleeHitboxes(false);
        animPlayerArms.Stop(false);
        canAim = true;
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
            Vector2 rightStick = new Vector2(Input.GetJoyAxis(playerNum - 2, JoyAxis.RightX), Input.GetJoyAxis(playerNum - 2, JoyAxis.RightY));
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
            eyeDir = new Vector2(lookDirR.X * (facingRight ? 1 : -1), -lookDirR.Y).Normalized();
        eyesAnimTree.Set("parameters/Idle/blend_position", eyeDir);
        eyesAnimTree.Set("parameters/Blink/blend_position", eyeDir);
    }
    public void UpdateArmDirection(bool right, bool forceAngleToInput = false) {
        Vector2 lD = right ? lookDirR : lookDirL;
        if(!lD.IsEqualApprox(Vector2.Zero)) {
            bool thisFacingRight = holding ? lD.X >= 0 : (movementInput.X != 0 ? movementInput.X > 0 : facingRight);
            bool flip = false;
            if(right && facingRight != thisFacingRight) {
                facingRight = thisFacingRight;
                flip = true;
            }
            if(holding) {
                float scaleX = (thisFacingRight ? 1 : -1);
                float angle = Mathf.Atan((lD.Y) / (lD.X));
                Node2D currArm = right ? armR : armL;
                bool angleToInput = true;
                ItemSlot currSl;
                flip = thisFacingRight ^ (int)currArm.Scale.X == 1;
                if(activeItems.TryGetValue(activeSlot, out currSl)) {
                    Item currItem = right ? currSl.handR : currSl.handL;
                    if(IsInstanceValid(currItem)) {
                        currItem.SetFacingRight(thisFacingRight);
                        if(!forceAngleToInput && currItem is WeaponMelee) {           //Override melee weapon arm angles when holding
                            currArm.RotationDegrees = Mathf.Lerp(currArm.RotationDegrees * (flip ? -1 : 1), thisFacingRight ? 125 : -125, 0.1f);
                            char dirChar = (right ? 'R' : 'L');
                            Node2D currUpperArm = currArm.GetNode<Node2D>("UpperArm" + dirChar);
                            currUpperArm.RotationDegrees = Mathf.Lerp(currUpperArm.RotationDegrees, 0, 0.1f);
                            angleToInput = false;
                            Node2D currHandSocket = currUpperArm.GetNode<Node2D>("Forearm" + dirChar + "/Hand" + dirChar + "Socket");
                            if(flip)
                                currHandSocket.RotationDegrees = (right == thisFacingRight) ? 69 : -20;
                            else
                                currHandSocket.RotationDegrees = Mathf.Lerp(currHandSocket.RotationDegrees, (right == thisFacingRight) ? 69 : -20, 0.1f);
                        }
                        if(angleToInput)
                            currArm.Rotation = angle;
                    } else {
                        currArm.RotationDegrees = (facingRight ? -38 : 52);
                    }
                }
                currArm.Scale = new Vector2(scaleX, 1);
                currArm.GetParent<Node2D>().ShowBehindParent = facingRight != right;
            }
        }
    }
    ///<summary>Finds an exit that goes to the given destination room and has the given sort by position index</summary>
    public void MoveToExit(string exitDest, int exitSortIndex) {
        RoomExit destinationExit;   //The exit in the destination room that the players will go to

        Array<Node> currExits = GetTree().GetNodesInGroup("Exits");
        List<RoomExit> currExitsToDest = new List<RoomExit>();
        foreach(Node n in currExits) {   //Find all exits that go to the same destination room as this one
            RoomExit e = n as RoomExit;
            if(e.destination == exitDest)
                currExitsToDest.Add(e);
        }
        if(currExitsToDest.Count > 1) {  //If there are multiple exits going to the same room, figure out this exit's position relative to the others
            destinationExit = RoomExit.SortExitsByPos(currExitsToDest)[exitSortIndex];
        } else {
            destinationExit = currExitsToDest[0];
        }

        destinationExit.overlappedPlayers.Add(this);
        GlobalPosition = destinationExit.GetNode<Node2D>("SpawnTarget" + playerNum).GlobalPosition;
        destinationExit.currentRoom.SetupPlayerForRoom(this);
    }

    public void SpawnPlayerMenuGUI() {
        ChangeItemSlot(ItemSlotType.None);
        GUIGamePlayerMenu menu = playerMenuGUIRef.Instantiate<GUIGamePlayerMenu>();
        AddChild(menu);
        menu.Position = bodySprite.GetRect().Size * new Vector2(0.5f, -1);
        menu.SetPlayer(this);
    }
    public override void _Input(InputEvent ie) {
        base._Input(ie);
        if(ie.IsActionPressed("move_right_p" + playerNum)) {                 //MOVEMENT
            movementInput.X = 1;
        } else if(ie.IsActionReleased("move_right_p" + playerNum)) {
            if(movementInput.X > 0)
                movementInput.X = 0;
        } else if(ie.IsActionPressed("move_left_p" + playerNum)) {
            movementInput.X = -1;
        } else if(ie.IsActionReleased("move_left_p" + playerNum)) {
            if(movementInput.X < 0)
                movementInput.X = 0;
        } else if(ie.IsActionPressed("move_up_p" + playerNum)) {
            movementInput.Y = -1;
        } else if(ie.IsActionReleased("move_up_p" + playerNum)) {
            if(movementInput.Y < 0)
                movementInput.Y = 0;
        } else if(ie.IsActionPressed("move_down_p" + playerNum)) {
            movementInput.Y = 1;
        } else if(ie.IsActionReleased("move_down_p" + playerNum)) {
            if(movementInput.Y > 0)
                movementInput.Y = 0;
        } else if(ie.IsActionPressed("dash_p" + playerNum)) {               //DASH
            Dash();
        } else if(ie.IsActionPressed("interact_p" + playerNum)) {           //INTERACT
            if(IsInstanceValid(closestInteractable)) {
                closestInteractable.Interact(this);
            }
        } else if(playerNum == 1 && ie.IsActionPressed("swap_weapons_p" + playerNum)) { //SWAP WEAPONS
            if(!IsInstanceValid(GetNodeOrNull<GUIGamePlayerMenu>("GUIGamePlayerMenu")))
                SwapWeapons();
        } else if(playerNum != 1 && ie.IsActionPressed("look_up_p" + playerNum)) {
            lookDirection.Y = -100;
        } else if(playerNum != 1 && ie.IsActionPressed("look_down_p" + playerNum)) {
            lookDirection.Y = 100;
        } else if(playerNum != 1 && ie.IsActionPressed("look_right_p" + playerNum)) {
            lookDirection.X = 100;
        } else if(playerNum != 1 && ie.IsActionPressed("look_left_p" + playerNum)) {
            lookDirection.X = -100;
        } else if(ie.IsActionPressed("item_use_p" + playerNum)) {              //Use/Attack 1
            if(ActiveItemValid()) {
                activeItems[activeSlot].handR.Use();
                eyebrows.Frame = 2;
            }
        } else if(ie.IsActionReleased("item_use_p" + playerNum)) {
            if(ActiveItemValid()) {
                activeItems[activeSlot].handR.CancelUse();
                eyebrows.Frame = 0;
            }
        } else if(ie.IsActionPressed("item_use2_p" + playerNum)) {             //Use/Attack 2
            if(ActiveItemValid(false)) {
                activeItems[activeSlot].handL.Use();
                eyebrows.Frame = 2;
            } else if(ActiveItemValid()) {
                activeItems[activeSlot].handR.AltUse();
                eyebrows.Frame = 2;
            }
        } else if(ie.IsActionReleased("item_use2_p" + playerNum)) {
            if(ActiveItemValid(false)) {
                activeItems[activeSlot].handL.CancelUse();
                eyebrows.Frame = 0;
            }
        } else if(ie.IsActionPressed("item_custom_p" + playerNum)) {
            if(ActiveItemValid())
                activeItems[activeSlot].handR.Custom();
            if(ActiveItemValid(false))
                activeItems[activeSlot].handL.Custom();
        } else if(ie.IsActionPressed("player_menu_p" + playerNum)) {            //Player Menu (Inventory and Active Items) open/close
            GUIGamePlayerMenu menuGUI = GetNodeOrNull<GUIGamePlayerMenu>("GUIGamePlayerMenu");
            if(!IsInstanceValid(menuGUI))
                SpawnPlayerMenuGUI();
            else {
                menuGUI.Close();
                ChangeItemSlot(lastActiveSlot);
            }
        }
          /////////TEST INPUTS///////////
          else if(ie.IsActionPressed("add_to_inv_p" + playerNum)) {                 //Add to Inventory
            if(IsInstanceValid(closestInteractable) && closestInteractable is Pickup && (closestInteractable as Pickup).payload is Item) {
                AddToInventory((closestInteractable as Pickup).payload as Item);
            }
        } else if(ie.IsActionPressed("add_hp_p" + playerNum)) {                 //Add HP TEST
            Heal(3);
        } else if(ie.IsActionPressed("remove_hp_p" + playerNum)) {                 //Remove HP TEST
            TakeDamage(2);
        }
    }
    public override Rect2 GetSpriteRectWorld(Vector2 pos) {
        if(pos.IsEqualApprox(Vector2.Zero))
            pos = GlobalPosition;
        return new Rect2(pos - bodySprite.GetRect().Size / 2 + new Vector2(0, bodySprite.Offset.Y), bodySprite.GetRect().Size);
    }
}