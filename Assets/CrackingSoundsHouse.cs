using UnityEngine;
using FMODUnity; // Required for FMOD features

/// <summary>
/// This script triggers an FMOD event at a regular, adjustable interval.
/// The FMOD event itself is responsible for deciding if it will play,
/// based on its internal probability settings.
/// </summary>
public class FMODIntervalTrigger : MonoBehaviour
{
    [Header("FMOD Event Settings")]
    [SerializeField]
    [Tooltip("The FMOD event that will be triggered at each interval.")]
    private EventReference soundEvent;

    [Header("Timing Settings")]
    [SerializeField]
    [Tooltip("The time in seconds between each attempt to play the sound.")]
    private float playInterval = 10f;

    // Internal timer to count down the interval.
    private float timer;

    void Start()
    {
        // Initialize the timer.
        // The first sound will be triggered after the first full interval has passed.
        timer = playInterval;
    }

    void Update()
    {
        // Subtract the time since the last frame from our timer.
        timer -= Time.deltaTime;

        // When the timer reaches zero or less...
        if (timer <= 0f)
        {
            // ...it's time to trigger the sound check.
            TriggerFMODEventCheck();

            // Reset the timer back to the full interval.
            timer = playInterval;
        }
    }

    /// <summary>
    /// Creates an instance of the FMOD event and tells it to start.
    /// FMOD will then use its internal logic (like probability) to determine
    /// if the sound is actually heard.
    /// </summary>
    private void TriggerFMODEventCheck()
    {
        // First, a safety check to make sure an event has been assigned in the Inspector.
        if (soundEvent.IsNull)
        {
            Debug.LogError("FMOD Event is not assigned in the Inspector on this object: " + this.gameObject.name);
            return; // Stop the function here if no sound is assigned.
        }

        // Log a message to the console so you know the C# script is working correctly.
        Debug.Log("Interval reached. Asking FMOD to play event. FMOD's probability will now take over.");

        // Create an instance of the FMOD event.
        FMOD.Studio.EventInstance eventInstance = RuntimeManager.CreateInstance(soundEvent);

        // If it's a 3D sound, attach it to the position of this GameObject.
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));

        // Tell the instance to start.
        eventInstance.start();

        // Release the instance immediately. This is crucial for "fire and forget" sounds
        // as it frees up memory. FMOD will continue playing the sound until it finishes.
        eventInstance.release();
    }
}