using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantBop : MonoBehaviour
{
    [Space(10)]
    [Header("Head bop settings")]
    [SerializeField] private float speedMult = 20f;
    [SerializeField] private float maxHeightDiff = 0.05f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
    }
    void Update()
    {
        transform.localPosition = startPos + new Vector3(0, Mathf.Sin(Time.time * speedMult) * maxHeightDiff, 0);
    }
}
