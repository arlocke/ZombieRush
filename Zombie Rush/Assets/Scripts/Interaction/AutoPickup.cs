using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPickup : Pickup
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.parent.gameObject.CompareTag("Player")) {
            Interact(other.transform.parent.GetComponent<PlayerController>());
        }
    }
    void OnTriggerExit2D(Collider2D other) {
       //Do Nothing
    }

    void OnTriggerStay2D(Collider2D other) {
        Debug.Log("inside my ass");
        if (other.transform.parent.gameObject.CompareTag("Player")) {
            Interact(other.transform.parent.GetComponent<PlayerController>());
        }
    }
}
