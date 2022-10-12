using Godot;
using System.Collections.Generic;

public class WeaponMelee:Weapon {
    [Export]
    public List<Hitbox> hitboxes = new List<Hitbox>();
    public override void _Ready() {
        base._Ready();
        Godot.Collections.Array c = GetChildren();
        foreach(Node n in c) {
            if(n is Hitbox) {
                hitboxes.Add(n as Hitbox);
                (n as Hitbox).damage = damage;
            }
        }
        DisableHitboxes();
    }
    public override void Setup(Creature c) {
        base.Setup(c);
        if(!IsInstanceValid(c)) return;
        foreach(Hitbox h in hitboxes) {
            h.origin = c;
        }
    }
    public override void Use() {
        if(IsInstanceValid(holder)) {
            if(!(holder as Player).animPlayerArms.IsPlaying()) {
                (holder as Player).UpdateArmDirection(true, true);
                (holder as Player).animPlayerArms.Play("PlayerMeleeAttack");
            }
        }
    }
    public void EnableHitboxes() {
        foreach(Hitbox h in hitboxes) {
            h.Enable();
        }
    }
    public void DisableHitboxes() {
        foreach(Hitbox h in hitboxes) {
            h.Disable();
        }
    }
}
