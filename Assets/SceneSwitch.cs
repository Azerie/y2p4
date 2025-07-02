using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Scene Switch Settings")]
    public float timeUntilSwitch = 5f; // Time in seconds
    public string sceneToLoad = "NextScene"; // Name of the scene to load

    private float timer;

    void Start()
    {
        timer = timeUntilSwitch;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
