using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingPlaceBehaviour : InteractableBehaviourAbstract  // TODO: consider moving the hiding bool to the enemies behaviour actually probably do an event instead
{
    [SerializeField] private Transform playerHiddenPosition;
    [SerializeField] private Transform playerOutsidePosition;

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
            player.GetComponent<PlayerInteraction>().UnHide();
        }
        else {
            player.transform.position = playerHiddenPosition.position;
            player.GetComponent<PlayerControls>().DisableMovement();
            player.GetComponent<PlayerInteraction>().Hide();
        }
        hasPlayer = !hasPlayer;
    }
}
