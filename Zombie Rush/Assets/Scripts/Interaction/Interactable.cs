using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public bool canInteract;

    public virtual void Interact(PlayerController pc) {
        //Do nothing overriding children
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.transform.parent.gameObject.CompareTag("Player")) {
            other.transform.parent.gameObject.GetComponent<PlayerController>().currentInteractables.Add(this);
        }
    }
    void OnTriggerExit2D(Collider2D other) {
        if(other.transform.parent.gameObject.CompareTag("Player")) {
            other.transform.parent.gameObject.GetComponent<PlayerController>().currentInteractables.Remove(this);
            canInteract = false;
        }
    }
}
