using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public float damage;
    public float speed;
    public float distanceRemaining;

    private void FixedUpdate() {
        transform.position += transform.right * speed; //right is the x asis, negative value for left
        distanceRemaining -= speed;

        if(distanceRemaining <= 0) {
            Destroy(gameObject);
        }
    }
}
