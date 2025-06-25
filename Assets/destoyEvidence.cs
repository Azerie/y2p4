using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destoyEvidence : InteractableBehaviourAbstract
{
    public override void OnInteract()
    {
        EvidenceManager.Instance.AddEvidence();
        isMarkedForDestruction = true;
        Destroy(gameObject);
    }
}