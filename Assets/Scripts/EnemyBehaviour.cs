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
    [Tooltip("How far away the enemy detects player")]
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
    [SerializeField] private bool failEnabled = true;

    [SerializeField] private string failSceneName = "MainMenu";
    


    enum State {Roaming, Alert, Chasing}
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
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if(currentPointIndex >= 0 || currentPointIndex < points.Count -1) {
            navMeshAgent.destination = points[currentPointIndex].position;
        } 
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        playerHeight = player.GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        height = GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        HidingPlaceBehaviour.OnPlayerHidden += HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed += RevealPlayer;

        ChangeState(State.Roaming);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
        UpdateDestination();

        // Debug.Log("Can detect player? " + CanDetectPlayer());
        // Debug.Log("Has line of sight to the player? " + HasLineOfSight());
    }

    void OnDestroy()
    {
        HidingPlaceBehaviour.OnPlayerHidden -= HidePlayer;
        HidingPlaceBehaviour.OnPlayerRevealed -= RevealPlayer;
    }

    private void ChangeState(State newState) {
        if(newState == State.Chasing) {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", Color.red);
            navMeshAgent.speed = ChasingSpeed;
            navMeshAgent.destination = player.position;
        }
        else if (newState == State.Alert) {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", Color.yellow);
            navMeshAgent.speed = AlertSpeed;
            navMeshAgent.destination = player.position;
        }
        else if (newState == State.Roaming) {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", Color.green);
            navMeshAgent.speed = RoamingSpeed;
            navMeshAgent.destination = points[currentPointIndex].position;
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
        else if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (state == State.Roaming)
            {
                GetNextPoint();
            }
        }
    }

    private void UpdateState() {
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
                stateTimer += Time.deltaTime;
                if (stateTimer >= AlertToChasingTime)
                {
                    ChangeState(State.Chasing);
                }
            }
            else if (IsAtDestination())
            {
                stateTimer -= Time.deltaTime;
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
                stateTimer += Time.deltaTime;
                if (stateTimer >= RoamingToAlertTime)
                {
                    ChangeState(State.Alert);
                }
            }
            else
            {
                if(stateTimer > 0)
                {
                    stateTimer -= Time.deltaTime;
                }
            }
        }
    }

    private bool IsPlayerInVisionCone() {
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

    private bool HasLineOfSight() {
        RaycastHit hit;
        Vector3 origin = transform.position + new Vector3(0, height, 0);
        
        Debug.DrawRay(origin, player.position + new Vector3(0, playerHeight, 0) - origin, Color.green);
        if(Physics.Raycast(origin, player.position + new Vector3(0, playerHeight, 0) - origin, out hit)) {
            if(hit.transform.CompareTag("Player")) {
                return true;
            }
        }
        return false;
    }

    private bool CanSeePlayer() {
        return !isPlayerHidden && HasLineOfSight() && IsPlayerInVisionCone();
    }

    private void HidePlayer() {
        isPlayerHidden = true;
    }
    private void RevealPlayer() {
        isPlayerHidden = false;
    }

    private bool IsAtDestination() {
        float dist = navMeshAgent.remainingDistance;
        return dist != Mathf.Infinity && navMeshAgent.pathStatus==NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("enemy collision");

        if (failEnabled && collision.transform.CompareTag("Player")) {
            SceneManager.LoadScene(failSceneName);
        }
    }
}
