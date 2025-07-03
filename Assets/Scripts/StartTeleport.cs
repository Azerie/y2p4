using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTeleport : MonoBehaviour
{
    [SerializeField] Transform teleportPosition;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player"))
        {
            if (teleportPosition == null || PlayerInventory.GetInstance().GetItems().Count == 0)
            {
                other.transform.parent.position = teleportPosition.position;
            }
        }
    }
}
