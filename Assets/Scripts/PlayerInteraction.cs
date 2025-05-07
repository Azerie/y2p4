using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Canvas promptReminder;

    private GameObject pickup;

    public void Interact()
    {
        if(pickup != null) 
        {
            // pick up item here
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Pickup")
        {
            if(promptReminder != null){
                promptReminder.enabled = true;
            }
            pickup = collision.gameObject;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.tag == "Pickup")
        {
            if(promptReminder != null){
                promptReminder.enabled = false;
            }
            pickup = null;
        }
    }
}
