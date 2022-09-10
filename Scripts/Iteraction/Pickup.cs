using Godot;
public class Pickup:Interactable {
    [Export]
    public Node2D payload;   //Whatever it is the player is picking up.
    public Vector2 payloadOffset = new Vector2();
    [Export]
    public bool canDespawn; //can despawn
    [Export]
    public bool storeable; //has to be pickedup with e and can be put in inventory
    //Movement vars for when pickups drop
    [Export]
    public bool moving;
    [Export]
    public Vector3 vel;     //Velocity
    [Export]
    public float startingZVel = 2f;
    [Export]
    public float startingHVel = 0.15f;  //Horizontal velocity. Relative to X-Y Plane
    [Export]
    public float startingHeight = 0.6f;
    [Export]
    public float bounceVelAbsorbtion = 0.6f;
    [Export]
    public float bounceVelMin = -0.05f;
    [Export]
    public float gravity = 185f;
    [Export]
    public float height;    //Theoretical Z position that we're simulating
    [Export]
    public float shadowCastMaxHeight = 8;
    [Export]
    public float itemSizeMaxShadowFactor = 9;
    [Export]
    public float itemSize = 1;
    public Sprite shadowSprite;

    public override void _Ready() {
        base._Ready();
        shadowSprite = GetNode<Sprite>("ShadowSprite");
        foreach(Node n in GetChildren()) {
            if(n.IsInGroup("Items")) {
                payload = (Node2D)n;
                break;
            }
        }
        if(payload != null) {
            SetPayloadOffset();
        }
    }
    public override void _PhysicsProcess(float dt) {
        if(payload == null) return;
        if(moving) {
            vel.z -= gravity * dt;
            height += vel.z * dt;
            float targetRot = (payload.Rotation < Mathf.Deg2Rad(90) || payload.Rotation > 270) ? 0 : Mathf.Deg2Rad(180);
            payload.Rotation = Mathf.Lerp(payload.Rotation, targetRot, dt * 3);
            if(height <= 0) {    //Hits the ground
                if(vel.z < bounceVelMin) {   //Still got some bounce left
                    vel.z *= -bounceVelAbsorbtion;
                    height = 0;
                } else {
                    moving = false;
                    vel = new Vector3(0, 0, 1);
                    height = 0;
                    payload.Rotation = targetRot;
                    startingHeight = 4;
                    payload.Position = payloadOffset;
                    return;
                }
            }
            Position = new Vector2(Position.x + vel.x * dt, Position.y + vel.y * dt);
            payload.Position = new Vector2(0, -height) + payloadOffset;
        } else {
            float bobDstFactor = height / startingHeight;
            height += vel.z * dt * 5;
            if(vel.z < 0 && height < 0) {
                vel.z = 1;
            } else if(vel.z > 0 && height > startingHeight) {
                vel.z = -1;
            }
            //\left\{x<0.5:\left(\frac{\left(x\cdot2\right)^{2}}{2}\right),1-\left(\frac{\left(\left(1-x\right)\cdot2\right)^{2}}{2}\right)\right\}
            float easeDst;
            if(bobDstFactor < 0.5) {
                easeDst = Mathf.Pow(bobDstFactor * 2, 2) / 2;
            } else {
                easeDst = 1 - Mathf.Pow((1 - bobDstFactor) * 2, 2) / 2;
            }
            payload.Position = new Vector2(0, -easeDst * startingHeight) + payloadOffset;
        }
        float maxShadowSize = itemSize / itemSizeMaxShadowFactor;
        shadowSprite.Frame = (int)(Mathf.Lerp(0, maxShadowSize, 1 - Mathf.Clamp(height / shadowCastMaxHeight, 0, 1)) * 32f);
    }
    public void DropRandomDirection(bool onGround, float h) {
        moving = true;
        if(!onGround)
            startingHeight = h;//onGround ? 0.125f : 0.5f;
        SetPayloadOffset();
        Vector2 heightOffset;
        if(!onGround) {
            height = startingHeight + payloadOffset.y;
            heightOffset = new Vector2(0, height) - payloadOffset;
            Position += heightOffset;
        } else {
            heightOffset = new Vector2(0, height);
        }
        payload.Position = -heightOffset + payloadOffset;
        float a = (float)GD.RandRange(0.0, 6.28318530718);   //6.28318530718 is 2 * pi, which is the number of radians in a circle
        vel = new Vector3(Mathf.Sin(a) * startingHVel, Mathf.Cos(a) * startingHVel / 1.41421356f, onGround ? startingZVel * 0.75f : startingZVel);    //1.41421356 is the square root of 2 which makes up for the 45 degree angle perspective
    }

    void SetPayloadOffset() {
        Sprite spr;
        if(payload is Sprite)
            spr = payload as Sprite;
        else
            spr = payload.GetNode<Sprite>("Sprite");

        payloadOffset = new Vector2(0, -spr.Texture.GetHeight() / spr.Vframes / 2);
    }
}
