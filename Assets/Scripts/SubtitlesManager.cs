using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SubtitlesManager : MonoBehaviour
{
    private Voiceline currentVoiceline;
    public static SubtitlesManager Instance;
    public static UnityAction OnSubtitleChanged;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public static void SetSubtitles(Voiceline line)
    {
        Instance.currentVoiceline = line;
        OnSubtitleChanged();
    }

    public static Voiceline GetCurrentSubtitles()
    {
        return Instance.currentVoiceline;
    }
    
    public static SubtitlesManager GetInstance()
    {
        return Instance;
    }
}
