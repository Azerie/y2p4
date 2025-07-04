using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoicelineZone : MonoBehaviour
{
    [SerializeField] private Collectable requiredCollectable;
    [SerializeField] private Voiceline voicelineToPlay;
    private bool isFirst = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player") && !other.isTrigger)
        {
            if (requiredCollectable == null || EvidenceManager.GetInstance().GetJournal().Contains(requiredCollectable))
            {
                if(isFirst) {
                    other.transform.parent.GetComponent<VoicelinePlayer>().PlayVoiceline(voicelineToPlay);
                    isFirst = false;
                }
            }
        }
    }
}
