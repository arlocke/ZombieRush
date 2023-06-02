using Godot;
public enum FireMode {
    Single,
    Auto,
    Burst,
}

public enum GunState {
    Idle,
    TriggerHeld,
    Reloading,
    Empty,
    //cancelling
}

public partial class Gun:Weapon {
    [Export]
    public FireMode fireMode;
    public GunState state;
    public string animState = "Idle";
    [Export]
    public float fireTimer;
    [Export]
    public float timeSinceLastShot;
    public int sprayCount;
    public float sprayTimer;
    [Export]
    public float sprayRecoverTime = 0.5f;
    [Export]
    public float inaccuracyMin = 1;     //Maximum innacuracy of shots at the beginning of a spray in degrees away from the aim angle
    [Export]
    public float inaccuracyMax = 20;     //Maximum innacuracy of shots at the end of a spray in degrees away from the aim angle
    [Export]
    public float range;
    [Export]
    public float bulletSpeed;
    [Export]
    public int magMaxSize;
    [Export]
    public int currentMagSize;
    [Export]
    public AmmoType ammoType; // enum in Bullet
    ///////////References
    [Export]
    public PackedScene muzzleFlashRef;
    [Export]
    public PackedScene bulletRef;
    ////////////Components
    public Node2D bulletSpawnSocket;

    public override void _Ready() {
        base._Ready();
        UpdateAnimTreeVars();
        bulletSpawnSocket = GetNode<Node2D>("BulletSpawnSocket");
    }
    public override void _PhysicsProcess(double dt) {

        if(fireTimer > 0)
            fireTimer -= (float)dt;
        if(fireTimer <= 0) {
            if(currentMagSize > 0 && state == GunState.TriggerHeld) {
                Shoot();
            } else {
                timeSinceLastShot += (float)dt;
            }

        }
    }
    public override bool CanUse() {
        return fireTimer <= 0 && currentMagSize > 0;
    }
    public override void Use() {
        base.Use();
        PullTrigger();
    }
    public override void CancelUse() {
        base.CancelUse();
        ReleaseTrigger();
    }
    public override void Custom() {
        base.Custom();
        if(currentMagSize < magMaxSize && IsInstanceValid(holder)) {
            int amt = (holder as Player).RemoveAmmo(ammoType, magMaxSize - currentMagSize);
            //reload animation
            FinishReload(currentMagSize + amt);
        }
    }
    public void PullTrigger() {
        if(state == GunState.Idle) {
            state = GunState.TriggerHeld;
        }
    }
    public void ReleaseTrigger() {
        if(state == GunState.TriggerHeld && currentMagSize > 0) {
            state = GunState.Idle;
        }
    }
    public void Shoot() {
        fireTimer += attackRate;
        if(timeSinceLastShot > 0)
            sprayCount = (int)((float)sprayCount * (1 - (Mathf.Min(timeSinceLastShot, sprayRecoverTime)) / sprayRecoverTime));

        timeSinceLastShot = fireTimer * -1;
        //Update spray count based on timer
        float bulletRot = bulletSpawnSocket.GlobalRotation + Mathf.DegToRad((float)GD.RandRange(-1, 1) * Mathf.Lerp(inaccuracyMin, inaccuracyMax, Mathf.Pow((float)sprayCount / (float)magMaxSize, 2)));

        Bullet newBullet = bulletRef.Instantiate<Bullet>();
        GetTree().Root.AddChild(newBullet);
        newBullet.GlobalPosition = bulletSpawnSocket.GlobalPosition;
        newBullet.GlobalRotation = bulletRot;
        newBullet.speed = bulletSpeed;
        newBullet.distanceRemaining = range;
        newBullet.damage = damage;
        newBullet.origin = holder; //sets bullet owner to this gun owner

        Sprite2D newMuzzleFlash = muzzleFlashRef.Instantiate<Sprite2D>();
        GetTree().Root.AddChild(newMuzzleFlash);
        newMuzzleFlash.GlobalPosition = bulletSpawnSocket.GlobalPosition;
        newMuzzleFlash.GlobalRotation = bulletSpawnSocket.GlobalRotation;

        if(Scale.X < 0) {
            newBullet.Rotate(Mathf.Pi);
            newMuzzleFlash.Rotate(Mathf.Pi);
        }

        sprayCount++;
        currentMagSize--;
        if(currentMagSize <= 0) {
            state = GunState.Empty;
        } else if(fireMode == FireMode.Single || fireMode == FireMode.Burst) {
            state = GunState.Idle;
        }
        SetAnimState("ActionBack");
        animTree.Set("parameters/conditions/Loaded", currentMagSize > 0);
    }
    public void StartReload() {
        state = GunState.Reloading;
    }
    public void FinishReload(int ammoToAdd) {
        state = GunState.Idle;
        sprayCount = 0;
        currentMagSize = ammoToAdd;
        animTree.Set("parameters/conditions/Loaded", currentMagSize > 0);
    }
    public override void SetFacingRight(bool r) {
        if(r != facingRight) {
            facingRight = r;
            ShowBehindParent = facingRight;
            //ZIndex = facingRight ? -1 : 1;
            animTree.Set("parameters/Idle/BlendSpace1D/blend_position", facingRight ? 1 : -1);
            animTree.Set("parameters/ActionBack/BlendSpace1D/blend_position", facingRight ? 1 : -1);
            animTree.Set("parameters/ActionForward/BlendSpace1D/blend_position", facingRight ? 1 : -1);
        }
    }
    public void UpdateAnimTreeVars() {
        if(animTree != null) {
            animTree.Set("parameters/" + animState + "/TimeScale/scale", Mathf.Max(1 / attackRate, 5));
        }
    }
    public void UpdateAnimFacingRight() {
        switch(state) {
            case GunState.Empty:
                break;
        }
    }
    public void SetAnimState(string s) {
        animState = s;
        animStateMachine.Travel(animState);
        animTree.Set("parameters/" + animState + "/BlendSpace1D/blend_position", facingRight ? 1 : -1);
    }
}