using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SkipCutscene : MonoBehaviour
{
    public string nextSceneName = "YourNextSceneName"; // Replace with your scene name
    private bool canSkip = false;

    void Update()
    {
        if (canSkip && Keyboard.current.spaceKey.wasPressedThisFrame)
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
