using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Canvas promptReminder;

    [SerializeField] private GameObject interactableObject;

    public void Interact()
    {
        if(interactableObject != null) 
        {
            InteractableBehaviourAbstract interactableObjectBehaviour = interactableObject.GetComponent<InteractableBehaviourAbstract>();
            interactableObjectBehaviour.OnInteract();
            if(interactableObjectBehaviour.IsMarkedForDestruction()) {
                RemoveInteractable();
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.CompareTag("Interactable"))
        {
            if(collision.gameObject.GetComponent<InteractableBehaviourAbstract>() != null) {
                SetInteractable(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.CompareTag("Interactable") && interactableObject == collision.gameObject)
        {
            RemoveInteractable();
        }
    }

    private void SetInteractable(GameObject pObject) {
        if(promptReminder != null){
            promptReminder.enabled = true;
        }
        interactableObject = pObject;
    }

    private void RemoveInteractable() {
        if(promptReminder != null){
            promptReminder.enabled = false;
        }
        interactableObject = null;
    }
}
