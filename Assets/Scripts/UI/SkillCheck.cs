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
    private bool wasLastCheckPassed = false;

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
            Debug.DrawRay(circle.position, Quaternion.Euler(0, 0, angle) * circle.transform.up.normalized * 200);
            Debug.DrawRay(circle.position, Quaternion.Euler(0, 0, -angle) * circle.transform.up.normalized * 200);
        }
    }

    public bool StartMinigame()
    {
        if (!isActive) 
        {
            canvas.enabled = true;
            isActive = true;
            speed *= Math.Sign(UnityEngine.Random.Range(-1f, 1f));
            pointer.Rotate(0, 0, UnityEngine.Random.Range(0, 360));
            circle.Rotate(0, 0, UnityEngine.Random.Range(0, 360));
            return true;
        }
        return false;
    }

    public bool EndMinigame()
    {
        if(isActive)
        {
            canvas.enabled = false;
            isActive = false;
            float r1 = circle.rotation.eulerAngles.z;
            float r2 = pointer.rotation.eulerAngles.z;
            wasLastCheckPassed = Math.Abs(r1 - r2) % 360 < angle;
            return true;
        }
        return false;
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

    public bool IsActive() { return canvas.enabled; }

    public bool WasLastCheckPassed() { return wasLastCheckPassed; }
}
