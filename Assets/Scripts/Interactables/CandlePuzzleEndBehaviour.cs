using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandlePuzzleEndBehaviour : MonoBehaviour
{
    [SerializeField] int requiredCandlesNumber;
    [SerializeField] List<GameObject> gameObjectsToDisable;
    [SerializeField] List<GameObject> gameObjectsToEnable;
    int currentCandlesNumber = 0;
    PuzzleItemReveal itemReveal;

    void Start()
    {
        CandlePuzzlePieceBehaviour.CandleLit += AddCandle;
        itemReveal = GetComponent<PuzzleItemReveal>();
        if (gameObjectsToDisable.Count != requiredCandlesNumber || gameObjectsToEnable.Count != requiredCandlesNumber)
        {
            Debug.LogWarning("wrong amount of gameobjects to disable/enable assigned");
        }
    }

    void AddCandle() {
        if (gameObjectsToDisable.Count == requiredCandlesNumber || gameObjectsToEnable.Count == requiredCandlesNumber)
        {
            gameObjectsToDisable[currentCandlesNumber].SetActive(false);
            gameObjectsToEnable[currentCandlesNumber].SetActive(true);
        }
        currentCandlesNumber += 1;
        if (currentCandlesNumber >= requiredCandlesNumber)
        {
            itemReveal.EndPuzzle();
        }
    }
}
