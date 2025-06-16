using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCheck : MonoBehaviour
{
    [SerializeField] private float angle;
    [SerializeField] private float speed;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform circle;
    private Canvas canvas;
    private bool isActive = false;
    private float circleRotation;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }

    void Update()
    {
        if (isActive)
        {
            pointer.Rotate(0, 0, speed * Time.deltaTime);
            Debug.DrawRay(pointer.position, Quaternion.Euler(0, 0, angle) * pointer.transform.up.normalized);
            Debug.DrawRay(pointer.position, Quaternion.Euler(0, 0, -angle) * pointer.transform.up.normalized);
        }
    }

    public void StartMinigame()
    {
        canvas.enabled = true;
        isActive = true;
        speed *= Math.Sign(UnityEngine.Random.Range(-1f, 1f));
        pointer.Rotate(0, 0, UnityEngine.Random.Range(0, 360));
        circle.Rotate(0, 0, UnityEngine.Random.Range(0, 360));
    }

    public bool EndMinigame()
    {
        canvas.enabled = false;
        isActive = false;
        float r1 = circle.rotation.eulerAngles.z;
        float r2 = pointer.rotation.eulerAngles.z;
        return Math.Abs(r1 - r2) % 360 < angle;
    }

    public void DebugFunc()
    {
        if (!isActive)
        {
            StartMinigame();
        }
        else
        {
            Debug.Log(EndMinigame());
        }
    }
}
