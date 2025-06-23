using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensSlider : MonoBehaviour
{
    private PlayerControls controls;
    private Slider slider;
    public void Awake()
    {
        controls = FindObjectOfType<PlayerControls>();
        slider = GetComponent<Slider>();
        if (controls != null)
        {
            slider.value = controls.GetCameraSensitivity();
        }
    }
    public void SensChange()
    {
        float value = slider.value;
        // Debug.Log(value);
        if (controls != null)
        {
            controls.SetCameraSensitivity(value);
        }
    }
}
