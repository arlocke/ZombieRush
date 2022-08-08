using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool canInteract;

    private void OnTriggerEnter(Collider other) {

        Debug.Log("interactable entered");

        if (other.transform.parent.gameObject.CompareTag("Player")) {
            other.transform.parent.gameObject.GetComponent<PlayerController>().currentInteractables.Add(this);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.transform.parent.gameObject.CompareTag("Player")) {
            other.transform.parent.gameObject.GetComponent<PlayerController>().currentInteractables.Remove(this);
        }
    }
}
