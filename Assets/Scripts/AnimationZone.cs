using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationZone : MonoBehaviour
{
    [SerializeField] private Animator _aniamtor;
    [SerializeField] private string animationName;
    [SerializeField] private Voiceline voicelineToPlay;
    private bool isFirst = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player") && !other.isTrigger)
        {
            if(isFirst) {
                _aniamtor.Play(animationName);
                if(voicelineToPlay != null)
                {
                    other.transform.parent.GetComponent<VoicelinePlayer>().PlayVoiceline(voicelineToPlay);
                }
                isFirst = false;
            }
        }
    }
}
