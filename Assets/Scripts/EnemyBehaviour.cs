using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    [Tooltip("Transforms of points the enemy is cycling")]
    [SerializeField] private List<Transform> points = new();
    [Tooltip("How far away the enemy has to be from the destination point")]
    [SerializeField] private float confirmRange = 0.1f;
    [Tooltip("How far away the enemy detects player")]
    [SerializeField] private float detectionRange = 5f;
    [Tooltip("How far away the player has to be for the enemy to stop chasing")]
    [SerializeField] private float chaseRange = 5f;
    [Tooltip("1/2 of the angle of the detection cone (in degrees)")]
    [SerializeField] private float detectionAngle = 45f;
    [SerializeField] private float heightOffset = 0.1f;

    private Transform player;
    private Transform mainCamera;
    private float playerHeight;
    private float height;
    private NavMeshAgent navMeshAgent;
    private int currentPointIndex = 0;
    private bool isChasing = false;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if(currentPointIndex >= 0 || currentPointIndex < points.Count -1) {
            navMeshAgent.destination = points[currentPointIndex].position;
        } 
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        detectionAngle *= Mathf.Deg2Rad;
        playerHeight = player.GetComponentInChildren<CapsuleCollider>().height - heightOffset;
        height = GetComponentInChildren<CapsuleCollider>().height - heightOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isChasing && (transform.position - player.position).magnitude < detectionRange) {
            // navMeshAgent.destination = player.position;
            // isChasing = true;
        }
        else if(isChasing && (transform.position - player.position).magnitude > chaseRange) {
            navMeshAgent.destination = points[currentPointIndex].position;
            isChasing = false;
        }

        if(navMeshAgent.remainingDistance < confirmRange) {
            currentPointIndex++;
            if(currentPointIndex > points.Count - 1) {
                currentPointIndex = 0;
            }
            navMeshAgent.destination = points[currentPointIndex].position;
            isChasing = false;
        }

        Debug.Log("Can detect player? " + CanDetectPlayer());
        Debug.Log("Has line of sight to the player? " + HasLineOfSight());
    }

    private bool CanDetectPlayer() {
        Vector3 directLine = transform.position - player.position;
        bool isInRange = (directLine).magnitude < detectionRange;
        bool isInCone = (Vector3.Cross(directLine, transform.forward).magnitude / directLine.magnitude / transform.forward.magnitude) > Mathf.Cos(detectionAngle);
        return isInRange && isInCone;
    }

    private bool HasLineOfSight() {
        RaycastHit hit;
        Vector3 origin = transform.position + new Vector3(0, height, 0);
        Physics.Raycast(origin, player.position + new Vector3(0, playerHeight, 0) - origin, out hit);
        if(hit.collider.transform.CompareTag("Player")) {
            return true;
        }
        return false;
    }
}
