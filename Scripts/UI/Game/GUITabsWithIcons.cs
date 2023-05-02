using Godot;
using Godot.Collections;

public partial class GUITabsWithIcons:TabContainer {
    [Export]
    Array<Texture2D> _icons;
    public override void _Ready() {
        base._Ready();
        for(int i = 0; i < GetTabCount(); i++) {
            SetTabIcon(i, _icons[i]);
            SetTabTitle(i, "");
        }
    }
}
