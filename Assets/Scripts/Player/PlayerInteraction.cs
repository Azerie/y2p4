using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Canvas promptReminder;

    private GameObject interactableObject;

    public void Interact()
    {
        if(interactableObject != null) 
        {
            interactableObject.GetComponent<InteractableBehaviourAbstract>().OnInteract();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Interactable")
        {
            if(promptReminder != null){
                promptReminder.enabled = true;
            }
            interactableObject = collision.gameObject;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.tag == "Interactable")
        {
            if(promptReminder != null){
                promptReminder.enabled = false;
            }
            interactableObject = null;
        }
    }
}
