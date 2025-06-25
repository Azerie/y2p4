using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBehaviourAbstract : MonoBehaviour
{
    [SerializeField] private EventReference pickupSound;

    protected bool isMarkedForDestruction = false;
    public virtual void OnInteract()
    {
        if (!pickupSound.IsNull)
        {
            RuntimeManager.PlayOneShot(pickupSound, transform.position);
        }
    }
    public bool IsMarkedForDestruction() {
        return isMarkedForDestruction;
    }
}
