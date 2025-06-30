using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class VoicelinePlayer : MonoBehaviour
{
    public void PlayVoiceline(Voiceline line)
    {
        if (!line.Audio.IsNull)
        {
            if (gameObject.CompareTag("Player"))
            {
                RuntimeManager.PlayOneShot(line.Audio);
            }
            else
            {
                RuntimeManager.PlayOneShotAttached(line.Audio, gameObject);
            }
        }
        SubtitlesManager.SetSubtitles(line);
    }
}
