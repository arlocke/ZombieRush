using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGun : Pickup{

    public new void Start() {
        base.Start();
        if(payload)
            itemSize = payload.GetComponent<GunBase>().itemSize;
    }
    public override void Interact(PlayerController pc) {
        pc.EquipGun(payload.GetComponent<GunBase>());
        Destroy(gameObject);
    }
}
