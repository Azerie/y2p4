using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipCutscene : MonoBehaviour
{
    public string nextSceneName = "YourNextSceneName"; // Replace with your scene name
    private bool canSkip = false;

    void Update()
    {
        if (canSkip && Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextScene();
        }
    }

    public void EnableSkip()  // Call from animation event
    {
        canSkip = true;
    }

    public void LoadNextScene()  // Call from animation event
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
