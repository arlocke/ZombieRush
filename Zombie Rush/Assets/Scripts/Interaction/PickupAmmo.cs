using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAmmo : AutoPickup
{
    public int amt;
    public AmmoType ammoType;

    public override void Interact(PlayerController pc) {

        int remainder = pc.AddAmmo(ammoType, amt);
        if (remainder < amt) {
            if (remainder > 0) {
                DropRandomDirection(true);
                amt = remainder;
            } else {
                Destroy(gameObject);
            }
        }
       
    }
}
