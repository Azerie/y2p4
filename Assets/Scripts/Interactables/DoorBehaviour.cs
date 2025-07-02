using FMODUnity;
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
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference lockedSound;


    private bool isOpen = false;
    protected Quaternion closedRotation;
    protected Quaternion openRotation;
    protected Quaternion targetRotation;

    private Collider doorCollider;
    [SerializeField] private float disableTime = 4f;

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
        doorCollider = GetComponent<Collider>();
    }

    void Update()
    {
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, targetRotation, openingSpeed * Time.deltaTime);
    }

    public override void OnInteract()
    {
        // Debug.Log("interacted");
        if (requiredItem != null)
        {
            if (PlayerInventory.GetInstance().GetSelectedItem() == requiredItem)
            {
                Use();
                PlayerInventory.GetInstance().RemoveItem(requiredItem);
                requiredItem = null;
                GetComponent<NavMeshObstacle>().enabled = false;
            }
            else
            {
                RuntimeManager.PlayOneShot(lockedSound, transform.position);
            }
        }
        else
        {
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
        Debug.Log("Door opened");
        if (!openSound.IsNull)
        {
            RuntimeManager.PlayOneShot(openSound, transform.position);
        }
        targetRotation = openRotation;
        isOpen = true;

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
            isMarkedForDestruction = true;
        }
        StartCoroutine(ReenableColliderAfterDelay(disableTime));

        // this part is for editor buttons to work properly
        GetComponent<NavMeshObstacle>().enabled = false;
        requiredItem = null;
    }

    public void Close()
    {
        Debug.Log("Door closed");

        if (!closeSound.IsNull)
        {
            RuntimeManager.PlayOneShot(closeSound, transform.position);
        }
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
            isMarkedForDestruction = false;
        }
    }
}
