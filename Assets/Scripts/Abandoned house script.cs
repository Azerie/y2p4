using UnityEngine;
using FMODUnity; // Required for FMOD integration
using System.Collections; // Required for IEnumerator

[RequireComponent(typeof(Collider))] // Ensure there's a Collider for trigger functionality
public class PlayFMODInTriggerZone : MonoBehaviour
{
    [Header("FMOD Event Settings")]
    [Tooltip("The FMOD event to play (e.g., your multi-instrument).")]
    public EventReference fmodEvent;

    [Tooltip("The interval in seconds between playing the event while the player is inside the trigger.")]
    public float playInterval = 30f;

    [Header("Trigger Settings")]
    [Tooltip("The tag of the Player GameObject. Common tag is 'Player'.")]
    public string playerTag = "Player";

    private Coroutine playLoopCoroutine;
    private bool isPlayerInside = false; // Tracks if the correctly tagged object is inside

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning($"Collider on {gameObject.name} was not set to 'Is Trigger'. Forcing it now. Please set it in the editor.", this);
                col.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError($"PlayFMODInTriggerZone requires a Collider component on the GameObject '{gameObject.name}'.", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"OnTriggerEnter detected by {other.name} with tag {other.tag} on {gameObject.name}"); // General trigger detection

        if (IsColliderThePlayer(other))
        {
            // Debug.Log($"{other.name} identified as player entered trigger zone on {gameObject.name}.");
            isPlayerInside = true;
            if (playLoopCoroutine == null)
            {
                playLoopCoroutine = StartCoroutine(PlayEventLoop());
                Debug.Log($"Player '{other.name}' entered trigger zone. Starting FMOD event loop for '{fmodEvent.Path}' every {playInterval}s.", this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Debug.Log($"OnTriggerExit detected by {other.name} with tag {other.tag} on {gameObject.name}"); // General trigger detection

        if (IsColliderThePlayer(other))
        {
            // Debug.Log($"{other.name} identified as player exited trigger zone on {gameObject.name}.");
            isPlayerInside = false; // Signal the coroutine to stop
            if (playLoopCoroutine != null)
            {
                StopCoroutine(playLoopCoroutine);
                playLoopCoroutine = null;
                Debug.Log($"Player '{other.name}' exited trigger zone. Stopped FMOD event loop for '{fmodEvent.Path}'.", this);
            }
        }
    }

    private bool IsColliderThePlayer(Collider other)
    {
        if (string.IsNullOrEmpty(playerTag))
        {
            // If playerTag is not set, consider any collider as "player" - useful for testing but be specific for gameplay
            // Debug.LogWarning("Player Tag is not set in PlayFMODInTriggerZone. Any collider will activate the sound.", this);
            return true;
        }
        return other.CompareTag(playerTag);
    }

    IEnumerator PlayEventLoop()
    {
        Debug.Log($"PlayEventLoop started for '{fmodEvent.Path}'. Waiting for initial interval of {playInterval}s.", this);
        // This waits for the first interval THEN plays.
        // If you want it to play immediately on enter, then wait, uncomment PlayEvent() and move the yield.
        // PlayEvent(); // Play immediately
        // yield return new WaitForSeconds(playInterval); // Then wait

        yield return new WaitForSeconds(playInterval); // Wait for the first interval

        while (isPlayerInside) // Loop only while player is inside and coroutine is active
        {
            PlayEvent();
            Debug.Log($"PlayEventLoop: Waiting for next interval of {playInterval}s for '{fmodEvent.Path}'.", this);
            yield return new WaitForSeconds(playInterval);
        }
        Debug.Log($"PlayEventLoop for '{fmodEvent.Path}' naturally ended because player is no longer inside or coroutine was stopped.", this);
        playLoopCoroutine = null; // Clean up
    }

    void PlayEvent()
    {
        if (fmodEvent.IsNull)
        {
            Debug.LogWarning($"FMOD Event not assigned in PlayFMODInTriggerZone script on {gameObject.name}. Cannot play event.", this);
            return;
        }

        // Play the FMOD event. For a multi-instrument, this triggers it once.
        // Its internal complexity (what sounds it makes, how long it lasts) is handled by FMOD Studio.
        RuntimeManager.PlayOneShotAttached(fmodEvent, this.gameObject);
        Debug.Log($"Played FMOD event: '{fmodEvent.Path}' at {Time.time} attached to {gameObject.name}.", this);
    }

    void OnDisable()
    {
        // If the GameObject with this script is disabled, stop the loop.
        if (playLoopCoroutine != null)
        {
            StopCoroutine(playLoopCoroutine);
            playLoopCoroutine = null;
            isPlayerInside = false; // Reset state
            Debug.Log($"PlayFMODInTriggerZone on {gameObject.name} was disabled. Stopping FMOD event loop.", this);
        }
    }

    void OnDestroy()
    {
        // Just in case, ensure cleanup when destroyed.
        if (playLoopCoroutine != null)
        {
            StopCoroutine(playLoopCoroutine);
            playLoopCoroutine = null;
        }
    }
}