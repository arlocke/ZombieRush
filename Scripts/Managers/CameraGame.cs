using Godot;
using System.Collections.Generic;

public partial class CameraGame:Camera2D {
    public List<Creature> targets = new List<Creature>();
    [Export]
    public float moveSpeed;
    [Export]
    Vector2 padding = new Vector2(2, 2);
    public override void _PhysicsProcess(double dt) {
        GlobalPosition = GetPosBetweenTargets();
    }
    public Vector2 GetPosBetweenTargets() {
        if(targets.Count == 0) return GlobalPosition;
        bool success = false;
        Vector2 min, max;
        if(IsInstanceValid(targets[0])) {
            Rect2 cSpriteRect = targets[0].GetSpriteRectWorld(targets[0].GlobalPosition);
            min = cSpriteRect.Position;
            max = cSpriteRect.End;
            success = true;
        } else {
            min = new Vector2(Mathf.Inf, Mathf.Inf);
            max = new Vector2(-Mathf.Inf, -Mathf.Inf);
        }
        for(int i = 1; i < targets.Count; i++) {
            if(IsInstanceValid(targets[i])) {
                Creature c = targets[i];
                Rect2 cSpriteRect = c.GetSpriteRectWorld(c.GlobalPosition);
                if(cSpriteRect.Position.X < min.X)
                    min.X = cSpriteRect.Position.X;
                if(cSpriteRect.End.X > max.X)
                    max.X = cSpriteRect.End.X;

                if(cSpriteRect.Position.Y < min.Y)
                    min.Y = cSpriteRect.Position.Y;
                if(cSpriteRect.End.Y > max.Y)
                    max.Y = cSpriteRect.End.Y;
                success = true;
            } else {
                targets.RemoveAt(i);
                i--;
            }
        }

        return success ? new Vector2((min.X + max.X) / 2, (min.Y + max.Y) / 2) : GlobalPosition;
    }
    public Rect2 GetRectWorld() {
        Vector2 bbSize = (GetWindow().Size) * (Zoom);
        return new Rect2(GlobalPosition - (bbSize / 2), bbSize);
    }
    public Rect2 GetPaddedRectWorld() {
        Vector2 bbSize = (GetWindow().Size) * (Zoom) - padding * 2;
        return new Rect2(GlobalPosition - (bbSize / 2), bbSize);
    }
    ///  <summary>Checks if the given creature is inside the camera's bounding box at the given position.</summary>
    /// <param name="p">The position of the creature. Might be different from the creature's actual Position if checking a hypothetical.</param>
    public bool CreatureInBB(Creature c, Vector2 p) {
        return GetRectWorld().Encloses(c.GetSpriteRectWorld(p));
    }
    ///  <summary>Returns the velocity that this camera will allow the given creature to move at, given the velocity that the creature wants to move at.</summary>
    /// <param name="dVel">The desired velocity that the creature wants to move with.</param>
    public Vector2 AllowedVel(Creature c, Vector2 dVel, float dt) {
        Rect2 cdRect = c.GetSpriteRectWorld(c.GlobalPosition + dVel * dt);
        Rect2 cRect = c.GetSpriteRectWorld(c.GlobalPosition);
        Rect2 camRect = GetPaddedRectWorld();
        if(dVel.X > 0 && cdRect.End.X > camRect.End.X) {
            dVel.X = camRect.End.X - cRect.End.X;
        } else if(dVel.X < 0 && cdRect.Position.X < camRect.Position.X) {
            dVel.X = camRect.Position.X - cRect.Position.X;
        }
        if(dVel.Y > 0 && cdRect.End.Y > camRect.End.Y) {
            dVel.Y = camRect.End.Y - cRect.End.Y;
        } else if(dVel.Y < 0 && cdRect.Position.Y < camRect.Position.Y) {
            dVel.Y = camRect.Position.Y - cRect.Position.Y;
        }
        return dVel;
    }
}
