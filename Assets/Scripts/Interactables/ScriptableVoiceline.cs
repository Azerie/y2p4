using System;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "NewVoiceline", menuName = "Voiceline")]
public class Voiceline : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private EventReference audio;
    [SerializeField] private string subtitlesText;
    [SerializeField] private float subtitlesTime = 3f;

    // Properties for access
    public EventReference Audio => audio;
    public string SubtitlesText => subtitlesText;
    public float SubtitlesTime => subtitlesTime;
}
