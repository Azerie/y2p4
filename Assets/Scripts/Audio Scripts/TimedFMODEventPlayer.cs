using UnityEngine;
using FMODUnity; // Required for FMOD integration

/// <summary>
/// Plays a specified FMOD event at a regular interval in seconds.
/// Includes an option to enable debug logging.
/// </summary>
public class TimedFMODEventPlayer : MonoBehaviour
{
    [Header("FMOD Event Settings")]
    [Tooltip("The FMOD event to play.")]
    [SerializeField]
    private EventReference fmodEvent;

    [Header("Timing Settings")]
    [Tooltip("The interval in seconds at which to play the event.")]
    [SerializeField]
    private float playInterval = 5.0f;

    [Header("Debugging")]
    [Tooltip("Enable to show debug messages in the console.")]
    [SerializeField]
    private bool enableDebugLogging = false;

    // A private timer to keep track of the elapsed time.
    private float timer;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// We initialize the timer here.
    /// </summary>
    void Start()
    {
        // Initialize the timer to 0 at the start.
        timer = 0f;
    }

    /// <summary>
    /// Update is called once per frame.
    /// We use it to count time and check if we need to play the sound.
    /// </summary>
    void Update()
    {
        // Add the time elapsed since the last frame to our timer.
        timer += Time.deltaTime;

        // Check if the timer has exceeded the desired interval.
        if (timer >= playInterval)
        {
            // If it has, play the FMOD event.
            PlayEvent();

            // Reset the timer by subtracting the interval.
            // This is more accurate than setting it to 0, especially with varying frame rates.
            timer -= playInterval;
        }
    }

    /// <summary>
    /// Plays the specified FMOD event at the position of this GameObject.
    /// </summary>
    private void PlayEvent()
    {
        // Check if the event path has been set in the inspector to avoid errors.
        if (!fmodEvent.IsNull)
        {
            // Play the FMOD event once at the position of this GameObject.
            RuntimeManager.PlayOneShot(fmodEvent, transform.position);

            // If debugging is enabled, log a message to the console.
            if (enableDebugLogging)
            {
                Debug.Log($"Played FMOD event '{fmodEvent.ToString()}' at {Time.time} seconds on {gameObject.name}.");
            }
        }
        else
        {
            // If debugging is enabled, log a warning if the event reference is not set.
            if (enableDebugLogging)
            {
                Debug.LogWarning("FMOD Event Reference is not set on " + gameObject.name);
            }
        }
    }
}
