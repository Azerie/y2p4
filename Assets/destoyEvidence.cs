// Filename: EvidencePickup.cs

using UnityEngine;
using FMODUnity; // Required for FMOD's EventReference and RuntimeManager

/// <summary>
/// Handles the collection of an evidence item.
/// When interacted with, it plays a specified FMOD sound event
/// and then destroys the GameObject.
/// </summary>
public class EvidencePickup : MonoBehaviour
{
    [Header("FMOD Sound Settings")]
    [Tooltip("The sound to play when this evidence is collected.")]
    // This 'EventReference' is the key. It automatically creates a searchable
    // dropdown menu in the Unity Inspector for all of your FMOD events.
    public EventReference collectionSound;

    /// <summary>
    /// This function should be called by your player's interaction system
    /// when they collect the evidence (e.g., by pressing 'E').
    /// </summary>
    public void OnInteract()
    {
        // Check if a sound has been assigned in the Inspector.
        // The !.IsNull check is the proper way to see if an EventReference is empty.
        if (!collectionSound.IsNull)
        {
            // Play the selected FMOD event as a simple one-shot sound
            // at the position of this GameObject.
            RuntimeManager.PlayOneShot(collectionSound, this.transform.position);
        }
        else
        {
            // Log a warning if no sound was assigned, which helps with debugging.
            Debug.LogWarning("Collection Sound not set in the Inspector for " + gameObject.name);
        }

        // --- Your other game logic can go here ---
        // For example, telling a manager to increase the evidence count.
        // EvidenceManager.Instance.AddEvidence(1);

        Debug.Log("Evidence collected: " + gameObject.name);

        // Finally, destroy the evidence GameObject.
        Destroy(gameObject);
    }
}