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
    [SerializeField] private Transform player;
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
    }

    // Update is called once per frame
    void Update()
    {
        if(!isChasing && (transform.position - player.position).magnitude < detectionRange) {
            navMeshAgent.destination = player.position;
            isChasing = true;
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
    }
}
