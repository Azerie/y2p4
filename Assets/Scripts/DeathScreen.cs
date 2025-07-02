using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private GameObject deathCanvas;
    [SerializeField] private GameObject mainPauseMenu;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private PlayerControls playerControls;
    void Awake()
    {
        playerControls = FindObjectOfType<PlayerControls>();
        pauseMenu = GetComponent<PauseMenu>();
    }
    public void Die()
    {
        pauseMenu.Pause();
        mainPauseMenu.SetActive(false);
        deathCanvas.SetActive(true);
    }

    public void Respawn()
    {
        mainPauseMenu.SetActive(true);
        deathCanvas.SetActive(false);
        pauseMenu.Resume();
        
        Debug.Log("Respawn button pressed");

        foreach (EnemyBehaviour enemyBehaviour in FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None))
        {
            enemyBehaviour.Respawn();
        }
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