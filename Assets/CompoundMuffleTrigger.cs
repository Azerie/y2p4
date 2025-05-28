using UnityEngine;
// Make sure you have FMODUnity namespace if it's not automatically recognized
// using FMODUnity; 

public class CompoundMuffleTrigger : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("The tag assigned to your player GameObject.")]
    public string playerTag = "Player";

    [Header("FMOD Target")]
    [Tooltip("Drag the GameObject that has the FMOD StudioEventEmitter for your ambient sound here (e.g., your 'music 1 object').")]
    public FMODUnity.StudioEventEmitter targetEmitter;

    [Header("FMOD Parameter Settings")]
    [Tooltip("The exact name of the parameter in your FMOD event (e.g., 'Filter').")]
    public string fmodParameterName = "Filter";
    [Tooltip("The FMOD parameter value for the fully muffled sound (usually 1.0 for a 0-1 range parameter).")]
    public float muffledValue = 1.0f;
    [Tooltip("The FMOD parameter value for the normal, unmuffled sound (usually 0.0).")]
    public float normalValue = 0.0f;

    private int overlappingCollidersCount = 0;
    private bool isPlayerConsideredInZone = false; // Tracks if the script logic considers player in zone

    void Start()
    {
        if (targetEmitter == null)
        {
            Debug.LogError("ERROR: 'Target Emitter' is NOT ASSIGNED in the CompoundMuffleTrigger script on GameObject: '" + gameObject.name + "'. Please assign it in the Inspector. Script will be disabled.");
            enabled = false; // Disable this script if the target isn't set
            return;
        }
        // Optional: If the event might be playing at start and player isn't in the zone, ensure it's normal.
        // This depends on your game's startup logic. If unsure, you can leave this commented.
        // if (targetEmitter.IsPlaying())
        // {
        //     targetEmitter.SetParameter(fmodParameterName, normalValue);
        // }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enabled || targetEmitter == null) return; // Early exit if script disabled or no target

        if (other.CompareTag(playerTag))
        {
            overlappingCollidersCount++;
            if (!isPlayerConsideredInZone && overlappingCollidersCount > 0) // Player just entered the first part of the compound zone
            {
                isPlayerConsideredInZone = true;
                Debug.Log("Player '" + other.name + "' ENTERED compound muffle zone '" + gameObject.name + "'. Setting FMOD Parameter '" + fmodParameterName + "' on '" + targetEmitter.gameObject.name + "' to: " + muffledValue);
                targetEmitter.SetParameter(fmodParameterName, muffledValue);
            }
            // Debug.Log("Entered a child trigger. Current count: " + overlappingCollidersCount);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!enabled || targetEmitter == null) return; // Early exit if script disabled or no target

        if (other.CompareTag(playerTag))
        {
            if (overlappingCollidersCount > 0) // Only decrement if we had registered an enter for the player
            {
                overlappingCollidersCount--;
            }

            if (isPlayerConsideredInZone && overlappingCollidersCount == 0) // Player has exited the last part of the compound zone
            {
                isPlayerConsideredInZone = false;
                Debug.Log("Player '" + other.name + "' EXITED compound muffle zone '" + gameObject.name + "'. Setting FMOD Parameter '" + fmodParameterName + "' on '" + targetEmitter.gameObject.name + "' to: " + normalValue);
                targetEmitter.SetParameter(fmodParameterName, normalValue);
            }

            // Safety for count going negative (shouldn't happen with balanced enter/exit on same object)
            if (overlappingCollidersCount < 0)
            {
                Debug.LogWarning("Overlapping colliders count went negative on " + gameObject.name + ". Resetting to 0.");
                overlappingCollidersCount = 0;
                if (isPlayerConsideredInZone) // If we still thought player was in, reset parameter
                {
                    targetEmitter.SetParameter(fmodParameterName, normalValue);
                    isPlayerConsideredInZone = false;
                }
            }
            // Debug.Log("Exited a child trigger. Current count: " + overlappingCollidersCount);
        }
    }

    // Optional: Handle case where the muffle zone GameObject itself is disabled
    void OnDisable()
    {
        // If the muffle zone is disabled while the player was considered inside it
        if (isPlayerConsideredInZone && targetEmitter != null)
        {
            Debug.Log("Muffle Zone '" + gameObject.name + "' was disabled. Resetting FMOD parameter '" + fmodParameterName + "' on '" + targetEmitter.gameObject.name + "' to " + normalValue);
            targetEmitter.SetParameter(fmodParameterName, normalValue);
            overlappingCollidersCount = 0; // Reset count
            isPlayerConsideredInZone = false; // Reset state
        }
    }
}