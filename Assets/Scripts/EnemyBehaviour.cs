using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
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
    [Tooltip("How much slower the detection is at maximum range")]
    [SerializeField] private float MinDetectionCoef = 0.5f;
    [SerializeField] private bool failEnabled = true;
    [SerializeField] private string failSceneName = "MainMenu";
    [SerializeField] private float killAnimationTime = 0.5f;



    public enum State { Roaming, Alert, Chasing, KillAnimation }
    private State state = State.Roaming;
    private Transform player;
    private Transform mainCamera;
    private float playerHeight;
    private float height;
    private NavMeshAgent navMeshAgent;
    private int currentPointIndex = 0;
    private float stateTimer = 0f;
    private bool isPlayerHidden = false;
    // Start is called before the first frame update

    Animator enemyAnimator;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        playerHeight = player.GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        height = GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        HidingPlaceBehaviour.OnPlayerHidden += HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed += RevealPlayer;

        enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator == null)
        {
            Debug.Log("no fk animator");
        }

        if (points.Count == 0 && pointsParent != null)
        {
            foreach (Transform point in pointsParent)
            {
                points.Add(point);
            }
        }
        if (currentPointIndex >= 0 || currentPointIndex < points.Count - 1)
        {
            navMeshAgent.destination = points[currentPointIndex].position;
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
    }

    private void ChangeState(State newState)
    {
        if (newState == State.Chasing)
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
            navMeshAgent.speed = ChasingSpeed;
            navMeshAgent.destination = player.position;
            if (enemyAnimator != null)
            {
                enemyAnimator.Play("F_Run");
            }
        }
        else if (newState == State.Alert)
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
            navMeshAgent.speed = AlertSpeed;
            navMeshAgent.destination = player.position;
            if (enemyAnimator != null)
            {
                enemyAnimator.Play("Walking");
            }
        }
        else if (newState == State.Roaming)
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            navMeshAgent.speed = RoamingSpeed;
            navMeshAgent.destination = points[currentPointIndex].position;
            if (enemyAnimator != null)
            {
                enemyAnimator.Play("Walking");
            }
        }
        else if (newState == State.KillAnimation)
        {
            navMeshAgent.destination = transform.position;
            if (enemyAnimator != null)
            {
                enemyAnimator.Play("Walking"); // change to the correct animation later
            }
        }
        state = newState;
        stateTimer = 0;
    }

    private void GetNextPoint()
    {
        currentPointIndex++;
        if (currentPointIndex > points.Count - 1)
        {
            currentPointIndex = 0;
        }
        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(points[currentPointIndex].position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            navMeshAgent.destination = points[currentPointIndex].position;
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
            navMeshAgent.destination = player.position;
        }
        else if (state == State.Roaming && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
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
                if (enemyAnimator != null)
                {
                    enemyAnimator.Play("Idle");
                }
                float curRot = Mathf.Sin(Mathf.Max(0, -(stateTimer / AlertToRoamingTime)) * Mathf.PI * 2) * 90;
                // Debug.Log("time: " + (stateTimer / AlertToRoamingTime).ToString() + "rotation: " + curRot);
                stateTimer -= Time.deltaTime;
                float newRot = Mathf.Sin(Mathf.Max(0, -(stateTimer / AlertToRoamingTime)) * Mathf.PI * 2) * 90;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, newRot - curRot, 0));
                if (stateTimer <= -AlertToRoamingTime)
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
            if(stateTimer >= killAnimationTime)
            {
                SceneManager.LoadScene(failSceneName);
                Cursor.lockState = CursorLockMode.None;
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
        return ((state == State.Chasing) || (!isPlayerHidden && HasLineOfSight())) && (IsPlayerInVisionCone() || IsPlayerClose());
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
        float dist = navMeshAgent.remainingDistance;
        return dist != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("enemy collision");

        if (failEnabled && collision.transform.CompareTag("Player"))
        {
            ChangeState(State.KillAnimation);
            collision.gameObject.GetComponent<PlayerControls>().DisableMovement();
            collision.gameObject.GetComponent<PlayerControls>().LookAt(transform);
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

        return linear0to1 * (1 - MinDetectionCoef) + MinDetectionCoef;
    }

    public State GetState() { return state; }
}