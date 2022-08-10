using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGun : Pickup
{
    public GunBase gun;

    private void Start() {
        transform.GetChild(0).GetComponent<GunBase>();
    }

    public override void Interact(PlayerController pc) {
        gun.transform.SetParent(pc.arm.transform, false);
        gun.transform.localPosition = pc.hand1Point.localPosition + gun.hand1Point.localPosition * -1;
        //something for 2 handed guns
        pc.heldGun = gun;
        pc.holding = true;
        pc.spriteRenderer.flipX = false; //
        Destroy(gameObject);
    }
}
