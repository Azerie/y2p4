using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubtitlesUI : MonoBehaviour
{
    [SerializeField] private const float defaultDisappearingTime = 3f;
    [SerializeField] private TMP_Text subtitlesField;
    [SerializeField] private GameObject subtitlesBg;
    private float endTime;
    private bool isActive = false;
    void Start()
    {
        SetSubtitles("");
        SubtitlesManager.OnSubtitleChanged += UpdateSubtitles;
    }
    void Update()
    {
        if (isActive && Time.time > endTime)
        {
            subtitlesField.enabled = false;
            isActive = false;
            if (subtitlesBg != null)
            {
                subtitlesBg.SetActive(false);
            }
        }
    }
    private void SetSubtitles(string text, float disappearingTime = defaultDisappearingTime)
    {
        endTime = Time.time + disappearingTime;
        isActive = true;
        subtitlesField.text = text;
        subtitlesField.enabled = true;
        if (subtitlesBg != null)
        {
            subtitlesBg.SetActive(true);
        }
    }
    private void UpdateSubtitles()
    {
        SetSubtitles(SubtitlesManager.GetCurrentSubtitles().SubtitlesText);
    }
}
