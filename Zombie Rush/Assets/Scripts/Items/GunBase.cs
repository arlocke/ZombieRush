using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode {
    Single,
    Auto,
    Burst,
}

public class GunBase : MonoBehaviour
{
    //public string name;
    public FireMode fireMode;
    public int tier;
    public float damage;
    public float fireRate;
    public float fireTimer;
    public bool triggerHeld; //Placeholder, research into interactions in PlayerInputActions
    public float range;
    public float bulletSpeed;
    public int magMaxSize;
    public int currentMagSize;
    public GameObject bulletRef;
    public Transform bulletSpawnPoint;
    public Transform hand1Point;
    public Transform hand2Point;

    
    public void FixedUpdate() {
        if(fireTimer > 0){
        fireTimer -= Time.deltaTime;
        }
        if (fireTimer <= 0) {
            if (triggerHeld && currentMagSize > 0) {
                fireTimer += fireRate;

                BulletBase newBullet = Instantiate(bulletRef, bulletSpawnPoint.position, bulletSpawnPoint.rotation).GetComponent<BulletBase>();

                newBullet.speed = bulletSpeed;
                newBullet.distanceRemaining = range;
                newBullet.damage = damage;

                if(fireMode == FireMode.Single || fireMode == FireMode.Burst) {
                    triggerHeld = false;
                }

                currentMagSize--;
            } else {
                fireTimer = 0;
            }
        }
    }

    public void PullTrigger() {
        triggerHeld = true;
    }

    public void ReleaseTrigger() {
        triggerHeld = false;
    }
}
