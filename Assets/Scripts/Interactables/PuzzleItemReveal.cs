using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleItemReveal : MonoBehaviour
{
    [SerializeField] GameObject itemInside;
    [SerializeField] private EventReference puzzleFinishSound;
    private void Start()
    {
        itemInside.SetActive(false);
    }
    public void EndPuzzle()
    {
        if(!puzzleFinishSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(puzzleFinishSound, gameObject);
        }
        itemInside.SetActive(true);
        gameObject.SetActive(false);
    }
}
