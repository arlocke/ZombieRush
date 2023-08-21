using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public enum FloorLayerType
{
    Base,
    Outer
}
public partial class Structure : Area2D
{
    [Export]
    PackedScene scene;
    TileMap tm;
    /*  Layers in the TileMap associated with certain floors and floor layer types
    *   The First List is the list of Floors in the structure.
    *   The Dictionary is the different floor types eg "Base" or "Outer"
    *   The List in the dictionary is the list of layer indexes from the TileMap that are in this floor and of the Floor Layer Type
    */
    List<Dictionary<FloorLayerType, List<byte>>> floors = new List<Dictionary<FloorLayerType, List<byte>>>();
    public int currentFloor = 0;
    public List<Player> overlappedPlayers = new List<Player>();
    public float topAlpha = 1;
    public override void _Ready()
    {
        Connect("body_entered", new Callable(this, "OnBodyEntered"));
        Connect("body_exited", new Callable(this, "OnBodyExited"));
        tm = GetNode("TileMap") as TileMap;//scene.Instantiate<TileMap>();
        //AddChild(tm);

        for (int i = 0; i < tm.GetLayersCount(); i++)
        {
            string layerName = tm.GetLayerName(i);
            if (layerName.StartsWith("Floor"))
            {
                byte layerFloor = Convert.ToByte(new string(layerName.Substring(5, layerName.Length > 6 ? 2 : 1).Where(c => char.IsDigit(c)).ToArray()));
                FloorLayerType layerFL = layerName.Contains("Outer") ? FloorLayerType.Outer : FloorLayerType.Base;
                if (floors.Count <= layerFloor)      //We don't have this floor in the list, so add it
                    floors.Add(new Dictionary<FloorLayerType, List<byte>>());
                if (!floors[layerFloor].ContainsKey(layerFL)) //We don't have that key, so add it
                    floors[layerFloor].Add(layerFL, new List<byte>());

                floors[layerFloor][layerFL].Add((byte)i);   //Finally add the layer to this overcomplicated data structure
            }
        }
        HideBaseLayers();
    }
    public override void _PhysicsProcess(double dt)
    {
        if (overlappedPlayers.Count > 0 && topAlpha > 0.5f)
        {
            topAlpha = Math.Max(topAlpha - (float)dt * 3, 0.5f);
            SetUpperLayersAlpha();
        }
        else if (overlappedPlayers.Count == 0 && topAlpha < 1)
        {
            topAlpha = Math.Min(topAlpha + (float)dt * 3, 1);
            SetUpperLayersAlpha();
        }
    }
    public void EnterPlayer(Player p, int floor = 0)
    {
        SetFloor(floor);
        p.SetFloor(floor + 1);
    }
    public void ExitPlayer(Player p, int floor = 0)
    {
        SetFloor(floors.Count - 1);
        p.SetFloor(0);
    }
    public void SetFloor(int f)
    {
        currentFloor = f;
        HideBaseLayers(currentFloor);
        HideOuterLayersAbove(currentFloor);
    }
    //Hide all Floor Base layers except the specified exception floor
    public void HideBaseLayers(int exception = -1000)
    {
        if (exception == -1000) exception = floors.Count - 1;
        for (int i = 0; i < floors.Count; i++)
        {
            Dictionary<FloorLayerType, List<byte>> fl = floors[i];
            if (fl.ContainsKey(FloorLayerType.Base))
            {
                foreach (byte li in fl[FloorLayerType.Base])
                {
                    tm.SetLayerEnabled(li, i == exception);
                }
            }
        }
    }
    //Hide all Floor Outer layers above and on the specified floor
    public void HideOuterLayersAbove(int f)
    {
        for (int i = 0; i < floors.Count; i++)
        {
            Dictionary<FloorLayerType, List<byte>> fl = floors[i];
            if (fl.ContainsKey(FloorLayerType.Outer))
            {
                foreach (byte li in fl[FloorLayerType.Outer])
                {
                    tm.SetLayerEnabled(li, i < f);
                }
            }
        }
    }
    public void SetUpperLayersAlpha()
    {
        for (int i = 1; i < floors.Count; i++)
        {
            Dictionary<FloorLayerType, List<byte>> f = floors[i];
            foreach (List<byte> flt in f.Values)
            {
                foreach (byte li in flt)
                {
                    tm.SetLayerModulate(li, new Color(1, 1, 1, topAlpha));
                }
            }
        }
    }
    public virtual void OnBodyEntered(CollisionObject2D other)
    {
        if (other != null && other.IsInGroup("Players"))
        {
            Player p = (Player)other;
            if (overlappedPlayers.Contains(p))
                return;
            else
                overlappedPlayers.Add(p);
            //Fade out
        }
    }
    public virtual void OnBodyExited(CollisionObject2D other)
    {
        if (other != null && other.IsInGroup("Players"))
        {
            Player p = (Player)other;
            overlappedPlayers.Remove(p);
        }
    }
}
