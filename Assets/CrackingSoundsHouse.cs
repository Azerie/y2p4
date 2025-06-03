using UnityEngine;
using FMODUnity; // Import the FMODUnity namespace

public class PlaySoundInArea : MonoBehaviour
{
    [Header("FMOD Event")]
    [SerializeField] private EventReference soundEvent; // Drag your FMOD event here in the Inspector

    [Header("Timing")]
    [SerializeField] private float playInterval = 30f; // Interval in seconds

    private float timer;
    private bool playerInArea = false;
    private FMOD.Studio.EventInstance soundInstance;

    void Start()
    {
        timer = playInterval; // Initialize timer
    }

    void Update()
    {
        if (playerInArea)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                PlayFMODEvent();
                timer = playInterval; // Reset timer
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player (or whatever object you want to detect)
        // You might want to use tags or layers for more specific detection.
        if (other.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag
        {
            playerInArea = true;
            Debug.Log("Player entered the area.");
            // Optionally, play the sound immediately when the player enters
            // PlayFMODEvent();
            // timer = playInterval; // Reset timer if playing immediately
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInArea = false;
            Debug.Log("Player exited the area.");
            timer = playInterval; // Reset timer when player leaves

            // Optional: Stop the sound instance if it's playing and you want it to stop when leaving
            // if (soundInstance.isValid())
            // {
            //     soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //     soundInstance.release();
            // }
        }
    }

    void PlayFMODEvent()
    {
        if (!soundEvent.IsNull) // Check if the event reference is valid
        {
            // Create an instance of the FMOD event
            soundInstance = RuntimeManager.CreateInstance(soundEvent);

            // Optionally, attach the sound to this GameObject's position
            soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));

            // Start the event
            soundInstance.start();

            // Release the instance when it's done playing (if it's a one-shot)
            // For looping sounds or sounds you need to control later, you might manage this differently.
            // If your sound is a one-shot, it will release itself based on its FMOD Studio settings.
            // If you want to ensure it's released, you can call release after starting.
            // However, for repeated playback, creating and starting a new instance each time is common.
            // If the sound is long and you want to ensure only one plays at a time or stop the previous one:
            // if (soundInstance.isValid()) {
            //    soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // or ALLOWFADEOUT
            //    soundInstance.release();
            // }
            // soundInstance = RuntimeManager.CreateInstance(soundEvent);
            // soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
            // soundInstance.start();

            Debug.Log($"Playing FMOD event: {soundEvent.Path}");
        }
        else
        {
            Debug.LogError("FMOD Sound Event not assigned in the Inspector!");
        }
    }

    // Optional: Make sure to release the FMOD event instance if the GameObject is destroyed
    // and the sound might still be playing or allocated.
    void OnDestroy()
    {
        if (soundInstance.isValid())
        {
            soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            soundInstance.release();
        }
    }
}