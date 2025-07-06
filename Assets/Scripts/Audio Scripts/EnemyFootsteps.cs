using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent
using System.Collections; // Only if you were planning to use Coroutines, not strictly needed for this version

public class EnemyFootsteps : MonoBehaviour
{
    [Header("FMOD Settings")]
    [Tooltip("FMOD event path for footsteps.")]
    public FMODUnity.EventReference m_EventPath = new FMODUnity.EventReference();

    [Header("FMOD Parameters (Read-Only in Inspector)")]
    [Tooltip("Current value for the 'Terrain' parameter in FMOD (0 for Dirt/Ground, 1 for Floor). Set automatically.")]
    public float m_Terrain;
    [Tooltip("Current value for the 'WalkRun' parameter in FMOD (0 for walk, 1 for run). Set automatically.")]
    public float m_WalkRun;

    [Header("Step Control")]
    [Tooltip("The distance the enemy needs to travel to trigger a footstep sound.")]
    public float m_StepDistance = 2.0f;
    private float m_StepRand; // Randomized addition to step distance
    private Vector3 m_PrevPos; // Enemy's position in the previous frame
    private float m_DistanceTravelled; // Distance travelled since the last footstep

    [Header("Debugging")]
    [Tooltip("Enable to draw debug lines for raycasts and log FMOD parameter values.")]
    public bool m_Debug;
    private Vector3 m_LinePos; // Start position for the debug raycast line

    // Component References
    private NavMeshAgent navMeshAgent;
    private Animator enemyAnimator;
    private Rigidbody enemyRigidbody; // For FMOD 3D attributes

    // Constants for tags and layers (ensure these match your project settings)
    private const string GroundTag = "Ground";
    private const string FloorTag = "Floor";
    private const string WalkableSurfaceLayer = "WalkableSurface";

    // Animator state names used in EnemyBehaviour.cs for movement
    private const string AnimStateWalking = "Walking";
    private const string AnimStateRunning = "F_Run";

    void Start()
    {
        // Initialize random seed for step distance variation
        Random.InitState(System.DateTime.Now.Millisecond);
        m_StepRand = Random.Range(0.0f, 0.5f);

        m_PrevPos = transform.position;
        m_LinePos = transform.position; // Initialize debug line position

        // Get required components attached to the enemy GameObject
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("EnemyFootsteps: NavMeshAgent component not found on this GameObject.", gameObject);
        }

        // The EnemyBehaviour script uses GetComponentInChildren for the Animator
        enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator == null)
        {
            Debug.LogError("EnemyFootsteps: Animator component not found in self or children of this GameObject.", gameObject);
        }

        enemyRigidbody = GetComponent<Rigidbody>();
        if (enemyRigidbody == null && m_Debug)
        {
            // Rigidbody is recommended for FMOD's 3D event positioning if it has velocity.
            Debug.LogWarning("EnemyFootsteps: Rigidbody component not found. FMOD 3D attributes might be less accurate for velocity-based effects.", gameObject);
        }


        // Debug check for the WalkableSurface layer
        if (LayerMask.NameToLayer(WalkableSurfaceLayer) == -1 && m_Debug)
        {
            Debug.LogWarning($"EnemyFootsteps: The '{WalkableSurfaceLayer}' layer does not exist. Please create it in Project Settings -> Tags and Layers for accurate terrain detection.");
        }
    }

    void Update()
    {
        if (navMeshAgent == null) return; // Essential component missing

        // Check if the enemy is actively moving using NavMeshAgent's velocity
        if (navMeshAgent.velocity.magnitude > 0.1f) // Threshold to ensure significant movement
        {
            m_DistanceTravelled += (transform.position - m_PrevPos).magnitude;
        }
        else
        {
            // If not moving, or moving very slowly, reset distance travelled
            m_DistanceTravelled = 0.0f;
        }

        m_PrevPos = transform.position; // Always update previous position

        // Check if a footstep sound should be played
        if (m_DistanceTravelled >= m_StepDistance + m_StepRand)
        {
            PlayFootstepSound();
            m_DistanceTravelled = 0.0f; // Reset distance
            m_StepRand = Random.Range(0.0f, 0.5f); // Re-randomize for next step
        }

        if (m_Debug)
        {
            // Draw the raycast line used for terrain detection (updates if raycast happens)
            Debug.DrawLine(m_LinePos, m_LinePos + Vector3.down * 2.0f, Color.magenta);
        }
    }

    void PlayFootstepSound()
    {
        // --- 1. Determine Terrain Type ---
        m_Terrain = 0.0f; // Default to Dirt/Ground (parameter value 0)
        RaycastHit hit;
        LayerMask walkableLayerMask = LayerMask.GetMask(WalkableSurfaceLayer);

        // Adjust rayOrigin Y position based on your enemy's pivot point and height to ensure it starts slightly above the ground
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2.0f, walkableLayerMask))
        {
            if (m_Debug)
            {
                m_LinePos = rayOrigin; // For visualizing the ray
                Debug.Log($"EnemyFootsteps: Raycast hit '{hit.collider.name}' on Layer: '{LayerMask.LayerToName(hit.collider.gameObject.layer)}' with Tag: '{hit.collider.tag}'", hit.collider.gameObject);
            }

            if (hit.collider.CompareTag(FloorTag))
            {
                m_Terrain = 1.0f; // Floor (parameter value 1)
            }
            else if (hit.collider.CompareTag(GroundTag))
            {
                m_Terrain = 0.0f; // Dirt/Ground (parameter value 0)
            }
            // Add more 'else if' conditions here for other terrain tags if needed

            if (m_Debug) Debug.Log($"EnemyFootsteps: Surface Tag: '{hit.collider.tag}' -> Terrain Parameter set to: {m_Terrain}");
        }
        else
        {
            if (m_Debug)
            {
                m_LinePos = rayOrigin; // For visualizing the ray
                Debug.Log($"EnemyFootsteps: Surface Raycast: No object on '{WalkableSurfaceLayer}' layer was hit below the enemy. Defaulting Terrain to {m_Terrain}.");
            }
            // If no valid surface is hit, m_Terrain remains at its default (e.g., 0.0f).
            // You could choose to not play a sound here by returning, if desired.
        }

        // --- 2. Determine Walk/Run State ---
        m_WalkRun = 0.0f; // Default to walking (parameter value 0)
        if (enemyAnimator != null)
        {
            AnimatorStateInfo currentState = enemyAnimator.GetCurrentAnimatorStateInfo(0); // Assuming animation is on Layer 0
            if (currentState.IsName(AnimStateRunning)) // "F_Run" from EnemyBehaviour
            {
                m_WalkRun = 1.0f; // Running (parameter value 1)
            }
            else if (currentState.IsName(AnimStateWalking)) // "Walking" from EnemyBehaviour
            {
                m_WalkRun = 0.0f; // Walking
            }
            // If the enemy is moving but in an animation state not listed (e.g., an attack anim with root motion),
            // it will use the default m_WalkRun = 0.0f. This can be adjusted if other states imply running.
        }
        else
        {
            if (m_Debug) Debug.LogWarning("EnemyFootsteps: Enemy Animator component not found! Cannot accurately determine Walk/Run state. Defaulting to Walk.");
        }

        if (m_Debug)
        {
            Debug.Log($"EnemyFootsteps: FMOD Parameters - Terrain: {m_Terrain}, WalkRun: {m_WalkRun}");
        }

        // --- 3. Play FMOD Event ---
        if (!string.IsNullOrEmpty(m_EventPath.ToString()))
        {
            FMOD.Studio.EventInstance footstepEvent = FMODUnity.RuntimeManager.CreateInstance(m_EventPath);

            // Set 3D attributes for the sound event. Pass Rigidbody if available.
            footstepEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, enemyRigidbody));

            // Set FMOD parameters
            footstepEvent.setParameterByName("Terrain", m_Terrain);
            footstepEvent.setParameterByName("WalkRun", m_WalkRun);

            footstepEvent.start();
            footstepEvent.release(); // Release the event instance immediately after starting for one-shot sounds
        }
        else
        {
            if (m_Debug) Debug.LogWarning("EnemyFootsteps: FMOD Event Path (m_EventPath) is not set. No footstep sound will be played.");
        }
    }
}