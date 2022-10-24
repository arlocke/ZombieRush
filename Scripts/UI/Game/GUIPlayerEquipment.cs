using Godot;
using System.Collections.Generic;

public class GUIPlayerEquipment:GUIItemContainer {
    [Export]
    public PackedScene spacer;
    public override void Init() {
        for(int i = 0; i < 3; i++) {    //Weapons and utility
            AddItemSlot();
            AddChild(spacer.Instance());
            AddItemSlot();
        }
        AddChild(spacer.Instance());
        AddItemSlot();                      //Armor
        foreach(KeyValuePair<ItemSlotType, ItemSlot> itemSlot in player.activeItems) {      //Instance equipment tiles
            if(IsInstanceValid(itemSlot.Value.handR)) {
                AddItemTile(itemSlot.Value.handR, (int)itemSlot.Key * 2);
            }
            if(IsInstanceValid(itemSlot.Value.handL)) {
                AddItemTile(itemSlot.Value.handL, (int)itemSlot.Key * 2 + 1);
            }
        }
    }
    public override void Close() {
        if(IsInstanceValid(player)) {
            player.ResetActiveItems();
        }
        RemoveSlots();
    }
}