using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class HidingPlaceBehaviour : InteractableBehaviourAbstract
{
    [SerializeField] private Transform playerHiddenPosition;
    [SerializeField] private Transform playerOutsidePosition;
    public static event UnityAction OnPlayerHidden;
    public static event UnityAction OnPlayerRevealed;

    private GameObject player;
    private bool hasPlayer = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public override void OnInteract()
    {
        if(hasPlayer) {
            player.transform.position = playerOutsidePosition.position;
            player.GetComponent<PlayerControls>().EnableMovement();
            OnPlayerRevealed();
        }
        else {
            player.transform.position = playerHiddenPosition.position;
            player.GetComponent<PlayerControls>().DisableMovement();
            OnPlayerHidden();
        }
        hasPlayer = !hasPlayer;
    }
}
