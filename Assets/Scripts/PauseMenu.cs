using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    private PlayerControls playerControls;
    void Awake()
    {
        playerControls = FindObjectOfType<PlayerControls>();
        Resume();
    }
    public void Resume()
    {
        Time.timeScale = 1;
        gameObject.GetComponent<Canvas>().enabled = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerControls != null)
        {
            playerControls.EnableMovement();
        }
    }
    public void Pause()
    {
        Time.timeScale = 0;
        gameObject.GetComponent<Canvas>().enabled = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerControls != null)
        {
            playerControls.DisableMovement();
        }
    }
    public void Menu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void Quit() {
        Application.Quit();
    }
}