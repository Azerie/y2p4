using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleItemReveal : MonoBehaviour
{
    [SerializeField] GameObject itemInside;
    private void Start()
    {
        itemInside.SetActive(false);
    }
    public void EndPuzzle()
    {
        itemInside.SetActive(true);
        Destroy(gameObject);
    }
}
