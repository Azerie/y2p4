using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablePickupBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] private Collectable collectableInfo;

    public override void OnInteract()
    {
        EvidenceManager.GetInstance().AddLine(collectableInfo.JournalEntry);
        Destroy(gameObject);
    }
}
