using Godot;
using System.Collections.Generic;

public class CameraGame:Camera2D {
    [Export]
    public List<Creature> targets = new List<Creature>();
    [Export]
    public float moveSpeed;
    [Export]
    Vector2 padding = new Vector2(2, 2);
    public override void _PhysicsProcess(float dt) {
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
                if(cSpriteRect.Position.x < min.x)
                    min.x = cSpriteRect.Position.x;
                if(cSpriteRect.End.x > max.x)
                    max.x = cSpriteRect.End.x;

                if(cSpriteRect.Position.y < min.y)
                    min.y = cSpriteRect.Position.y;
                if(cSpriteRect.End.y > max.y)
                    max.y = cSpriteRect.End.y;
                success = true;
            } else {
                targets.RemoveAt(i);
                i--;
            }
        }

        return success ? new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2) : GlobalPosition;
    }
    public Rect2 GetRectWorld() {
        Vector2 bbSize = (GetViewport().Size) * (Zoom);
        return new Rect2(GlobalPosition - (bbSize / 2), bbSize);
    }
    public Rect2 GetPaddedRectWorld() {
        Vector2 bbSize = (GetViewport().Size) * (Zoom) - padding * 2;
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
        if(dVel.x > 0 && cdRect.End.x > camRect.End.x) {
            dVel.x = camRect.End.x - cRect.End.x;
        } else if(dVel.x < 0 && cdRect.Position.x < camRect.Position.x) {
            dVel.x = camRect.Position.x - cRect.Position.x;
        }
        if(dVel.y > 0 && cdRect.End.y > camRect.End.y) {
            dVel.y = camRect.End.y - cRect.End.y;
        } else if(dVel.y < 0 && cdRect.Position.y < camRect.Position.y) {
            dVel.y = camRect.Position.y - cRect.Position.y;
        }
        return dVel;
    }

}
