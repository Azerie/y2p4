using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HiddePanelBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] GameObject itemInside;
    void Start()
    {
        itemInside.SetActive(false);
    }

    public override void OnInteract() {
        itemInside.SetActive(true);
        Destroy(gameObject);
    }
}
