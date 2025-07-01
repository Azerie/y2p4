using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoicelineZone : MonoBehaviour
{
    [SerializeField] private Collectable requiredCollectable;
    [SerializeField] private Voiceline voicelineToPlay;
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player"))
        {
            if (requiredCollectable == null || EvidenceManager.GetInstance().GetJournal().Contains(requiredCollectable))
            {
                other.transform.parent.GetComponent<VoicelinePlayer>().PlayVoiceline(voicelineToPlay);
            }
        }
    }
}
