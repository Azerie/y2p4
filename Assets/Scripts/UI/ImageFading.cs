using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFading : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    private Image image;
    [SerializeField] private bool isVisible = false;
    void Start()
    {
        image = GetComponent<Image>();
    }
    
    void Update()
    {
        if (isVisible)
        {
            image.CrossFadeAlpha(1, fadeDuration, false);
        }
        else
        {
            image.CrossFadeAlpha(0, fadeDuration, false);
        }
    }
    public void FadeIn()
    {
        isVisible = true;
    }

    public void FadeOut()
    {
        isVisible = false;
    }
}
