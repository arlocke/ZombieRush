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

public class Gun:Weapon {
    [Export]
    public FireMode fireMode;
    public GunState state;
    public string animState = "Idle";
    public float fireTimer;
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
    public override void _PhysicsProcess(float dt) {
        if(fireTimer > 0) {
            fireTimer -= dt;
        }
        if(fireTimer <= 0) {
            if(state == GunState.TriggerHeld && currentMagSize > 0) {
                Shoot();
            } else {
                fireTimer = 0;
            }
        }
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

        Bullet newBullet = bulletRef.Instance<Bullet>();
        GetTree().Root.AddChild(newBullet);
        newBullet.GlobalPosition = bulletSpawnSocket.GlobalPosition;
        newBullet.GlobalRotation = bulletSpawnSocket.GlobalRotation;
        newBullet.speed = bulletSpeed;
        newBullet.distanceRemaining = range;
        newBullet.damage = damage;
        newBullet.origin = holder; //sets bullet owner to this gun owner

        Sprite newMuzzleFlash = muzzleFlashRef.Instance<Sprite>();
        GetTree().Root.AddChild(newMuzzleFlash);
        newMuzzleFlash.GlobalPosition = bulletSpawnSocket.GlobalPosition;
        newMuzzleFlash.GlobalRotation = bulletSpawnSocket.GlobalRotation;

        if(Scale.x < 0) {
            newBullet.Rotate(Mathf.Pi);
            newMuzzleFlash.Rotate(Mathf.Pi);
        }

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