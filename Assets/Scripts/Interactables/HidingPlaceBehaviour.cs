using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class HidingPlaceBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] private Transform playerHiddenPosition;
    [SerializeField] private Transform playerOutsidePosition;
    [SerializeField] private float hideTime = 0.5f;
    public static event UnityAction OnPlayerHidden;
    public static event UnityAction OnPlayerRevealed;
    private GameObject player;
    private float startTime;
    private float startYRotation;
    private bool isInHidingAnimation;
    private bool hasPlayer = false;
    private EnemyBehaviour[] enemies;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Awake()
    {
        enemies = GameObject.FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (isInHidingAnimation)
        {
            float t = (Time.time - startTime) / hideTime;

            float xPos = Mathf.Lerp(playerOutsidePosition.position.x, playerHiddenPosition.position.x, t);
            float yPos = Mathf.Lerp(playerOutsidePosition.position.y, playerHiddenPosition.position.y, t);
            float zPos = Mathf.Lerp(playerOutsidePosition.position.z, playerHiddenPosition.position.z, t);
            player.transform.position = new Vector3(xPos, yPos, zPos);

            float yRot = Mathf.Lerp(startYRotation, playerHiddenPosition.rotation.eulerAngles.y, t);
            player.transform.rotation = Quaternion.Euler(0, yRot, 0);

            if (t >= 1)
            {
                isInHidingAnimation = false;
                GetComponent<Collider>().enabled = true;
            }
        }
        
    }

    public override void OnInteract()
    {
        if (!hasPlayer)
        {
            bool canHide = true;
            foreach (EnemyBehaviour enemy in enemies)
            {
                canHide = canHide && (!enemy.HasLineOfSight() || enemy.GetState() != EnemyBehaviour.State.Chasing);
            }

            if (canHide)
            {
                HidePlayer();
            }
            else
            {
                Debug.Log("Cannot hide while being chased");
                // popup "cannot hide while he sees me" text here
            }
        }
        else
        {
            RevealPlayer();
        }
    }

    public void HidePlayer()
    {
        playerOutsidePosition.position = player.transform.position;
        player.GetComponent<PlayerControls>().DisableMovement();
        startTime = Time.time;
        startYRotation = player.transform.rotation.eulerAngles.y;
        isInHidingAnimation = true;
        GetComponent<Collider>().enabled = false;
        OnPlayerHidden();

        hasPlayer = true;
    }

    public void RevealPlayer()
    {
        player.transform.position = playerOutsidePosition.position;
        player.GetComponent<PlayerControls>().EnableMovement();
        // isInHidingAnimation = true;
        OnPlayerRevealed();

        hasPlayer = false;
    }
}
