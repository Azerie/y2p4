using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : InteractableBehaviourAbstract
{
    [Tooltip("Don't set required item for doors that dont need an item")]
    [SerializeField] private Item requiredItem;
    [Tooltip("Angle of open door")]
    [SerializeField] private int angle;
    [Tooltip("How fast the door opens")]
    [SerializeField] private float openingSpeed = 1f;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;


    void Start()
    {
        targetRotation = transform.parent.rotation;
        closedRotation = transform.parent.rotation;
        openRotation = Quaternion.Euler(transform.parent.rotation.eulerAngles + new Vector3(0, angle, 0));
    }

    void Update()
    {
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, targetRotation, openingSpeed * Time.deltaTime);
    }

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
        targetRotation = openRotation;
        isOpen = true;
    }

    private void Close() 
    {
        targetRotation = closedRotation;
        isOpen = false;
    }

    public bool IsOpen() { return isOpen; }
}
