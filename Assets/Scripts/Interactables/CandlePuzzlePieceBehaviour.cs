using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandlePuzzlePieceBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] GameObject visualToEnable;
    [SerializeField] GameObject visualToDisable;

    public static event UnityAction CandleLit;
    private bool isLit = false;

    public override void OnInteract()
    {
        if(!isLit) {
            CandleLit();
            isLit = true;
            visualToDisable.SetActive(false);
            visualToEnable.SetActive(true);
        }
    }
}
