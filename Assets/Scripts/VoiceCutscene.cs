using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class VoiceCutscene : MonoBehaviour
{
    [EventRef]
    public string fmodEventPath;  // Drag your FMOD event here in the Inspector

    private EventInstance voiceEvent;

    void Start()
    {
        if (!string.IsNullOrEmpty(fmodEventPath))
        {
            voiceEvent = RuntimeManager.CreateInstance(fmodEventPath);
        }
    }

    public void PlayVoiceLine()
    {
        if (voiceEvent.isValid())
        {
            voiceEvent.start();
        }
        else
        {
            Debug.LogWarning("FMOD event not set or invalid.");
        }
    }
}
