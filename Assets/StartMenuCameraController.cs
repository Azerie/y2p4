using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartMenuCameraController : MonoBehaviour
{
    [SerializeField]private GameObject[] cameraList;
    public float Camerainterval = 2;

    private int currentNumb = -1;

    private Coroutine cycleCoroutine;

    public Image FadeImage;
    public float FadeDuration = 1f;
    // Start is called before the first frame update
    void Start()
    {
        if(cameraList == null || cameraList.Length == 0) 
        {
            Debug.Log("no cameras?");
            return;
        }
        FadeImage.gameObject.SetActive(true);
        FadeImage.color= new Color(0, 0, 0, 0);

        cycleCoroutine = StartCoroutine(cycleCameras());
    }

    void Update()
    {
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Right Arrow pressed (new Input System)");

            if (cycleCoroutine != null)
                StopCoroutine(cycleCoroutine);

            // Do one manual switch with fade
            StartCoroutine(ManualSwitchWithFade());
        }
    }

    IEnumerator cycleCameras()
    {
        while (true)
        {
            yield return new WaitForSeconds(Camerainterval);
            yield return StartCoroutine(SwitchToRandomCamera());
        }
    }
    IEnumerator ManualSwitchWithFade()
    {
        yield return StartCoroutine(SwitchToRandomCamera());

        // Resume auto-switching after manual
        cycleCoroutine = StartCoroutine(cycleCameras());
    }
    void FadeAndThat()
    {
        StartCoroutine(SwitchToRandomCamera());
    }

    IEnumerator SwitchToRandomCamera()
    {
        yield return StartCoroutine(Fade(0f, 1f));

        // Deactivate all cameras
        foreach (GameObject cam in cameraList)
            cam.SetActive(false);

        // Choose a new random camera
        currentNumb = UnityEngine.Random.Range(0, cameraList.Length);
        cameraList[currentNumb].SetActive(true);

        Debug.Log("Switched to camera: " + cameraList[currentNumb].name);

        // Fade from black
        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        float timer = 0f;
        Color color = FadeImage.color;

        while (timer < FadeDuration)
        {
            float t = timer / FadeDuration;
            color.a = Mathf.Lerp(from, to, t);
            FadeImage.color = color;
            timer += Time.deltaTime;
            yield return null;
        }

        color.a = to;
        FadeImage.color = color;
    }

}
