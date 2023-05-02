using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class GUIGamePlayer:VSplitContainer {
    [Export]
    PackedScene hpRef;
    [Export]
    Player player;
    GridContainer heartBox;
    List<Control> activeHearts = new List<Control>();
    List<Control> inactiveHearts = new List<Control>();
    public override void _Ready() {
        heartBox = GetNode<GridContainer>("GUIGamePlayerHP");
    }
    public void SetupPlayer(Player p) {
        if(heartBox == null) heartBox = GetNode<GridContainer>("GUIGamePlayerHP");
        p.playerGUI = this;
        player = p;
        UpdateHP();
    }
    public void UpdateHP() {
        int dif;
        if(IsInstanceValid(player))
            dif = (int)player.hp - activeHearts.Count;
        else
            dif = -activeHearts.Count;
        if(dif == 0) return;

        if(dif > 0) { //Add Hearts
            for(int i = 0; i < dif; i++) {
                Control newHP;
                if(inactiveHearts.Count == 0) {
                    newHP = hpRef.Instantiate<Control>();
                    heartBox.AddChild(newHP);
                    newHP.AddUserSignal("reborn_completed", new Godot.Collections.Array() { new Godot.Collections.Dictionary() { { "name", "heart" }, { "type", (int)Variant.Type.Object } } });
                    newHP.Connect("reborn_completed", Callable.From(() => SetHeartBeatSeek(newHP)));//new Callable(this, MethodName.SetHeartBeatSeek));
                } else {
                    newHP = inactiveHearts[inactiveHearts.Count - 1];
                    inactiveHearts.RemoveAt(inactiveHearts.Count - 1);
                }
                activeHearts.Add(newHP);
                AnimationTree newHPAnimTree = newHP.GetNode<AnimationTree>("AnimationTree");
                ((AnimationNodeStateMachinePlayback)(newHPAnimTree.Get("parameters/playback"))).Travel("Reborn");
                newHPAnimTree.Set("parameters/Reborn/Seek/seek_position", (float)1 - (float)i * 0.075f);
            }
        } else {    //Remove Hearts
            for(int i = 0; i > dif && activeHearts.Count > 0; i--) {
                Control hpToRemove = activeHearts[activeHearts.Count - 1];
                activeHearts.Remove(hpToRemove);
                inactiveHearts.Add(hpToRemove);
                AnimationTree hpToRemoveAnimTree = hpToRemove.GetNode<AnimationTree>("AnimationTree");
                ((AnimationNodeStateMachinePlayback)(hpToRemoveAnimTree.Get("parameters/playback"))).Travel("Die");
                hpToRemoveAnimTree.Set("parameters/Die/Seek/seek_position", (float)1 + (float)i * 0.125f);
            }
        }
        ResetAnchors();
    }
    public void SetHeartBeatSeek(Control heart) {
        AnimationTree heartAnimTree = heart.GetNode<AnimationTree>("AnimationTree");
        if(activeHearts.Count > 0 && activeHearts[0] != heart) {
            AnimationNodeStateMachinePlayback firstHeartStateMachine =
                (AnimationNodeStateMachinePlayback)(activeHearts[0].GetNode<AnimationTree>("AnimationTree").Get("parameters/playback"));
            heartAnimTree.Set("parameters/Beat/Seek/seek_position", firstHeartStateMachine.GetCurrentPlayPosition());
        } else {
            heartAnimTree.Set("parameters/Beat/Seek/seek_position", 0);
        }

    }
    public void ResetAnchors() {
        if(player == null) return;
        switch(player.playerNum) {
            case 1:
                SetAnchorsAndOffsetsPreset(LayoutPreset.TopLeft, LayoutPresetMode.KeepSize, 3);
                break;
            case 2:
                //heartBox.GrowHorizontal = GrowDirection.Begin;
                //Scale = new Vector2(-1, 1);
                SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight, LayoutPresetMode.KeepSize, 3);
                break;
            case 3:
                SetAnchorsAndOffsetsPreset(LayoutPreset.BottomLeft, LayoutPresetMode.KeepSize, 3);
                break;
            case 4:
                SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight, LayoutPresetMode.KeepSize, 3);
                break;
        }
    }
}
