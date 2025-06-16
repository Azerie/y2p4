using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorBehaviour : InteractableBehaviourAbstract
{
    [Tooltip("Don't set required item for doors that dont need an item")]
    [SerializeField] private Item requiredItem;
    [Tooltip("Angle of open door")]
    [SerializeField] private int angle;
    [Tooltip("How fast the door opens")]
    [SerializeField] private float openingSpeed = 1f;
    private bool isOpen = false;
    protected Quaternion closedRotation;
    protected Quaternion openRotation;
    protected Quaternion targetRotation;

    public Collider doorCollider;
    public Collider secondDoorCollider;
    public float disableLenght = 1;

    void Start()
    {
        targetRotation = transform.parent.rotation;
        closedRotation = transform.parent.rotation;
        openRotation = Quaternion.Euler(transform.parent.rotation.eulerAngles + new Vector3(0, angle, 0));
        GetComponent<NavMeshObstacle>().carving = true;
        if (requiredItem == null)
        {
            GetComponent<NavMeshObstacle>().enabled = false;
        }
        else
        {
            GetComponent<NavMeshObstacle>().enabled = true;
        }
    }

    void Update()
    {
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, targetRotation, openingSpeed * Time.deltaTime);
    }

    public override void OnInteract()
    {
        // Debug.Log("interacted");
        if(requiredItem != null) {
            if (PlayerInventory.GetInstance().HasItem(requiredItem))
            {
                Use();
                PlayerInventory.GetInstance().RemoveItem(requiredItem);
                requiredItem = null;
                GetComponent<NavMeshObstacle>().enabled = false;
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

    public void Open()
    {
        targetRotation = openRotation;
        isOpen = true;

        doorCollider.enabled = false;
        secondDoorCollider.enabled = false;

        StartCoroutine(ReenableColliderAfterDelay(disableLenght));

        // this part is for editor buttons to work properly
        GetComponent<NavMeshObstacle>().enabled = false;
        requiredItem = null;
    }

    public void Close()
    {
        targetRotation = closedRotation;
        isOpen = false;
    }

    public void CloseAndLock(Item pRequiredItem)
    {
        Close();
        requiredItem = pRequiredItem;
        GetComponent<NavMeshObstacle>().enabled = true;
    }

    public Item GetRequiredItem()
    {
        return requiredItem;
    }

    public void SetRequiredItem(Item newItem)
    {
        if (newItem != null)
        {
            GetComponent<NavMeshObstacle>().enabled = true;
        }
        else
        {
            GetComponent<NavMeshObstacle>().enabled = false;
        }
        requiredItem = newItem;
    }

    public bool IsOpen() { return isOpen; }

    private IEnumerator ReenableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(doorCollider != null)
        {
            doorCollider.enabled = true;
        }
        if(secondDoorCollider != null)
        {
            secondDoorCollider.enabled = true;
        }
    }
}
