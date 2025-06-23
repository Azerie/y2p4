using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class EnemyBehaviour : MonoBehaviour
{
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
    [Tooltip("How much slower the detection is at maximum range while in roaming state")]
    [SerializeField] private float MinDetectionCoefRoaming = 0.5f;
    [Tooltip("How much slower the detection is at maximum range while in alert state")]
    [SerializeField] private float MinDetectionCoefAlert = 0.5f;

    [SerializeField] private bool failEnabled = true;
    [SerializeField] private string failSceneName = "MainMenu";
    [SerializeField] private float killAnimationTime = 0.5f;
    [SerializeField] private float knockoutTime = 2f;
    [SerializeField] private bool isLethal = true;

    [Header("Animation names")]
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string walkingAnimationName = "Walking";
    [SerializeField] private string runningAnimationName = "F_Run";
    [SerializeField] private string attackAnimationName = "Idle";
    [SerializeField] private string knockedAnimationName = "Idle";

    [Header("FMOD State Sounds")]
    [Tooltip("FMOD event path for the alert state sound.")]
    public FMODUnity.EventReference m_AlertSoundEventPath = new FMODUnity.EventReference();
    [Tooltip("FMOD event path for the chasing state sound.")]
    public FMODUnity.EventReference m_ChasingSoundEventPath = new FMODUnity.EventReference();



    public enum State { Roaming, Alert, Chasing, KillAnimation, SkillCheck, Knocked }
    private State state = State.Roaming;
    private Transform player;
    private Transform mainCamera;
    private float playerHeight;
    private float height;
    private int currentPointIndex = 0;
    private float stateTimer = 0f;
    private bool isPlayerHidden = false;
    private float lookAroundTimer = 0f;
    private SkillCheck skillCheck;
    public static event UnityAction OnKillAnimationStart;
    public static event UnityAction OnKillAnimationEnd;
    
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Rigidbody _rb;
    private Collider _collider;
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        playerHeight = player.GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        height = GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        HidingPlaceBehaviour.OnPlayerHidden += HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed += RevealPlayer;
        OnKillAnimationStart += HidePlayer;
        OnKillAnimationEnd += RevealPlayer;

        _animator = GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.Log("no fk animator");
        }
        skillCheck = FindObjectOfType<SkillCheck>();

        if (points.Count == 0 && pointsParent != null)
        {
            foreach (Transform point in pointsParent)
            {
                points.Add(point);
            }
        }
        if (currentPointIndex >= 0 || currentPointIndex < points.Count - 1)
        {
            _navMeshAgent.destination = points[currentPointIndex].position;
        }

        ChangeState(State.Roaming);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
        UpdateDestination();

        // Debug.Log(GetDetectionTimeCoef());
        // Debug.Log("Can detect player? " + CanDetectPlayer());
        // Debug.Log("Has line of sight to the player? " + HasLineOfSight());
    }

    void OnDestroy()
    {
        HidingPlaceBehaviour.OnPlayerHidden -= HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed -= RevealPlayer;
        OnKillAnimationStart -= HidePlayer;
        OnKillAnimationEnd -= RevealPlayer;
    }

    private void ChangeState(State newState)
    {
        if (newState == State.Chasing)
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
            _navMeshAgent.speed = ChasingSpeed;
            if (player != null)
            {
                _navMeshAgent.destination = player.position;
            }
            if (_animator != null)
            {
                _animator.Play(runningAnimationName);
            }
            PlayFMODEvent(m_ChasingSoundEventPath, "Chasing");
        }
        else if (newState == State.Alert)
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
            _navMeshAgent.speed = AlertSpeed;
            if (player != null)
            {
                _navMeshAgent.destination = player.position;
            }
            if (_animator != null)
            {
                _animator.Play(walkingAnimationName);
            }
            lookAroundTimer = 0;
            PlayFMODEvent(m_AlertSoundEventPath, "Alert");
        }
        else if (newState == State.Roaming)
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            _navMeshAgent.speed = RoamingSpeed;
            _navMeshAgent.destination = points[currentPointIndex].position;
            if (_animator != null)
            {
                _animator.Play(walkingAnimationName);
            }
        }
        else if (newState == State.KillAnimation)
        {
            _navMeshAgent.destination = transform.position;
            OnKillAnimationStart();
            if (_animator != null)
            {
                _animator.Play(attackAnimationName);
            }
        }
        else if (newState == State.SkillCheck)
        {
            skillCheck.StartMinigame();
        }
        else if (newState == State.Knocked)
        {
            _collider.enabled = false;
            if (_animator != null)
            {
                _animator.Play(knockedAnimationName);
            }
        }
        state = newState;
        stateTimer = 0;
    }

    private void PlayFMODEvent(FMODUnity.EventReference eventReference, string eventNameForLog)
    {
        if (!string.IsNullOrEmpty(eventReference.Path))
        {
            FMOD.Studio.EventInstance stateSoundInstance = FMODUnity.RuntimeManager.CreateInstance(eventReference);
            // Corrected line: passing gameObject instead of transform
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(stateSoundInstance, gameObject, _rb);
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
        currentPointIndex++;
        if (currentPointIndex > points.Count - 1)
        {
            currentPointIndex = 0;
        }
        NavMeshPath path = new NavMeshPath();
        _navMeshAgent.CalculatePath(points[currentPointIndex].position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            _navMeshAgent.destination = points[currentPointIndex].position;
        }
        else
        {
            GetNextPoint();
        }
    }

    private void UpdateDestination()
    {
        if (state == State.Chasing)
        {
            _navMeshAgent.destination = player.position;
        }
        else if (state == State.Roaming && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            GetNextPoint();
        }
    }

    private void UpdateState()
    {
        if (state == State.Chasing)
        {
            if (CanSeePlayer())
            {
                if (stateTimer < 0)
                {
                    stateTimer = 0;
                }
            }
            else
            {
                // for state timers i use negative numbers for lowering aggression and positive for upping it
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
                if (stateTimer < 0)
                {
                    stateTimer = 0;
                }
                stateTimer += Time.deltaTime * GetDetectionTimeCoef();
                if (stateTimer >= AlertToChasingTime)
                {
                    ChangeState(State.Chasing);
                }
            }
            else if (IsAtDestination())
            {
                if (_animator != null)
                {
                    _animator.Play(idleAnimationName);
                }
                float curRot = Mathf.Sin(Mathf.Max(0, -(lookAroundTimer / AlertToRoamingTime)) * Mathf.PI * 2) * 90;
                // Debug.Log("time: " + (lookAroundTimer / AlertToRoamingTime).ToString() + "rotation: " + curRot);
                lookAroundTimer -= Time.deltaTime;
                float newRot = Mathf.Sin(Mathf.Max(0, -(lookAroundTimer / AlertToRoamingTime)) * Mathf.PI * 2) * 90;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, newRot - curRot, 0));
                if (lookAroundTimer <= -AlertToRoamingTime)
                {
                    ChangeState(State.Roaming);
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
        else if (state == State.KillAnimation)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= killAnimationTime)
            {
                if(isLethal)
                {
                    KillPlayer();
                }
                else
                {
                    ChangeState(State.SkillCheck);
                }
            }
        }
        else if (state == State.SkillCheck)
        {
            if (!skillCheck.IsActive())
            {
                Debug.Log("Skillcheck over, Result: " + skillCheck.WasLastCheckPassed());
                if (skillCheck.WasLastCheckPassed())
                {
                    ChangeState(State.Knocked);
                    player.gameObject.GetComponent<PlayerControls>().ExitKillAnimation();
                }
                else
                {
                    KillPlayer();
                }
            }
        }
        else if (state == State.Knocked)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= knockoutTime)
            {
                _collider.enabled = true;
                OnKillAnimationEnd();
                ChangeState(State.Roaming);
            }
        }
    }

    private bool IsPlayerInVisionCone()
    {
        Vector3 directLine = player.position - transform.position;
        bool isInRange = (directLine).magnitude < detectionRange;
        bool isInCone = (Vector3.Dot(directLine, transform.forward) / directLine.magnitude / transform.forward.magnitude) > Mathf.Cos(detectionAngle * Mathf.Deg2Rad);
        // Debug.DrawRay(transform.position, transform.forward, Color.blue);
        // Debug.DrawRay(transform.position, Vector3.Dot(directLine, transform.forward) * Vector3.up, Color.blue);
        // Debug.DrawRay(transform.position, directLine, Color.red);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-detectionAngle, Vector3.up) * transform.forward * detectionRange, Color.yellow);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(detectionAngle, Vector3.up) * transform.forward * detectionRange, Color.yellow);
        return isInRange && isInCone;
    }

    private bool IsPlayerClose()
    {
        Vector3 directLine = player.position - transform.position;
        return directLine.magnitude < proximityDetectionRange;
    }

    public bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + new Vector3(0, height, 0);
        Debug.DrawRay(origin, player.position + new Vector3(0, playerHeight, 0) - origin, Color.green);
        if (Physics.Raycast(origin, player.position + new Vector3(0, playerHeight, 0) - origin, out hit))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private bool CanSeePlayer()
    {
        return !isPlayerHidden && HasLineOfSight() && (IsPlayerInVisionCone() || IsPlayerClose());
    }

    private void HidePlayer()
    {
        isPlayerHidden = true;
    }
    private void RevealPlayer()
    {
        isPlayerHidden = false;
    }

    private bool IsAtDestination()
    {
        float dist = _navMeshAgent.remainingDistance;
        return dist != Mathf.Infinity && _navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && _navMeshAgent.remainingDistance == 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("enemy collision");

        if (!isPlayerHidden && failEnabled && collision.transform.CompareTag("Player"))
        {
            ChangeState(State.KillAnimation);
            collision.gameObject.GetComponent<PlayerControls>().StartKillAnimation(this);
        }
    }

    float GetDetectionTimeCoef()
    {
        float dist = (player.position - transform.position).magnitude;
        if (dist > detectionRange)
        {
            return 0;
        }
        if (dist < proximityDetectionRange)
        {
            return 1;
        }
        float linear0to1 = (detectionRange - dist) / (detectionRange - proximityDetectionRange);
        // Debug.Log("dist: " + dist.ToString() + "linear: " + linear0to1.ToString());
        float MinDetectionCoef = 0;
        if (state == State.Roaming) { MinDetectionCoef = MinDetectionCoefRoaming; }
        else { MinDetectionCoef = MinDetectionCoefAlert; }

        return linear0to1 * (1 - MinDetectionCoef) + MinDetectionCoef;
    }

    private void KillPlayer()
    {
        OnKillAnimationEnd();
        SceneManager.LoadScene(failSceneName);
        Cursor.lockState = CursorLockMode.None;
    }

    public State GetState() { return state; }

    public float GetHeight() { return height; }
}