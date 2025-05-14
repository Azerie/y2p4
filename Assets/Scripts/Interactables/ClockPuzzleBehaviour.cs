using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzleBehaviour : MonoBehaviour
{
    [SerializeField] GameObject centrePiece;
    [SerializeField] List<GameObject> clockPositions;
    [SerializeField] Transform answerPosition;
    PuzzleItemReveal itemReveal;
    private void Start()
    {
        itemReveal = GetComponent<PuzzleItemReveal>();
    }

    public void SetClockInPosition(Transform target)
    {
        Debug.Log(target.position);
        centrePiece.transform.LookAt(target.position);
        if (answerPosition == target) 
        {
            itemReveal.EndPuzzle();
        }
    }
}
