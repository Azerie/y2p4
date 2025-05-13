using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandlePuzzleEndBehaviour : MonoBehaviour
{
    [SerializeField] GameObject itemInside;
    [SerializeField] int requiredCandlesNumber;
    int currentCandlesNumber = 0;

    void Start()
    {
        CandlePuzzlePieceBehaviour.CandleLit += AddCandle;
        itemInside.SetActive(false);
    }

    void AddCandle() {
        currentCandlesNumber += 1;
        if(currentCandlesNumber >= requiredCandlesNumber) {
            EndPuzzle();
        }
    }

    void EndPuzzle() {
        itemInside.SetActive(true);
        Destroy(gameObject);
    }
}
