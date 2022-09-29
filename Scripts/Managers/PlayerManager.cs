using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class PlayerManager:Node {
    [Export]
    List<Player> players;
    CanvasLayer gameGUI;
    CameraGame cam;
    [Export]
    PackedScene playerGUIRef;
    public override void _Ready() {
        gameGUI = GetTree().GetNodesInGroup("Canvases")[0] as CanvasLayer;
        cam = GetNode<CameraGame>("CameraGame");
        Array newPlayers = GetTree().GetNodesInGroup("Players");
        players = new List<Player>();
        foreach(Player p in newPlayers) {
            AddPlayer(p);
        }
    }
    public void AddPlayer(Player p) {
        players.Add(p);
        p.playerNum = (byte)players.Count;
        GUIGamePlayer pGUI = playerGUIRef.Instance<GUIGamePlayer>();
        gameGUI.AddChild(pGUI);
        pGUI.SetupPlayer(p);
        cam.targets.Add(p);
        p.cam = cam;
        p.inputDeviceType = p.playerNum == 1 ? InputDeviceType.MouseKeyboard : InputDeviceType.Controller;
    }

}
