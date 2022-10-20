using Godot;
using System;

public class GUIGamePlayerMenu:Control {
    public Player player;
    GUIPlayerInventory inv;
    GUIPlayerEquipment equip;
    public void SetPlayer(Player p) {
        inv = GetNode<GUIPlayerInventory>("TabContainer/Inventory");
        equip = GetNode<GUIPlayerEquipment>("TabContainer/Equipment");
        player = inv.player = equip.player = p;
    }
    public void Init() {
        inv = GetNode<GUIPlayerInventory>("TabContainer/Inventory");
        inv.Init();
        equip = GetNode<GUIPlayerEquipment>("TabContainer/Equipment");
        equip.Init();
    }
    public void Close() {
        if(IsInstanceValid(inv))
            inv.Close();
        if(IsInstanceValid(equip))
            equip.Close();
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Close");
    }
}
