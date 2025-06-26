using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class EnemyVoicelinePlayer : VoicelinePlayer
{
    [SerializeField] float RoamingTimeInterval = 30;
    [SerializeField] List<Voiceline> RoamingVoicelineList = new List<Voiceline>();
    [SerializeField] float AlertTimeInterval = 10;
    [SerializeField] List<Voiceline> AlertVoicelineList = new List<Voiceline>();
    [SerializeField] float ChasingTimeInterval = 10;
    [SerializeField] List<Voiceline> ChasingVoicelineList = new List<Voiceline>();
    private EnemyBehaviour stateMachine;
    private float timer;
    private int counterRoaming = 0;
    private int counterAlert = 0;
    private int counterChasing = 0;
    void Start()
    {
        stateMachine = GetComponent<EnemyBehaviour>();
        stateMachine.OnStateChange += ResetTimer;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (stateMachine.GetState() == EnemyBehaviour.State.Roaming && timer >= RoamingTimeInterval && RoamingVoicelineList.Count != 0)
        {
            PlayVoiceline(RoamingVoicelineList[counterRoaming]);
            counterRoaming++;
            if (counterRoaming >= RoamingVoicelineList.Count)
            {
                counterRoaming = 0;
            }
            ResetTimer();
        }
        else if (stateMachine.GetState() == EnemyBehaviour.State.Alert && timer >= AlertTimeInterval && AlertVoicelineList.Count != 0)
        {
            PlayVoiceline(AlertVoicelineList[counterAlert]);
            counterAlert++;
            if (counterAlert >= AlertVoicelineList.Count)
            {
                counterAlert = 0;
            }
            ResetTimer();
        }
        else if (stateMachine.GetState() == EnemyBehaviour.State.Chasing && timer >= ChasingTimeInterval && ChasingVoicelineList.Count != 0)
        {
            PlayVoiceline(ChasingVoicelineList[counterChasing]);
            counterChasing++;
            if (counterChasing >= ChasingVoicelineList.Count)
            {
                counterChasing = 0;
            }
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = 0;
    }
}
