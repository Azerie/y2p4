using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    void Start()
    {
        Resume();
    }
    public void Resume() {
        Time.timeScale = 1;
        gameObject.GetComponent<Canvas>().enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Pause()
    {
        Time.timeScale = 0;
        gameObject.GetComponent<Canvas>().enabled = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Menu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void Quit() {
        Application.Quit();
    }
}
