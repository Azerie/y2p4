using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private Canvas deathCanvas;
    [SerializeField] private PlayerControls playerControls;
    void Awake()
    {
        playerControls = FindObjectOfType<PlayerControls>();
        deathCanvas = GetComponent<Canvas>();
    }
    public void Die()
    {
        Time.timeScale = 0;
        deathCanvas.enabled = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Respawn()
    {
        Time.timeScale = 1;
        deathCanvas.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerControls != null)
        {
            playerControls.Respawn();
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