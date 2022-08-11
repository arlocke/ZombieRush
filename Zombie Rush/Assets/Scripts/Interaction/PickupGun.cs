using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGun : Pickup{
    private void Start() {
        if(payload)
            itemSize = payload.GetComponent<GunBase>().itemSize;
    }
    public override void Interact(PlayerController pc) {
        pc.EquipGun(payload.GetComponent<GunBase>());
        Destroy(gameObject);
    }
}
