using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class VoicelinePlayer : MonoBehaviour
{
    public void PlayVoiceline(Voiceline line)
    {
        if (gameObject.CompareTag("Player"))
        {
            RuntimeManager.PlayOneShot(line.Audio);
        }
        else
        {
            RuntimeManager.PlayOneShot(line.Audio, transform.position);
        }
        SubtitlesManager.SetSubtitles(line);
    }
}
