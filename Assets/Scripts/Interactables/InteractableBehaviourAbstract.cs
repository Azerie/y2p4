using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBehaviourAbstract : MonoBehaviour
{
    protected bool isMarkedForDestruction = false;
    public abstract void OnInteract();
    public bool IsMarkedForDestruction() {
        return isMarkedForDestruction;
    }
}
