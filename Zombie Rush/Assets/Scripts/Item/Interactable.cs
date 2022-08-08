using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public bool canInteract;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.parent.gameObject.CompareTag("Player")) {
            other.transform.parent.gameObject.GetComponent<PlayerController>().currentInteractables.Add(this);
        }
    }
    void OnTriggerExit2D(Collider2D other) {
        if (other.transform.parent.gameObject.CompareTag("Player")) {
            other.transform.parent.gameObject.GetComponent<PlayerController>().currentInteractables.Remove(this);
        }
    }
}
