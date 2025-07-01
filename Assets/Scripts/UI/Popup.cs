using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] private float popupDuration = 5f;
    private float timer;
    void OnEnable()
    {
        StartCoroutine(disableAfterTime(popupDuration));
    }

    private IEnumerator disableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
