using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzleBehaviour : MonoBehaviour // this is responsible for the clock hand
{
    [SerializeField] GameObject centrePiece;
    [SerializeField] Transform clockPositionsParent;
    [SerializeField] Transform answerPosition;
    private ClockPuzzleEndBehaviour clockPuzzleEndBehaviour;
    private bool isInPosition = false;
    private List<GameObject> clockPositions = new List<GameObject>();
    void Start()
    {
        clockPuzzleEndBehaviour = GetComponent<ClockPuzzleEndBehaviour>();
        foreach (Transform pos in clockPositionsParent)
        {
            if (pos.TryGetComponent<ClockPuzzlePieceBehaviour>(out ClockPuzzlePieceBehaviour a))
            {
                a.SetClockHand(this);
                clockPositions.Add(pos.gameObject);
            }
        }
    }

    public void SetClockInPosition(Transform target)
    {
        // Debug.Log(target.position);
        centrePiece.transform.LookAt(target.position);
        if (answerPosition == target)
        {
            isInPosition = true;
        }
        else
        {
            isInPosition = false;
        }
        clockPuzzleEndBehaviour.CheckCorrectPositions();
    }

    public bool IsInPosition()
    {
        return isInPosition;
    }
}
