using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : Node
{
    [Export]
    List<PackedScene> playersToLoad = new List<PackedScene>();
    [Export]
    List<Player> players;
    CanvasLayer gameGUI;
    CameraGame cam;
    [Export]
    public PackedScene playerGUIRef;
    public override void _Ready()
    {
        gameGUI = GetTree().GetNodesInGroup("Canvases")[0] as CanvasLayer;
        cam = GetNode<CameraGame>("CameraGame");
        players = new List<Player>();
        Array foundPlayers = GetTree().GetNodesInGroup("Players");
        if (foundPlayers.Count > 0)
        { //There are already spawned players, no need to make more. For testing and stuff
            foreach (Player p in foundPlayers)
            {
                AddPlayer(p);
            }
        }
        else if (playersToLoad.Count > 0)
        {
            Array spawnPoints = GetTree().GetNodesInGroup("PlayerSpawns");
            YSort playersYSort = GetTree().GetNodesInGroup("PlayersYSort")[0] as YSort;
            for (int i = 0; i < playersToLoad.Count; i++)
            {
                Player newPlayer = playersToLoad[i].Instance<Player>();
                playersYSort.AddChild(newPlayer);
                if (spawnPoints.Count > 0)
                    newPlayer.GlobalPosition = (spawnPoints[i % spawnPoints.Count] as Node2D).GlobalPosition;
                AddPlayer(newPlayer);
            }
        }
    }
    public void AddPlayer(Player p)
    {
        players.Add(p);
        p.playerNum = (byte)players.Count;
        GUIGamePlayer pGUI = playerGUIRef.Instance<GUIGamePlayer>();
        gameGUI.AddChild(pGUI);
        pGUI.SetupPlayer(p);
        cam.targets.Add(p);
        p.cam = cam;
        p.inputDeviceType = p.playerNum == 1 ? InputDeviceType.MouseKeyboard : InputDeviceType.Controller;
        p.playerManager = this;
    }
}