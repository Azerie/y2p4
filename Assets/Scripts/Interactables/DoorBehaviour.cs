using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : InteractableBehaviourAbstract
{
    [Tooltip("Don't set required item for doors that dont need an item")]
    [SerializeField] private Item requiredItem;
    [SerializeField] private int angle;
    private bool isOpen = false;

    public override void OnInteract()
    {
        Debug.Log("interacted");
        if(requiredItem != null) {
            if(PlayerInventory.GetInstance().HasItem(requiredItem)) {
                Use();
                PlayerInventory.GetInstance().RemoveItem(requiredItem);
                requiredItem = null;
            }
        } 
        else {
            Use();
        }
    }

    private void Use()
    {
        if(isOpen) {
            Close();
        }
        else {
            Open();
        }
    }

    private void Open() 
    {
        transform.parent.rotation = Quaternion.Euler(transform.parent.rotation.eulerAngles + new Vector3(0, angle, 0));
        isOpen = true;
    }

    private void Close() 
    {
        transform.parent.rotation = Quaternion.Euler(transform.parent.rotation.eulerAngles - new Vector3(0, angle, 0));
        isOpen = false;
    }

    public bool IsOpen() { return isOpen; }
}
