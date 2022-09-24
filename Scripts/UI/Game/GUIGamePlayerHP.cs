using Godot;
using System;

public class GUIGamePlayerHP:GridContainer {
    [Export]
    PackedScene hpRef;
    [Export]
    Player player;
    public override void _Ready() {
        player = GetTree().GetNodesInGroup("Players")[0] as Player;
        for(int i = 0; i < (player as Player).hp; i++) {
            Control newHP = hpRef.Instance<Control>();
            AddChild(newHP);
        }
    }
    public void AddHP(int num) {
        for(int i = 0; i < num; i++) {
            Control newHP = hpRef.Instance<Control>();
            AddChild(newHP);
        }
    }
    public void RemoveHP(int num) {

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
