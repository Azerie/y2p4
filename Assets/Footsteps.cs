using UnityEngine;
using System.Collections;

public class Footsteps : MonoBehaviour
{
    [Tooltip("FMOD event path for footsteps.")]
    public FMODUnity.EventReference m_EventPath = new FMODUnity.EventReference { Path = "event:/Footsteps/Dynamic footsteps" };

    [Header("FMOD Parameters (Read-Only)")]
    [Tooltip("Current value for the 'Terrain' parameter in FMOD (0 for Dirt/Ground, 1 for Floor).")]
    public float m_Terrain;
    [Tooltip("Current value for the 'WalkRun' parameter in FMOD (0 for walk, 1 for run).")]
    public float m_WalkRun;

    [Header("Step Control")]
    [Tooltip("The distance the player needs to travel to trigger a regular footstep sound.")] // cite: 1
    public float m_StepDistance = 2.0f; // cite: 1
    [Tooltip("Multiplier for Step Distance for the first footstep after starting movement (e.g., 0.25 for 25%). Set to 1.0 for no special first step distance. Very small values (e.g., 0.01) will make the first step play almost immediately.")]
    [Range(0.01f, 1.0f)]
    public float m_FirstStepDistanceFactor = 0.25f; // NEW: Tweakable factor for the first step

    private float m_StepRand; // cite: 1
    private Vector3 m_PrevPos; // cite: 1
    private float m_DistanceTravelled; // cite: 1
    private bool m_IsNextStepTheFirstSinceStop = true; // NEW: Flag for first step logic

    [Header("Debugging")]
    [Tooltip("Enable to draw debug lines for raycasts and log FMOD parameter values.")] // cite: 1
    public bool m_Debug; // cite: 1
    Vector3 m_LinePos; // cite: 1

    private PlayerControls playerControls; // cite: 1
    private Rigidbody playerRigidbody; // cite: 1

    // Define the exact names of your tags here
    private const string GroundTag = "Ground"; // cite: 1
    private const string FloorTag = "Floor"; // cite: 1

    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond); // cite: 1
        m_PrevPos = transform.position; // cite: 1
        m_LinePos = transform.position; // cite: 1
        m_IsNextStepTheFirstSinceStop = true; // Initialize: next step will be the first
        m_StepRand = Random.Range(0.0f, 0.5f); // Initialize m_StepRand // cite: 1

        playerControls = transform.parent.GetComponent<PlayerControls>(); // cite: 1
        playerRigidbody = transform.parent.GetComponent<Rigidbody>(); // cite: 1

        if (playerControls == null) // cite: 1
        {
            Debug.LogError("Footsteps script could not find PlayerControls script on this GameObject."); // cite: 1
        }
        if (playerRigidbody == null) // cite: 1
        {
            Debug.LogError("Footsteps script could not find Rigidbody component on this GameObject."); // cite: 1
        }

        if (LayerMask.NameToLayer("WalkableSurface") == -1 && m_Debug) // cite: 1
        {
            Debug.LogWarning("Footsteps Script: The 'WalkableSurface' layer does not exist. Please create it in Project Settings -> Tags and Layers."); // cite: 1
        }
    }

    void Update()
    {
        // Check if the player is considered to be actively trying to move and is physically moving
        bool hasMovementInput = playerControls != null && playerControls.GetMoveInput() != Vector2.zero; // cite: 1
        bool isPhysicallyMoving = playerRigidbody != null && playerRigidbody.velocity.magnitude > 0.05f; // Using a small threshold

        if (hasMovementInput && isPhysicallyMoving)
        {
            m_DistanceTravelled += (transform.position - m_PrevPos).magnitude; // cite: 1
            // If m_IsNextStepTheFirstSinceStop is true, it means we are accumulating distance for the first potential step
        }
        else
        {
            // Player is not actively moving (no input or no physical movement)
            if (m_DistanceTravelled > 0.0f) // Only log/reset if there was some distance travelled
            {
                if (m_Debug && !m_IsNextStepTheFirstSinceStop) Debug.Log("Player stopped or no input, resetting for next first step.");
            }
            m_DistanceTravelled = 0.0f; // cite: 1
            m_IsNextStepTheFirstSinceStop = true; // When movement resumes, it will be the "first step"
        }

        m_PrevPos = transform.position; // Update previous position every frame // cite: 1

        // Determine the target distance for the current step
        float targetDistanceForThisStep;
        if (m_IsNextStepTheFirstSinceStop)
        {
            // For the first step, use the factor. Ensure it's at least a very small positive distance.
            // m_StepRand is intentionally not used for the first step to make it more predictable.
            targetDistanceForThisStep = Mathf.Max(0.01f, m_StepDistance * m_FirstStepDistanceFactor);
            if (m_Debug && hasMovementInput && isPhysicallyMoving)
            {
                // This log can be very spammy, enable if specifically debugging first step distances
                // Debug.Log($"Waiting for first step. Target: {targetDistanceForThisStep:F2}, Current: {m_DistanceTravelled:F2}");
            }
        }
        else
        {
            // For subsequent steps, use the regular distance + random variation
            targetDistanceForThisStep = m_StepDistance + m_StepRand; // cite: 1
        }

        // Check if the footstep sound should be played
        // Only play if there's active input and physical movement to avoid sounds when sliding without input
        if (hasMovementInput && isPhysicallyMoving && m_DistanceTravelled >= targetDistanceForThisStep)
        {
            PlayFootstepSound(); // cite: 1
            m_DistanceTravelled = 0.0f; // Reset distance for the next step // cite: 1

            if (m_IsNextStepTheFirstSinceStop)
            {
                if (m_Debug) Debug.Log("First footstep played.");
                m_IsNextStepTheFirstSinceStop = false; // The next step will be a regular one
            }
            m_StepRand = Random.Range(0.0f, 0.5f); // Re-randomize for the *next* regular step // cite: 1
        }

        if (m_Debug) // cite: 1
        {
            Debug.DrawLine(m_LinePos, m_LinePos + Vector3.down * 2.0f, Color.cyan); // cite: 1
        }
    }

    void PlayFootstepSound()
    {
        if (playerControls == null) return; // cite: 1

        // Reset FMOD parameters to default before checking surface
        m_Terrain = 0.0f; // Default to Dirt/Ground // cite: 1
        m_WalkRun = 0.0f; // Default to walking // cite: 1

        RaycastHit hit;
        LayerMask walkableLayerMask = LayerMask.GetMask("WalkableSurface"); // cite: 1
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // cite: 1

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2.0f, walkableLayerMask)) // cite: 1
        {
            if (m_Debug) // cite: 1
            {
                m_LinePos = rayOrigin; // cite: 1
                Debug.Log("Raycast hit: " + hit.collider.name + " on Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer) + " with Tag: " + hit.collider.tag); // cite: 1
            }

            if (hit.collider.CompareTag(FloorTag)) // cite: 1
            {
                m_Terrain = 1.0f; // cite: 1
                if (m_Debug) Debug.Log("Surface Tag: " + FloorTag + " -> Terrain Parameter: 1.0"); // cite: 1
            }
            else if (hit.collider.CompareTag(GroundTag)) // cite: 1
            {
                m_Terrain = 0.0f; // cite: 1
                if (m_Debug) Debug.Log("Surface Tag: " + GroundTag + " -> Terrain Parameter: 0.0"); // cite: 1
            }
            else
            {
                m_Terrain = 0.0f; // cite: 1
                if (m_Debug) Debug.Log("Surface Tag: Unrecognized or other (" + hit.collider.tag + ") on WalkableSurface layer -> Terrain Parameter: 0.0 (Defaulting to Dirt/Ground)"); // cite: 1
            }
        }
        else
        {
            if (m_Debug) Debug.Log("Surface: No object on 'WalkableSurface' layer hit."); // cite: 1
            return; // Don't play a sound if not on a designated surface // cite: 1
        }

        bool isTryingToMove = playerControls.GetMoveInput() != Vector2.zero; // This re-checks input, already have hasMovementInput // cite: 1
        bool isSprinting = playerControls.IsSprinting(); // cite: 1

        if (isTryingToMove) // Redundant if covered by hasMovementInput in Update's call condition, but safe
        {
            if (isSprinting) // cite: 1
            {
                m_WalkRun = 1.0f; // cite: 1
            }
            else
            {
                m_WalkRun = 0.0f; // cite: 1
            }
        }

        if (m_Debug)
            Debug.Log("FMOD Params - Terrain: " + m_Terrain + ", WalkRun: " + m_WalkRun); // cite: 1

        if (!string.IsNullOrEmpty(m_EventPath.Path)) // cite: 1
        {
            FMOD.Studio.EventInstance footstepEvent = FMODUnity.RuntimeManager.CreateInstance(m_EventPath); // cite: 1
            footstepEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject)); // cite: 1
            footstepEvent.setParameterByName("Terrain", m_Terrain); // cite: 1
            footstepEvent.setParameterByName("WalkRun", m_WalkRun); // cite: 1
            footstepEvent.start(); // cite: 1
            footstepEvent.release(); // cite: 1
        }
        else
        {
            if (m_Debug) Debug.LogWarning("FMOD Event Path is not set for footsteps."); // cite: 1
        }
    }
}