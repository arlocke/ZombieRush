using Godot;
using System.Collections.Generic;

public class GlobalData:Node {
    public static Dictionary<ItemTier, Color> tierColors;
    public override void _Ready() {
        base._Ready();
        tierColors = new Dictionary<ItemTier, Color>{
            {ItemTier.Common,Colors.White},
            {ItemTier.Uncommon,Colors.LightBlue},
            {ItemTier.Rare,Colors.Blue},
            {ItemTier.Epic,Colors.Fuchsia},
            {ItemTier.Legendary,Colors.Gold},
        };
    }
}
