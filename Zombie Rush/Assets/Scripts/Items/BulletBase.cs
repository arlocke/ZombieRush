using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType {
    cal9mm,
    cal762,
    cal50ae,
}

public class BulletBase : MonoBehaviour
{
    public float damage;
    public float speed;
    public float distanceRemaining;

    public AmmoType ammoType;

    private void FixedUpdate() {
        transform.position += transform.right * speed; //right is the x axis, negative value for left
        distanceRemaining -= speed;

        if(distanceRemaining <= 0) {
            Destroy(gameObject);
        }
    }
}
