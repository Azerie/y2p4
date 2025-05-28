using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVoiceline", menuName = "Voiceline")]
public class Voiceline : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private AudioClip audio;
    [SerializeField] private string subtitlesText;
    [SerializeField] private float subtitlesTime;

    // Properties for access
    public AudioClip Audio => audio;
    public string SubtitlesText => subtitlesText;
    public float SubtitlesTime => subtitlesTime;
}
