using Godot;
using System;

public partial class GUIItemContainer:GridContainer {
    public Player player;
    public bool dragging = false;
    public Button draggedTile;
    public Vector2 dragOffset;
    public int dragFromIndex;
    [Export]
    public PackedScene itemTileRef;
    [Export]
    public PackedScene itemSlotRef;
    public virtual void Init() {
        for(int i = 0; i < player.inventory.Length; i++) {
            AddItemSlot();
            if(IsInstanceValid(player.inventory[i])) {
                AddItemTile(player.inventory[i], i);
            }
        }
    }
    public override void _PhysicsProcess(double dt) {
        base._PhysicsProcess(dt);
        if(!IsInstanceValid(player))
            (GetParent() as GUIGamePlayerMenu).Close();
        if(dragging && IsInstanceValid(draggedTile)) {
            draggedTile.GlobalPosition = GetGlobalMousePosition() + dragOffset;
        }
    }
    public virtual void AddItemSlot() {
        Control newSlot = itemSlotRef.Instantiate<Control>();
        AddChild(newSlot, true);
    }
    public void RemoveSlots() {
        foreach(Node s in GetChildren()) {
            s.QueueFree();
        }
    }
    public virtual void AddItemTile(Item i, int index = 0) {
        Control slot = GetNode<Control>("Slot" + index);
        if(!IsInstanceValid(slot)) return;
        foreach(Node n in slot.GetChildren()) {
            if(n.IsInGroup("ItemTiles"))
                n.QueueFree();
        }
        Button newTile = itemTileRef.Instantiate<Button>();
        slot.AddChild(newTile);
        newTile.SelfModulate = GlobalData.tierColors[i.tier];
        if(IsInstanceValid(i.GetParent()))
            i.GetParent().RemoveChild(i);
        newTile.AddChild(i);
        i.Position = newTile.Size / 2;
        i.ShowBehindParent = false;
        i.RotationDegrees = -45;
        i.Visible = true;
        newTile.Connect("button_down", Callable.From(() => GrabItemTile(newTile)));
        newTile.Connect("button_up", new Callable(this, MethodName.ReleaseItemTile));
    }
    public virtual void GrabItemTile(Button tile) {
        dragging = true;
        draggedTile = tile;
        dragOffset = tile.GlobalPosition - GetGlobalMousePosition();
        Control draggedSlot = tile.GetParent<Control>();
        dragFromIndex = GetSlotIndex(draggedSlot);
        draggedTile.GlobalPosition = GetGlobalMousePosition() + dragOffset;
    }
    public virtual bool ReleaseItemTile() {
        dragging = false;
        //Find closest slot
        Control closestSlot = GetNode<Control>("Slot0");
        float closestDstSqr = closestSlot.GlobalPosition.DistanceSquaredTo(draggedTile.GlobalPosition);
        for(int i = 1; i < GetChildCount(); i++) {
            Control slot = GetChild<Control>(i);
            if(!slot.Name.ToString().Contains("Slot"))
                continue;
            float d = slot.GlobalPosition.DistanceSquaredTo(draggedTile.GlobalPosition);
            if(d < closestDstSqr) {
                closestSlot = slot;
                closestDstSqr = d;
            }
        }
        if(closestDstSqr < Mathf.Pow(closestSlot.Size.X + 5, 2)) {                //Slot is close enough that we can swap
            if(closestSlot.GetChildCount() > 0) {
                Button tileToSwap = closestSlot.GetChildOrNull<Button>(0);
                if(IsInstanceValid(tileToSwap)) {
                    closestSlot.RemoveChild(tileToSwap);
                    GetNode<Control>("Slot" + dragFromIndex).AddChild(tileToSwap);
                    tileToSwap.Position = Vector2.Zero;
                }
            }
            draggedTile.GetParent().RemoveChild(draggedTile);
            if(IsInstanceValid(closestSlot)) {
                closestSlot.AddChild(draggedTile);
            }
            draggedTile.Position = Vector2.Zero;
            draggedTile = null;

            int closestIndex = GetSlotIndex(closestSlot);
            if(this is GUIPlayerInventory)
                player.SwapInventorySlots(dragFromIndex, closestIndex);
            else
                player.SwapEquipmentSlots(dragFromIndex, closestIndex);
            return true;
        } else {          //Slot is too far away. Drop the item
            if(this is GUIPlayerInventory)
                player.DropFromInventory(dragFromIndex);
            else if(draggedTile.GetChildCount() > 0) {
                foreach(Node n in draggedTile.GetChildren()) {
                    if(n is Item) {
                        player.DropFromEquipment(n as Item, false);
                    }
                }
            }
            draggedTile.QueueFree();
            return false;
        }
    }
    public int GetSlotIndex(Control c) {
        string cName = c.Name;
        int i = cName.Length - 1;
        while(cName[i] >= '0' && cName[i] <= '9') {
            i--;
        }
        return Int32.Parse(cName.Substring(i + 1));
    }
    public virtual void Close() {
        if(IsInstanceValid(player)) {
            for(int i = 0; i < player.inventory.Length; i++) {
                if(IsInstanceValid(player.inventory[i])) {
                    Item invItem = player.inventory[i];
                    invItem.GetParent().RemoveChild(invItem);
                    player.AddChild(invItem);
                    invItem.Position = new Vector2(0, 0);
                    invItem.RotationDegrees = 0;
                    invItem.Visible = false;
                }
            }
        }
        RemoveSlots();
    }

}
