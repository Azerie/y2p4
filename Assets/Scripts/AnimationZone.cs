using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AnimationZone : MonoBehaviour
{
    [SerializeField] private Animator _aniamtor;
    [SerializeField] private string animationName;
    [SerializeField] private EventReference _audio;

    private bool isFirst = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player") && !other.isTrigger)
        {
            if(isFirst) {
                _aniamtor.Play(animationName);
                if(!_audio.IsNull)
                {
                    RuntimeManager.PlayOneShot(_audio);
                }
                isFirst = false;
            }
        }
    }
}
