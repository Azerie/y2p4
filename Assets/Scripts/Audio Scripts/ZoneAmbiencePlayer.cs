using UnityEngine;
using System.Collections.Generic; // Required for using Lists
using FMOD.Studio;
using FMODUnity;

/// <summary>
/// Plays a continuous FMOD event while a player is inside a compound trigger zone.
/// This version monitors a list of Enemies and adjusts a parameter on the 
/// ambience event based on the most aggressive enemy's state.
/// </summary>
public class ZoneAmbiencePlayer : MonoBehaviour
{
    // ------------------------------------------------------------------
    // SETTINGS
    // ------------------------------------------------------------------

    [Header("FMOD Event to Play")]
    [Tooltip("Select the continuous, loopable FMOD sound that will play inside this zone.")]
    public EventReference zoneAmbienceEvent;

    [Header("Player Detection")]
    [Tooltip("The tag of the PARENT player GameObject this zone should react to.")]
    public string playerParentTag = "Player";

    [Header("Enemy State Integration")]
    [Tooltip("Drag all Enemy GameObjects here. The ambience will react to the most aggressive one.")]
    [SerializeField] private List<EnemyBehaviour> enemiesToMonitor = new List<EnemyBehaviour>();

    [Tooltip("The name of the FMOD parameter to control (e.g., 'EnemyState').")]
    [SerializeField] private string parameterName = "EnemyState";


    // ------------------------------------------------------------------
    // INTERNAL STATE
    // ------------------------------------------------------------------
    private int _triggerCount = 0;
    private EventInstance _ambienceInstance;
    private bool _isPlayerInZone = false;


    private void Start()
    {
        if (enemiesToMonitor.Count == 0)
        {
            Debug.LogWarning($"The 'Enemies To Monitor' list is empty on {gameObject.name}. The FMOD parameter will not be updated.", this);
        }
    }

    // This will run every frame.
    private void Update()
    {
        // If the player is in the zone and we have a valid sound playing...
        if (_isPlayerInZone && _ambienceInstance.isValid())
        {
            // ...continuously update the FMOD parameter based on the highest enemy threat.
            UpdateAmbienceParameter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag(playerParentTag))
        {
            _triggerCount++;

            if (_triggerCount == 1)
            {
                Debug.Log("Player has entered the zone. Starting ambience.");
                _isPlayerInZone = true;

                _ambienceInstance = RuntimeManager.CreateInstance(zoneAmbienceEvent);
                RuntimeManager.AttachInstanceToGameObject(_ambienceInstance, gameObject);
                _ambienceInstance.start();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag(playerParentTag))
        {
            if (_triggerCount > 0)
            {
                _triggerCount--;
            }

            if (_triggerCount == 0)
            {
                Debug.Log("Player has exited the zone. Stopping ambience.");
                _isPlayerInZone = false;

                _ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _ambienceInstance.release();
            }
        }
    }

    /// <summary>
    /// Checks all monitored enemies and finds the most aggressive state among them.
    /// </summary>
    private void UpdateAmbienceParameter()
    {
        if (enemiesToMonitor == null || enemiesToMonitor.Count == 0) return;

        // Assume the least aggressive state to start.
        EnemyBehaviour.State highestState = EnemyBehaviour.State.Roaming;

        // Loop through all enemies to find the highest alert level.
        foreach (var enemy in enemiesToMonitor)
        {
            if (enemy == null) continue; // Skip if an entry in the list is empty.

            EnemyBehaviour.State currentState = enemy.GetState();

            // Chasing is the highest priority. If we find one, we can stop looking.
            if (currentState == EnemyBehaviour.State.Chasing)
            {
                highestState = EnemyBehaviour.State.Chasing;
                break;
            }
            // Alert is the next highest.
            else if (currentState == EnemyBehaviour.State.Alert)
            {
                highestState = EnemyBehaviour.State.Alert;
            }
        }

        // Now set the parameter based on the most aggressive state we found.
        float parameterValue = 0f;
        switch (highestState)
        {
            case EnemyBehaviour.State.Roaming:
                parameterValue = 0f;
                break;
            case EnemyBehaviour.State.Alert:
                parameterValue = 0.25f;
                break;
            case EnemyBehaviour.State.Chasing:
                parameterValue = 0.75f;
                break;
            default:
                parameterValue = 0f;
                break;
        }

        // Set the parameter. This will now be called every frame.
        _ambienceInstance.setParameterByName(parameterName, parameterValue);
    }


    private void OnDisable()
    {
        if (_ambienceInstance.isValid())
        {
            Debug.LogWarning("Zone object was disabled. Forcing sound to stop immediately.");
            _isPlayerInZone = false;
            _ambienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _ambienceInstance.release();
        }
    }
}
