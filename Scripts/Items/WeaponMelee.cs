using Godot;
using System;

public class WeaponMelee:Weapon {
    public override void Use() {
        if(IsInstanceValid(holder)) {
            if(!(holder as Player).animPlayerArms.IsPlaying())
                (holder as Player).animPlayerArms.Play("MeleeAttack");
        }
    }
}
