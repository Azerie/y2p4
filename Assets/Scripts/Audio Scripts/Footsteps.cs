using UnityEngine;
using UnityEngine.InputSystem; // Required for InputAction

public class Footsteps : MonoBehaviour
{
    [Tooltip("FMOD event path for footsteps.")]
    public FMODUnity.EventReference m_EventPath = new FMODUnity.EventReference { Path = "event:/Footsteps/Dynamic footsteps" };

    [Header("FMOD Parameters (Read-Only)")]
    [Tooltip("Current value for the 'Terrain' parameter in FMOD (0 for Dirt/Ground, 1 for Floor).")]
    public float m_Terrain;
    [Tooltip("Current value for the 'WalkRun' parameter in FMOD (0 for walk, 1 for run).")]
    public float m_WalkRun;
    [Tooltip("Current value for the 'isCrouching' parameter in FMOD (0 for not crouching, 1 for crouching).")]
    public float m_IsCrouching;

    // Internal state to track the crouch toggle
    private bool m_IsCrouchingState = false;

    [Header("Step Control")]
    public float m_StepDistance = 2.0f;
    [Range(0.01f, 1.0f)]
    public float m_FirstStepDistanceFactor = 0.25f;

    private float m_StepRand;
    private Vector3 m_PrevPos;
    private float m_DistanceTravelled;
    private bool m_IsNextStepTheFirstSinceStop = true;

    [Header("Debugging")]
    public bool m_Debug;
    Vector3 m_LinePos;

    private PlayerControls playerControls;
    private Rigidbody playerRigidbody;

    private const string GroundTag = "Ground";
    private const string FloorTag = "Floor";

    void Awake()
    {
        playerControls = transform.parent.GetComponent<PlayerControls>();
        playerRigidbody = transform.parent.GetComponent<Rigidbody>();

        if (playerControls == null)
        {
            Debug.LogError("Footsteps script could not find PlayerControls script on parent GameObject.");
        }
    }

    // Subscribe to events when the component is enabled
    void OnEnable()
    {
        if (playerControls != null)
        {
            // Find the action by its name (as a string). This is more reliable.
            // IMPORTANT: If your action isn't named "Crouch", change the string below.
            InputAction crouchAction = playerControls.asset.FindAction("Crouch"); // <-- MODIFIED

            if (crouchAction != null) // <-- MODIFIED
            {
                crouchAction.performed += ToggleCrouchState; // <-- MODIFIED
            }
            else
            {
                Debug.LogError("Input Action 'Crouch' not found! Please check the name in your Input Actions asset.", this);
            }
        }
    }

    // Unsubscribe when the component is disabled to prevent errors
    void OnDisable()
    {
        if (playerControls != null)
        {
            // Find the action again to safely unsubscribe
            InputAction crouchAction = playerControls.asset.FindAction("Crouch"); // <-- MODIFIED
            if (crouchAction != null) // <-- MODIFIED
            {
                crouchAction.performed -= ToggleCrouchState; // <-- MODIFIED
            }
        }
    }

    // This method is called by the event whenever the crouch button is pressed
    private void ToggleCrouchState(InputAction.CallbackContext context)
    {
        m_IsCrouchingState = !m_IsCrouchingState;
        if (m_Debug) Debug.Log("Crouch state toggled to: " + m_IsCrouchingState);
    }


    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        m_PrevPos = transform.position;
        m_LinePos = transform.position;
        m_IsNextStepTheFirstSinceStop = true;
        m_StepRand = Random.Range(0.0f, 0.5f);

        if (playerRigidbody == null)
        {
            Debug.LogError("Footsteps script could not find Rigidbody component on parent GameObject.");
        }

        if (LayerMask.NameToLayer("WalkableSurface") == -1 && m_Debug)
        {
            Debug.LogWarning("Footsteps Script: The 'WalkableSurface' layer does not exist. Please create it in Project Settings -> Tags and Layers.");
        }
    }

    void Update()
    {
        bool hasMovementInput = playerControls != null && playerControls.GetMoveInput() != Vector2.zero;
        bool isPhysicallyMoving = playerRigidbody != null && playerRigidbody.velocity.magnitude > 0.05f;

        if (hasMovementInput && isPhysicallyMoving)
        {
            m_DistanceTravelled += (transform.position - m_PrevPos).magnitude;
        }
        else
        {
            if (m_DistanceTravelled > 0.0f)
            {
                if (m_Debug && !m_IsNextStepTheFirstSinceStop) Debug.Log("Player stopped or no input, resetting for next first step.");
            }
            m_DistanceTravelled = 0.0f;
            m_IsNextStepTheFirstSinceStop = true;
        }

        m_PrevPos = transform.position;

        float targetDistanceForThisStep;
        if (m_IsNextStepTheFirstSinceStop)
        {
            targetDistanceForThisStep = Mathf.Max(0.01f, m_StepDistance * m_FirstStepDistanceFactor);
        }
        else
        {
            targetDistanceForThisStep = m_StepDistance + m_StepRand;
        }

        if (hasMovementInput && isPhysicallyMoving && m_DistanceTravelled >= targetDistanceForThisStep)
        {
            PlayFootstepSound();
            m_DistanceTravelled = 0.0f;

            if (m_IsNextStepTheFirstSinceStop)
            {
                if (m_Debug) Debug.Log("First footstep played.");
                m_IsNextStepTheFirstSinceStop = false;
            }
            m_StepRand = Random.Range(0.0f, 0.5f);
        }

        if (m_Debug)
        {
            Debug.DrawLine(m_LinePos, m_LinePos + Vector3.down * 2.0f, Color.cyan);
        }
    }

    void PlayFootstepSound()
    {
        if (playerControls == null) return;

        m_Terrain = 0.0f;
        m_WalkRun = 0.0f;

        RaycastHit hit;
        LayerMask walkableLayerMask = LayerMask.GetMask("WalkableSurface");
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2.0f, walkableLayerMask))
        {
            if (m_Debug)
            {
                m_LinePos = rayOrigin;
            }

            if (hit.collider.CompareTag(FloorTag))
            {
                m_Terrain = 1.0f;
            }
            else if (hit.collider.CompareTag(GroundTag))
            {
                m_Terrain = 0.0f;
            }
        }
        else
        {
            if (m_Debug) Debug.Log("Surface: No object on 'WalkableSurface' layer hit.");
            return;
        }

        bool isTryingToMove = playerControls.GetMoveInput() != Vector2.zero;
        bool isSprinting = playerControls.IsSprinting();

        // --- CROUCH LOGIC ---
        // Check our internal crouch state, which is toggled by the input event
        if (m_IsCrouchingState)
        {
            m_IsCrouching = 1.0f;
        }
        else
        {
            m_IsCrouching = 0.0f;
        }

        if (isTryingToMove)
        {
            if (isSprinting)
            {
                m_WalkRun = 1.0f;
            }
            else
            {
                m_WalkRun = 0.0f;
            }
        }

        if (m_Debug)
            Debug.Log("FMOD Params - Terrain: " + m_Terrain + ", WalkRun: " + m_WalkRun + ", isCrouching: " + m_IsCrouching);

        if (!string.IsNullOrEmpty(m_EventPath.Path))
        {
            FMOD.Studio.EventInstance footstepEvent = FMODUnity.RuntimeManager.CreateInstance(m_EventPath);
            footstepEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
            footstepEvent.setParameterByName("Terrain", m_Terrain);
            footstepEvent.setParameterByName("WalkRun", m_WalkRun);
            footstepEvent.setParameterByName("isCrouching", m_IsCrouching);
            footstepEvent.start();
            footstepEvent.release();
        }
        else
        {
            if (m_Debug) Debug.LogWarning("FMOD Event Path is not set for footsteps.");
        }
    }
}