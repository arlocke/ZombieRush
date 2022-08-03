using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBase : MonoBehaviour
{
    //public string name;
    public int tier;
    public float damage;
    public float fireRate;
    public float range;
    public float bulletSpeed;
    public int magMaxSize;
    public int currentMagSize;
    public GameObject bulletRef;
    public Transform bulletSpawnPoint;
    public Transform hand1Point;
    public Transform hand2Point;

    public void Shoot() {
        if (currentMagSize > 0) {

            Debug.Log("clicked fagg");
            BulletBase newBullet = Instantiate(bulletRef, bulletSpawnPoint.position, bulletSpawnPoint.rotation).GetComponent<BulletBase>();

            newBullet.speed = bulletSpeed;
            newBullet.distanceRemaining = range;
            newBullet.damage = damage;

            currentMagSize--;
        }
    }
}
