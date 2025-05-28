using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoorOpener : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            DoorBehaviour doorBehaviour = collision.gameObject.GetComponent<DoorBehaviour>();
            if (doorBehaviour != null && doorBehaviour.GetRequiredItem() == null)
            {
                doorBehaviour.Open();
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if(collision.gameObject.CompareTag("Interactable"))
        {
            DoorBehaviour doorBehaviour = collision.gameObject.GetComponent<DoorBehaviour>();
            if (doorBehaviour != null)
            {
                doorBehaviour.Close();
            }
        }
    }
}
