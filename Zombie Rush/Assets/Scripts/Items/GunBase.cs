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
    public GunState state;
    public bool facingRight = true;
    public int tier;
    public float damage;
    public float fireRate;
    public float fireTimer;
    //public bool triggerHeld; //Placeholder, research into interactions in PlayerInputActions
    public float range;
    public float bulletSpeed;
    public int magMaxSize;
    public int currentMagSize;
    public AmmoType ammoType; // enum in BulletBase
    ///////////References
    [Tooltip("Muzzle Flash Prefab")]
    public GameObject muzzleFlashRef;
    [Tooltip("Bullet Prefab")]
    public GameObject bulletRef;
    ////////////Components
    public Animator animator;
    public Transform bulletSpawnPoint;
    public Transform hand1Point;
    public Transform hand2Point;
    public int itemSize;
    public CreatureBase owner;

    private void Start() {
        ammoType = bulletRef.GetComponent<BulletBase>().ammoType;
        animator = GetComponent<Animator>();
        UpdateAnimator();
        SetFacingRight(true);
    }
    public void FixedUpdate() {
        if(fireTimer > 0){
        fireTimer -= Time.deltaTime;
        }
        if (fireTimer <= 0) {
            if(state == GunState.TriggerHeld && currentMagSize > 0) {
                Shoot();
            } else {
                fireTimer = 0;
            }
        }
    }
    public void PullTrigger() {
        if(state == GunState.Idle) {
            state = GunState.TriggerHeld;
        }
    }
    public void ReleaseTrigger() {
        if (state == GunState.TriggerHeld && currentMagSize > 0) {
            state = GunState.Idle;
        }
    }
    public void Shoot(){
        fireTimer += fireRate;

        BulletBase newBullet = Instantiate(bulletRef, bulletSpawnPoint.position, bulletSpawnPoint.rotation).GetComponent<BulletBase>();
        newBullet.speed = bulletSpeed;
        newBullet.distanceRemaining = range;
        newBullet.damage = damage;
        newBullet.owner = owner; //sets bullet owner to this gun owner

        GameObject newMuzzleFlash = Instantiate(muzzleFlashRef, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        if (transform.lossyScale.x < 0){
            newBullet.transform.Rotate(new Vector3(0, 0, 180), Space.World);
            newMuzzleFlash.transform.Rotate(new Vector3(0, 0, 180), Space.World);
        }

        currentMagSize--;
        if (currentMagSize <= 0) {
            state = GunState.Empty;
        } else if (fireMode == FireMode.Single || fireMode == FireMode.Burst) {
            state = GunState.Idle;
        }
        animator.Play("ActionBackR",0);
        animator.SetBool("Loaded",currentMagSize > 0);
    }
    public void StartReload() {
        state = GunState.Reloading;
    }
    public void FinishReload(int ammoToAdd) {
        state = GunState.Idle;

        currentMagSize = ammoToAdd;
        animator.SetBool("Loaded", currentMagSize > 0);
    }
    public void SetFacingRight(bool r){
        if(r != facingRight){
            facingRight = r;
            GetComponent<SpriteRenderer>().sortingOrder = facingRight ? 1 : -1;
            animator.SetBool("FacingRight",facingRight);
        }
    }
    public void UpdateAnimator(){
        if (animator && animator.runtimeAnimatorController) {
            animator.SetFloat("ActionSpeedFactor", Mathf.Max(1 / fireRate,5));
        }
    }
    public void UpdateAnimFacingRight(){
        switch(state){
            case GunState.Empty:
                AnimatorStateInfo si = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play("",0,si.normalizedTime);
                break;
        }
    }
}