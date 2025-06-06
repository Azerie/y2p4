using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Movement & Detection")]
    [Tooltip("Transforms of points the enemy is cycling")]
    [SerializeField] private List<Transform> points = new();
    [Tooltip("Alternatively, assign the parent of the points here")]
    [SerializeField] private Transform pointsParent;
    [Tooltip("How far away the enemy detects player when player is not in vision cone")]
    [SerializeField] private float proximityDetectionRange = 1f;
    [Tooltip("How far away the enemy detects player when player is in vision cone")]
    [SerializeField] private float detectionRange = 5f;
    [Tooltip("1/2 of the angle of the detection cone (in degrees)")]
    [SerializeField] private float detectionAngle = 45f;
    [SerializeField] private float heightOffset = 0.1f;
    [Tooltip("How long it takes for the enemy to go from roaming to alert state while seeing the player")]
    [SerializeField] private float RoamingToAlertTime = 1f;
    [Tooltip("How long it takes for the enemy to go from alert to chasing state while seeing the player")]
    [SerializeField] private float AlertToChasingTime = 1f;
    [Tooltip("How long it takes for the enemy to go from alert to roaming state while not seeing the player")]
    [SerializeField] private float AlertToRoamingTime = 1f;
    [Tooltip("How long it takes for the enemy to go from chasing to alert state while not seeing the player")]
    [SerializeField] private float ChasingToAlertTime = 1f;
    [SerializeField] private float RoamingSpeed = 3f;
    [SerializeField] private float AlertSpeed = 4f;
    [SerializeField] private float ChasingSpeed = 6f;
    [Tooltip("How much slower the detection is at maximum range")]
    [SerializeField] private float MinDetectionCoef = 0.5f;
    [SerializeField] private bool failEnabled = true;
    [SerializeField] private string failSceneName = "MainMenu";

    [Header("FMOD State Sounds")]
    [Tooltip("FMOD event path for the alert state sound.")]
    public FMODUnity.EventReference m_AlertSoundEventPath = new FMODUnity.EventReference();
    [Tooltip("FMOD event path for the chasing state sound.")]
    public FMODUnity.EventReference m_ChasingSoundEventPath = new FMODUnity.EventReference();

    // Added Initializing state for robust startup
    public enum State { Initializing, Roaming, Alert, Chasing }
    private State state = State.Initializing; // Start in Initializing state
    private Transform player;
    private Transform mainCamera;
    private float playerHeight;
    private float height;
    private NavMeshAgent navMeshAgent;
    private int currentPointIndex = 0;
    private float stateTimer = 0f;
    private bool isPlayerHidden = false;

    private Animator enemyAnimator;
    private Rigidbody enemyRigidbody;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyRigidbody = GetComponent<Rigidbody>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        if (player != null) // Check if player is found
        {
            CapsuleCollider playerCollider = player.GetComponentInChildren<CapsuleCollider>();
            if (playerCollider != null)
            {
                playerHeight = playerCollider.height - heightOffset;
            }
            else
            {
                Debug.LogError("EnemyBehaviour: Player's CapsuleCollider not found!", player);
            }
        }
        else
        {
            Debug.LogError("EnemyBehaviour: Player GameObject not found! Ensure it has the 'Player' tag.", gameObject);
        }

        CapsuleCollider selfCollider = GetComponentInChildren<CapsuleCollider>();
        if (selfCollider != null)
        {
            height = selfCollider.height - heightOffset;
        }
        else
        {
            Debug.LogError("EnemyBehaviour: Enemy's CapsuleCollider not found in children!", gameObject);
        }


        HidingPlaceBehaviour.OnPlayerHidden += HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed += RevealPlayer;

        enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator == null)
        {
            Debug.LogError("EnemyBehaviour: Animator component not found in self or children.", gameObject); // Changed from Log to LogError
        }

        if (points.Count == 0 && pointsParent != null)
        {
            foreach (Transform point in pointsParent)
            {
                points.Add(point);
            }
        }

        // Initial destination setting moved to ChangeState for Roaming
        ChangeState(State.Roaming); // Initial state transition
    }

    void Update()
    {
        // Early exit if essential components are missing
        if (navMeshAgent == null || enemyAnimator == null || player == null) return;

        UpdateState();
        UpdateDestination();
    }

    void OnDestroy()
    {
        HidingPlaceBehaviour.OnPlayerHidden -= HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed -= RevealPlayer;
    }

    private void ChangeState(State newState)
    {
        // Guard to prevent re-running logic if already in the target state
        // This now works correctly because 'state' starts as 'Initializing'
        if (state == newState)
        {
            // Debug.LogWarning($"Attempted to change to the same state: {newState}"); // Optional debug
            return;
        }

        state = newState;
        stateTimer = 0;

        var meshRenderer = GetComponentInChildren<MeshRenderer>(); // Cache for slight optimization

        if (newState == State.Chasing)
        {
            if (meshRenderer) meshRenderer.material.SetColor("_BaseColor", Color.red);
            navMeshAgent.speed = ChasingSpeed;
            if (player != null) navMeshAgent.destination = player.position;
            enemyAnimator.Play("F_Run"); //
            PlayFMODEvent(m_ChasingSoundEventPath, "Chasing");
        }
        else if (newState == State.Alert)
        {
            if (meshRenderer) meshRenderer.material.SetColor("_BaseColor", Color.yellow);
            navMeshAgent.speed = AlertSpeed;
            if (player != null) navMeshAgent.destination = player.position;
            enemyAnimator.Play("Walking"); //
            PlayFMODEvent(m_AlertSoundEventPath, "Alert");
        }
        else if (newState == State.Roaming)
        {
            if (meshRenderer) meshRenderer.material.SetColor("_BaseColor", Color.green);
            navMeshAgent.speed = RoamingSpeed;
            if (points.Count > 0)
            {
                // Ensure currentPointIndex is valid before accessing points list
                if (currentPointIndex < 0 || currentPointIndex >= points.Count) currentPointIndex = 0;
                if (points.Count > 0) navMeshAgent.destination = points[currentPointIndex].position; // Check again in case currentPointIndex was reset
            }
            enemyAnimator.Play("Walking"); //
        }
    }

    private void PlayFMODEvent(FMODUnity.EventReference eventReference, string eventNameForLog)
    {
        if (!string.IsNullOrEmpty(eventReference.Path))
        {
            FMOD.Studio.EventInstance stateSoundInstance = FMODUnity.RuntimeManager.CreateInstance(eventReference);
            // Corrected line: passing gameObject instead of transform
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(stateSoundInstance, gameObject, enemyRigidbody);
            stateSoundInstance.start();
            stateSoundInstance.release();
            // Debug.Log($"EnemyBehaviour: Playing {eventNameForLog} sound."); // Optional: uncomment for debugging
        }
        else
        {
            Debug.LogWarning($"EnemyBehaviour: FMOD Event Path for {eventNameForLog} sound is not set.");
        }
    }

    private void GetNextPoint()
    {
        if (points.Count == 0) return;

        currentPointIndex++;
        if (currentPointIndex >= points.Count)
        {
            currentPointIndex = 0;
        }
        if (navMeshAgent.isOnNavMesh)
        {
            NavMeshPath path = new NavMeshPath();
            navMeshAgent.CalculatePath(points[currentPointIndex].position, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                navMeshAgent.destination = points[currentPointIndex].position;
            }
        }
    }

    private void UpdateDestination()
    {
        if (player == null) return;

        if (state == State.Chasing)
        {
            if (navMeshAgent.isOnNavMesh) navMeshAgent.destination = player.position;
        }
        else if (navMeshAgent.isOnNavMesh && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (state == State.Roaming && points.Count > 0)
            {
                GetNextPoint();
            }
        }
    }

    private void UpdateState()
    {
        if (player == null) return;

        if (state == State.Chasing)
        {
            if (CanSeePlayer())
            {
                if (stateTimer < 0) stateTimer = 0; // Reset timer if player re-seen
            }
            else
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= -ChasingToAlertTime)
                {
                    ChangeState(State.Alert);
                }
            }
        }
        else if (state == State.Alert)
        {
            if (CanSeePlayer())
            {
                if (stateTimer < 0) stateTimer = 0;
                stateTimer += Time.deltaTime * GetDetectionTimeCoef();
                if (stateTimer >= AlertToChasingTime)
                {
                    ChangeState(State.Chasing);
                }
            }
            else
            {
                if (IsAtDestination())
                {
                    enemyAnimator.Play("Idle");
                    float curRot = Mathf.Sin(Mathf.Max(0, -(stateTimer / AlertToRoamingTime)) * Mathf.PI * 2) * 90f;
                    stateTimer -= Time.deltaTime;
                    float newRot = Mathf.Sin(Mathf.Max(0, -(stateTimer / AlertToRoamingTime)) * Mathf.PI * 2) * 90f;
                    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, newRot - curRot, 0));

                    if (stateTimer <= -AlertToRoamingTime)
                    {
                        ChangeState(State.Roaming);
                    }
                }
            }
        }
        else if (state == State.Roaming)
        {
            if (CanSeePlayer())
            {
                stateTimer += Time.deltaTime * GetDetectionTimeCoef();
                if (stateTimer >= RoamingToAlertTime)
                {
                    ChangeState(State.Alert);
                }
            }
            else
            {
                if (stateTimer > 0)
                {
                    stateTimer -= Time.deltaTime;
                }
            }
        }
    }

    private bool IsPlayerInVisionCone()
    {
        if (player == null) return false;
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange) return false;
        if (distanceToPlayer < 0.001f) return true;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        Vector3 coneRayOrigin = transform.position + Vector3.up * height * 0.5f;
        Debug.DrawRay(coneRayOrigin, Quaternion.AngleAxis(-detectionAngle, Vector3.up) * transform.forward * detectionRange, Color.yellow);
        Debug.DrawRay(coneRayOrigin, Quaternion.AngleAxis(detectionAngle, Vector3.up) * transform.forward * detectionRange, Color.yellow);

        return angleToPlayer <= detectionAngle;
    }

    private bool IsPlayerClose()
    {
        if (player == null) return false;
        return (player.position - transform.position).sqrMagnitude < proximityDetectionRange * proximityDetectionRange;
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;
        RaycastHit hit;
        Vector3 origin = transform.position + new Vector3(0, height, 0);
        Vector3 playerTargetPos = player.position + new Vector3(0, playerHeight, 0);
        Vector3 directionToPlayer = playerTargetPos - origin;

        Debug.DrawRay(origin, directionToPlayer, Color.green);
        if (Physics.Raycast(origin, directionToPlayer, out hit, detectionRange + 1f))
        {
            return hit.transform.CompareTag("Player");
        }
        return false;
    }

    private bool CanSeePlayer()
    {
        if (player == null || isPlayerHidden) return false;

        bool hasSight = HasLineOfSight();
        bool inCone = IsPlayerInVisionCone();
        bool isClose = IsPlayerClose();

        return hasSight && (inCone || isClose);
    }

    private void HidePlayer() { isPlayerHidden = true; }
    private void RevealPlayer() { isPlayerHidden = false; }

    private bool IsAtDestination()
    {
        if (!navMeshAgent.isOnNavMesh || navMeshAgent.pathPending) return false;
        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (player == null) return;
        if (failEnabled && collision.transform.CompareTag("Player"))
        {
            SceneManager.LoadScene(failSceneName);
        }
    }

    float GetDetectionTimeCoef()
    {
        if (player == null) return 0f;
        float distSqr = (player.position - transform.position).sqrMagnitude;
        float detectionRangeSqr = detectionRange * detectionRange;
        float proximityRangeSqr = proximityDetectionRange * proximityDetectionRange;

        if (distSqr > detectionRangeSqr) return 0f;
        if (distSqr < proximityRangeSqr) return 1f;

        float dist = Mathf.Sqrt(distSqr);
        float linear0to1 = (detectionRange - dist) / (detectionRange - proximityDetectionRange);
        return Mathf.Lerp(MinDetectionCoef, 1f, linear0to1);
    }
}