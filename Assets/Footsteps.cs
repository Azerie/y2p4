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
    [Tooltip("The distance the player needs to travel to trigger a footstep sound.")]
    public float m_StepDistance = 2.0f;
    float m_StepRand;
    Vector3 m_PrevPos;
    float m_DistanceTravelled;

    [Header("Debugging")]
    [Tooltip("Enable to draw debug lines for raycasts and log FMOD parameter values.")]
    public bool m_Debug;
    Vector3 m_LinePos;

    private PlayerControls playerControls;
    private Rigidbody playerRigidbody;

    // Define the exact names of your tags here
    private const string GroundTag = "Ground";
    private const string FloorTag = "Floor";

    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        m_StepRand = Random.Range(0.0f, 0.5f);
        m_PrevPos = transform.position;
        m_LinePos = transform.position;

        playerControls = GetComponent<PlayerControls>();
        playerRigidbody = GetComponent<Rigidbody>();

        if (playerControls == null)
        {
            Debug.LogError("Footsteps script could not find PlayerControls script on this GameObject.");
        }
        if (playerRigidbody == null)
        {
            Debug.LogError("Footsteps script could not find Rigidbody component on this GameObject.");
        }

        // Check if layers and tags exist to help with debugging
        if (LayerMask.NameToLayer("WalkableSurface") == -1 && m_Debug)
        {
            Debug.LogWarning("Footsteps Script: The 'WalkableSurface' layer does not exist. Please create it in Project Settings -> Tags and Layers.");
        }
        // You might want to add checks for tag existence too, but Unity doesn't have a direct API for that like layers.
    }

    void Update()
    {
        if (playerRigidbody != null && playerRigidbody.velocity.magnitude > 0.1f && playerControls != null && playerControls.GetMoveInput() != Vector2.zero)
        {
            m_DistanceTravelled += (transform.position - m_PrevPos).magnitude;
        }
        else
        {
            m_DistanceTravelled = 0.0f;
        }

        if (m_DistanceTravelled >= m_StepDistance + m_StepRand)
        {
            PlayFootstepSound();
            m_StepRand = Random.Range(0.0f, 0.5f);
            m_DistanceTravelled = 0.0f;
        }

        m_PrevPos = transform.position;

        if (m_Debug)
        {
            Debug.DrawLine(m_LinePos, m_LinePos + Vector3.down * 2.0f, Color.cyan); // Changed color for clarity
        }
    }

    void PlayFootstepSound()
    {
        if (playerControls == null) return;

        m_Terrain = 0.0f; // Default to Dirt/Ground
        m_WalkRun = 0.0f; // Default to walking

        RaycastHit hit;
        // Raycast specifically for the "WalkableSurface" layer
        LayerMask walkableLayerMask = LayerMask.GetMask("WalkableSurface");

        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2.0f, walkableLayerMask))
        {
            if (m_Debug)
            {
                m_LinePos = rayOrigin;
                Debug.Log("Raycast hit: " + hit.collider.name + " on Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer) + " with Tag: " + hit.collider.tag);
            }

            // Now check the TAG of the hit object
            if (hit.collider.CompareTag(FloorTag))
            {
                m_Terrain = 1.0f; // Set Terrain to 1.0 for Floor
                if (m_Debug) Debug.Log("Surface Tag: " + FloorTag + " -> Terrain Parameter: 1.0");
            }
            else if (hit.collider.CompareTag(GroundTag))
            {
                m_Terrain = 0.0f; // Set Terrain to 0.0 for Dirt/Ground
                if (m_Debug) Debug.Log("Surface Tag: " + GroundTag + " -> Terrain Parameter: 0.0");
            }
            else
            {
                // Object is on "WalkableSurface" layer but has an unrecognized tag (or no tag for this purpose)
                // Defaulting to Dirt/Ground. You can change this if needed.
                m_Terrain = 0.0f;
                if (m_Debug) Debug.Log("Surface Tag: Unrecognized or other (" + hit.collider.tag + ") on WalkableSurface layer -> Terrain Parameter: 0.0 (Defaulting to Dirt/Ground)");
            }
        }
        else
        {
            if (m_Debug) Debug.Log("Surface: No object on 'WalkableSurface' layer hit.");
            return; // Don't play a sound if not on a designated surface
        }

        bool isTryingToMove = playerControls.GetMoveInput() != Vector2.zero;
        bool isSprinting = playerControls.IsSprinting();

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
            Debug.Log("FMOD Params - Terrain: " + m_Terrain + ", WalkRun: " + m_WalkRun);

        if (!string.IsNullOrEmpty(m_EventPath.Path))
        {
            FMOD.Studio.EventInstance footstepEvent = FMODUnity.RuntimeManager.CreateInstance(m_EventPath);
            footstepEvent.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
            footstepEvent.setParameterByName("Terrain", m_Terrain);
            footstepEvent.setParameterByName("WalkRun", m_WalkRun);
            footstepEvent.start();
            footstepEvent.release();
        }
        else
        {
            if (m_Debug) Debug.LogWarning("FMOD Event Path is not set for footsteps.");
        }
    }
}