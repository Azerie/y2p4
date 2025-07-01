using UnityEngine;
using FMOD.Studio; // Needed for EventInstance and the correct STOP_MODE
using FMODUnity; // Needed for EventReference and the RuntimeManager

/// <summary>
/// Plays a continuous FMOD event while a player is inside a compound trigger zone.
/// This version is designed for hierarchical characters, where the collider is on a child
/// object (e.g., "Body") and the identifying tag is on the parent object.
/// </summary>
public class ZoneAmbiencePlayer : MonoBehaviour
{
    // ------------------------------------------------------------------
    // SETTINGS - Configure these in the Unity Inspector
    // ------------------------------------------------------------------

    [Header("FMOD Event to Play")]
    [Tooltip("Select the continuous, loopable FMOD sound that will play inside this zone.")]
    public EventReference zoneAmbienceEvent;

    [Header("Player Detection")]
    [Tooltip("The tag of the PARENT player GameObject this zone should react to.")]
    public string playerParentTag = "Player";


    // ------------------------------------------------------------------
    // INTERNAL STATE - Do not modify these from other scripts
    // ------------------------------------------------------------------
    private int _triggerCount = 0;
    private EventInstance _ambienceInstance;


    // This function is called whenever any object's collider ENTERS one of the child trigger volumes.
    private void OnTriggerEnter(Collider other)
    {
        // --- KEY CHANGE IS HERE ---
        // Instead of checking the tag of 'other' directly, we check the tag of its PARENT.
        // This works for characters where a child object like "Body" has the collider.
        if (other.transform.parent != null && other.transform.parent.CompareTag(playerParentTag))
        {
            // The object that entered belongs to our player, so increase the count.
            _triggerCount++;

            // If this is the FIRST volume the player has entered (count is now 1), start the sound.
            if (_triggerCount == 1)
            {
                Debug.Log("Player has entered the zone. Starting ambience.");

                // Start the sound instance.
                _ambienceInstance = RuntimeManager.CreateInstance(zoneAmbienceEvent);
                RuntimeManager.AttachInstanceToGameObject(_ambienceInstance, gameObject);
                _ambienceInstance.start();
            }
        }
    }

    // This function is called whenever any object's collider EXITS one of the child trigger volumes.
    private void OnTriggerExit(Collider other)
    {
        // --- KEY CHANGE IS HERE ---
        // We also check the parent's tag upon exiting.
        if (other.transform.parent != null && other.transform.parent.CompareTag(playerParentTag))
        {
            // The player is leaving one of our volumes, so decrease the count.
            if (_triggerCount > 0)
            {
                _triggerCount--;
            }

            // If the count has dropped to ZERO, the player is completely outside the zone. Stop the sound.
            if (_triggerCount == 0)
            {
                Debug.Log("Player has exited the zone. Stopping ambience.");

                // Stop the sound with a fade-out and release it from memory.
                _ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _ambienceInstance.release();
            }
        }
    }

    // A safety function to stop the sound if this zone object is ever disabled.
    private void OnDisable()
    {
        if (_ambienceInstance.isValid())
        {
            Debug.LogWarning("Zone object was disabled. Forcing sound to stop immediately.");
            _ambienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _ambienceInstance.release();
        }
    }
}
