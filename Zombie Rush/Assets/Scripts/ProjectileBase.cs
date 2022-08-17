using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType {
    Cal9mm,
    Cal762,
    Cal50ae,
    //more types for zombie sludge etc
}

public class ProjectileBase : MonoBehaviour {
    public float damage;
    public float speed;
    public float distanceRemaining;

    public CreatureBase owner; //the creature the projectile originated from

    public AmmoType ammoType;

    private void FixedUpdate() {
        transform.position += transform.right * speed; //right is the x axis, negative value for left
        distanceRemaining -= speed;

        if(distanceRemaining <= 0) {
            Destroy(gameObject);
        }
    }

    public void RemoveDamage(float dmg) {
        damage -= dmg;
        if(damage <= 0) {
            GetComponent<BoxCollider2D>().enabled = false; //disables projectile box collider to avoid negative numbers
            Destroy(gameObject);
        }
    }

    public void AddDamage(float dmg) {

    }
}
