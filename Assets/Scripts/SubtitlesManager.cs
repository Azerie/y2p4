using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SubtitlesManager : MonoBehaviour
{
    private Queue<Voiceline> voicelineQueue = new Queue<Voiceline>();
    public static SubtitlesManager Instance;
    public static UnityAction ActiveSubtitleChanged;
    private float endTime;
    private bool isQueueEmpty = true;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (!isQueueEmpty && Time.time > endTime)
        {
            Instance.voicelineQueue.Dequeue();
            Voiceline newLine = Instance.voicelineQueue.Peek();
            if (newLine != null)
            {
                endTime = Time.time + newLine.SubtitlesTime;
            }
            else
            {
                Instance.isQueueEmpty = true;
            }
        }
    }
    public static void AddSubtitles(Voiceline line)
    {
        Instance.voicelineQueue.Enqueue(line);
        Instance.isQueueEmpty = false;
    }

    public static Voiceline GetCurrentSubtitles()
    {
        return Instance.voicelineQueue.Peek();
    }
    
    public static SubtitlesManager GetInstance()
    {
        return Instance;
    }
}
