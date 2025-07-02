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
        if (itemInside != null)
        {
            itemInside.SetActive(true);
        }
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
