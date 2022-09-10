using Godot;
public class FPSCounter:Label {
    public override void _Process(float delta) {
        Text = "FPS: " + Engine.GetFramesPerSecond();
    }
}
