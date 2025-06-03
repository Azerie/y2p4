using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensSlider : MonoBehaviour
{
    public void Awake()
    {
        PlayerControls controls = FindObjectOfType<PlayerControls>();
        if (controls != null)
        {
            GetComponent<Slider>().value = controls.GetCameraSensitivity();
        }
    }
    public void SensChange()
    {
        float value = GetComponent<Slider>().value;
        Debug.Log(value);
        PlayerControls controls = FindObjectOfType<PlayerControls>();
        if (controls != null)
        {
            controls.SetCameraSensitivity(value);
        }
    }
}
