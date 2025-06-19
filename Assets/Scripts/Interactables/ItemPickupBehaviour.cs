using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] private Item itemToGive;

    public override void OnInteract()
    {
        base.OnInteract();

        PlayerInventory.GetInstance().AddItem(itemToGive);
        isMarkedForDestruction = true;
        Destroy(gameObject);
    }
}