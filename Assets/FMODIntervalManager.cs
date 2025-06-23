// --- FMOD Interval Manager ---
//
// HOW TO USE:
// 1. Attach THIS script ("FMODIntervalManager") to a single GameObject in your scene.
// 2. In the Inspector, find the "Managed Sounds" list.
// 3. Click the '+' button to add a new sound entry.
// 4. Fill in the details (description, FMOD event, and interval) for each entry.
//
// IMPORTANT:
// This file contains a second helper class called "SoundIntervalEntry".
// DO NOT try to attach "SoundIntervalEntry" to any GameObject. It only exists to
// hold data for the main FMODIntervalManager.

using UnityEngine;
using FMODUnity;
using System.Collections.Generic; // Required to use a List<>

// ------------------------------------------------------------------------------------
// CLASS 1: THE DATA CONTAINER (Do NOT attach this to a GameObject)
// This is a "helper" class that simply holds the information for a single sound.
// The [System.Serializable] attribute is what allows it to appear in the Inspector.
// ------------------------------------------------------------------------------------
[System.Serializable]
public class SoundIntervalEntry
{
    [Tooltip("A name for you to easily identify this sound in the Inspector.")]
    public string description;

    [Tooltip("The FMOD Event you want to play for this entry.")]
    public EventReference soundEvent;

    [Tooltip("How many seconds to wait between attempts to play this specific sound.")]
    public float playInterval = 15f;

    // This is the internal timer for this entry. It's hidden in the inspector to keep things tidy.
    [HideInInspector]
    public float timer;
}


// ------------------------------------------------------------------------------------
// CLASS 2: THE MAIN COMPONENT (This is the one you attach to a GameObject)
// This is the main script that runs all the logic.
// ------------------------------------------------------------------------------------
public class FMODIntervalManager : MonoBehaviour
{
    [Header("Sound Library")]
    [Tooltip("Add all the sounds you want to manage here. Each one will run on its own timer.")]
    public List<SoundIntervalEntry> managedSounds;

    void Start()
    {
        // This loop runs once at the beginning to set up the initial timer for each sound.
        // This prevents every sound from trying to play at the exact same moment on the first cycle.
        foreach (SoundIntervalEntry entry in managedSounds)
        {
            entry.timer = entry.playInterval;
        }
    }

    void Update()
    {
        // If the list is empty, do nothing.
        if (managedSounds == null) return;

        // This loop runs every frame.
        // It checks the timer for every single sound in the list.
        foreach (SoundIntervalEntry entry in managedSounds)
        {
            // Count down the personal timer for this specific sound.
            entry.timer -= Time.deltaTime;

            // When this sound's timer reaches zero...
            if (entry.timer <= 0f)
            {
                // ...tell FMOD to try and play it...
                TriggerFMODEvent(entry);
                // ...and then reset THIS sound's timer back to its full interval.
                entry.timer = entry.playInterval;
            }
        }
    }

    // This method handles the actual FMOD event playback.
    void TriggerFMODEvent(SoundIntervalEntry entry)
    {
        // A safety check to make sure you've assigned an FMOD Event in the Inspector.
        if (entry.soundEvent.IsNull)
        {
            Debug.LogWarning($"You have a sound entry named '{entry.description}' but have not assigned an FMOD Event to it.", this.gameObject);
            return;
        }

        // Create a new instance of the sound from FMOD's memory.
        FMOD.Studio.EventInstance eventInstance = RuntimeManager.CreateInstance(entry.soundEvent);

        // Set the sound's position in 3D space to this GameObject's position.
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(this.gameObject));

        // Tell the sound to start. FMOD's internal probability will now decide if you hear it.
        eventInstance.start();

        // Release the instance immediately to free up memory. This is critical for performance.
        eventInstance.release();
    }
}