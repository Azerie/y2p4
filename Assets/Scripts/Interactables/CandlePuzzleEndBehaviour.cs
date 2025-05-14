using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandlePuzzleEndBehaviour : MonoBehaviour
{
    [SerializeField] int requiredCandlesNumber;
    int currentCandlesNumber = 0;
    PuzzleItemReveal itemReveal;

    void Start()
    {
        CandlePuzzlePieceBehaviour.CandleLit += AddCandle;
        itemReveal = GetComponent<PuzzleItemReveal>();
    }

    void AddCandle() {
        currentCandlesNumber += 1;
        if(currentCandlesNumber >= requiredCandlesNumber) {
            itemReveal.EndPuzzle();
        }
    }
}
