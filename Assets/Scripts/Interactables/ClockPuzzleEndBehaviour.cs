using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzleEndBehaviour : MonoBehaviour
{
    private PuzzleItemReveal itemReveal;
    private ClockPuzzleBehaviour[] clockPuzzleBehaviours;

    private void Start()
    {
        itemReveal = GetComponent<PuzzleItemReveal>();
        clockPuzzleBehaviours = GetComponents<ClockPuzzleBehaviour>();
    }

    public void CheckCorrectPositions()
    {
        bool isCorrect = true;
        foreach (ClockPuzzleBehaviour clockHand in clockPuzzleBehaviours)
        {
            isCorrect = isCorrect && clockHand.IsInPosition();
        }
        if (isCorrect)
        {
            itemReveal.EndPuzzle();
        }
    }
}
