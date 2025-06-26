using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablePickupBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] private Collectable collectableInfo;

    public override void OnInteract()
    {
        base.OnInteract();
        EvidenceManager.GetInstance().AddEntry(collectableInfo);
        isMarkedForDestruction = true;
        Destroy(gameObject);
    }
}
