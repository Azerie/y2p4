using UnityEngine;
using FMOD.Studio; // Required for EventInstance
using FMODUnity; // Required for EventReference and RuntimeManager

public class CompoundZonePlayer : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("The tag assigned to your player GameObject.")]
    public string playerTag = "Player";

    [Header("FMOD Event Settings")]
    [Tooltip("The continuous FMOD event to play while the player is inside this zone.")]
    // FIX 1: This uses the modern EventReference struct instead of the obsolete [EventRef] attribute.
    // This will still give you the searchable event browser in the Inspector.
    public EventReference zoneEvent;

    private int overlappingCollidersCount = 0;
    private bool isPlayerConsideredInZone = false;

    // This holds the sound while it's playing
    private EventInstance zoneEventInstance;

    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag(playerTag))
        {
            overlappingCollidersCount++;

            // If the player is entering the VERY FIRST part of the zone...
            if (!isPlayerConsideredInZone && overlappingCollidersCount > 0)
            {
                isPlayerConsideredInZone = true;

                // Check if an event has been selected in the Inspector before trying to play it.
                if (!zoneEvent.IsNull)
                {
                    Debug.Log("Player entered zone. Starting continuous sound.");

                    // Create an instance of the FMOD event
                    zoneEventInstance = RuntimeManager.CreateInstance(zoneEvent);

                    // FIX 2: This is the modern way to attach an instance, passing the entire GameObject.
                    RuntimeManager.AttachInstanceToGameObject(zoneEventInstance, this.gameObject);

                    // Start the sound
                    zoneEventInstance.start();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player
        if (other.CompareTag(playerTag))
        {
            if (overlappingCollidersCount > 0)
            {
                overlappingCollidersCount--;
            }

            // If the player is exiting the VERY LAST part of the zone...
            if (isPlayerConsideredInZone && overlappingCollidersCount == 0)
            {
                isPlayerConsideredInZone = false;

                // Check if the sound instance is valid before trying to stop it
                if (zoneEventInstance.isValid())
                {
                    Debug.Log("Player exited zone. Stopping continuous sound.");

                    // Stop the sound with a gentle fade out (ALLOWFADEOUT)
                    zoneEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                    // Release the memory used by the sound instance
                    zoneEventInstance.release();
                }
            }
        }
    }

    // This is important! Make sure the sound is stopped if the zone object is ever disabled or destroyed.
    void OnDisable()
    {
        if (isPlayerConsideredInZone)
        {
            // Check if the instance is valid before trying to stop it
            if (zoneEventInstance.isValid())
            {
                Debug.Log("Zone disabled. Stopping continuous sound.");
                zoneEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // Stop immediately, no fade
                zoneEventInstance.release();
            }
            isPlayerConsideredInZone = false;
            overlappingCollidersCount = 0;
        }
    }
}
