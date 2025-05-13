using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            if(interactableObject.IsDestroyed()) {
                interactableObject = null;
                if(promptReminder != null){
                    promptReminder.enabled = false;
                }
            }
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
