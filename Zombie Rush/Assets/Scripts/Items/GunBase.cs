using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class GunBase : MonoBehaviour
{
    //public string name;
    public FireMode fireMode;
    public GunState currentState;
    public int tier;
    public float damage;
    public float fireRate;
    public float fireTimer;
    //public bool triggerHeld; //Placeholder, research into interactions in PlayerInputActions
    public float range;
    public float bulletSpeed;
    public int magMaxSize;
    public int currentMagSize;
    public GameObject bulletRef;
    public AmmoType ammoType; // enum in BulletBase
    public Transform bulletSpawnPoint;
    public Transform hand1Point;
    public Transform hand2Point;
    public int itemSize;
    public CreatureBase owner;

    private void Start() {
        ammoType = bulletRef.GetComponent<BulletBase>().ammoType;
    }

    public void FixedUpdate() {
        if(fireTimer > 0){
        fireTimer -= Time.deltaTime;
        }
        if (fireTimer <= 0) {
            if(currentState == GunState.TriggerHeld && currentMagSize > 0) {
                fireTimer += fireRate;

                BulletBase newBullet = Instantiate(bulletRef, bulletSpawnPoint.position, bulletSpawnPoint.rotation).GetComponent<BulletBase>();
                if(transform.lossyScale.x < 0)
                    newBullet.transform.Rotate(new Vector3(0,0,180),Space.World);
                newBullet.speed = bulletSpeed;
                newBullet.distanceRemaining = range;
                newBullet.damage = damage;
                newBullet.owner = owner; //sets bullet owner to this gun owner
                currentMagSize--;

                if (currentMagSize <= 0) {
                    currentState = GunState.Empty;
                } else if(fireMode == FireMode.Single || fireMode == FireMode.Burst) {
                    currentState = GunState.Idle;
                }
                
            } else {
                fireTimer = 0;
            }
        }
    }

    public void PullTrigger() {
        if(currentState == GunState.Idle) {
            currentState = GunState.TriggerHeld;
        }
    }

    public void ReleaseTrigger() {
        if (currentState == GunState.TriggerHeld && currentMagSize > 0) {
            currentState = GunState.Idle;
        }
    }

    public void StartReload() {
        currentState = GunState.Reloading;
    }

    public void FinishReload(int ammoToAdd) {
        currentState = GunState.Idle;

        currentMagSize = ammoToAdd;
    }
}
