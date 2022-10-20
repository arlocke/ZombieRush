using Godot;
using System;

public class GUIItemContainer:GridContainer {
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
    public override void _PhysicsProcess(float dt) {
        base._PhysicsProcess(dt);
        if(!IsInstanceValid(player))
            (GetParent() as GUIGamePlayerMenu).Close();
        if(dragging && IsInstanceValid(draggedTile)) {
            draggedTile.RectGlobalPosition = GetGlobalMousePosition() + dragOffset;
        }
    }
    public virtual void AddItemSlot() {
        Control newSlot = itemSlotRef.Instance<Control>();
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
        Button newTile = itemTileRef.Instance<Button>();
        slot.AddChild(newTile);
        newTile.SelfModulate = GlobalData.tierColors[i.tier];
        if(IsInstanceValid(i.GetParent()))
            i.GetParent().RemoveChild(i);
        newTile.AddChild(i);
        i.Position = newTile.RectSize / 2;
        i.ShowBehindParent = false;
        i.RotationDegrees = -45;
        i.Visible = true;
        newTile.Connect("button_down", this, "GrabItemTile", new Godot.Collections.Array { newTile });
        newTile.Connect("button_up", this, "ReleaseItemTile");
    }
    public virtual void GrabItemTile(Button tile) {
        dragging = true;
        draggedTile = tile;
        dragOffset = tile.RectGlobalPosition - GetGlobalMousePosition();
        Control draggedSlot = tile.GetParent<Control>();
        dragFromIndex = GetSlotIndex(draggedSlot);
        draggedTile.RectGlobalPosition = GetGlobalMousePosition() + dragOffset;
    }
    public virtual bool ReleaseItemTile() {
        dragging = false;
        //Find closest slot
        Control closestSlot = GetNode<Control>("Slot0");
        float closestDstSqr = closestSlot.RectGlobalPosition.DistanceSquaredTo(draggedTile.RectGlobalPosition);
        for(int i = 1; i < GetChildCount(); i++) {
            Control slot = GetChild<Control>(i);
            if(!slot.Name.Contains("Slot"))
                continue;
            float d = slot.RectGlobalPosition.DistanceSquaredTo(draggedTile.RectGlobalPosition);
            if(d < closestDstSqr) {
                closestSlot = slot;
                closestDstSqr = d;
            }
        }
        if(closestDstSqr < Mathf.Pow(closestSlot.RectSize.x + 5, 2)) {                //Slot is close enough that we can swap
            if(closestSlot.GetChildCount() > 0) {
                Button tileToSwap = closestSlot.GetChildOrNull<Button>(0);
                if(IsInstanceValid(tileToSwap)) {
                    closestSlot.RemoveChild(tileToSwap);
                    GetNode<Control>("Slot" + dragFromIndex).AddChild(tileToSwap);
                    tileToSwap.RectPosition = Vector2.Zero;
                }
            }
            draggedTile.GetParent().RemoveChild(draggedTile);
            if(IsInstanceValid(closestSlot)) {
                closestSlot.AddChild(draggedTile);
            }
            draggedTile.RectPosition = Vector2.Zero;
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
        int i = c.Name.Length - 1;
        while(c.Name[i] >= '0' && c.Name[i] <= '9') {
            i--;
        }
        return Int32.Parse(c.Name.Substring(i + 1));
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
