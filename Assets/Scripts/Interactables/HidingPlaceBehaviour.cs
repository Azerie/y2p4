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
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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
            HidePlayer();
        }
        else
        {
            RevealPlayer();
        }
        hasPlayer = !hasPlayer;
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
    }

    public void RevealPlayer()
    {
        player.transform.position = playerOutsidePosition.position;
        player.GetComponent<PlayerControls>().EnableMovement();
        // isInHidingAnimation = true;
        OnPlayerRevealed();
    }
}
