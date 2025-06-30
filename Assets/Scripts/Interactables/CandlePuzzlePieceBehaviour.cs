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
        if (!isLit) {
            base.OnInteract();
            CandleLit?.Invoke();
            isLit = true;
            if (visualToDisable != null)
            {
                visualToDisable.SetActive(false);
            }
            if (visualToEnable != null)
            {
                visualToEnable.SetActive(true);
            }
        }
    }
}
