using Godot;
using System.Collections.Generic;

public class GUITabsWithIcons:TabContainer {
    [Export]
    List<Texture> icons;
    public override void _Ready() {
        base._Ready();
        for(int i = 0; i < GetTabCount(); i++) {
            SetTabIcon(i, icons[i]);
            SetTabTitle(i, "");
        }
    }
}
