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
        GameObject pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
        pauseMenu.transform.GetComponent<Canvas>().enabled = false;
        Cursor.visible = false;
    }
    public void Menu() {
        SceneManager.LoadScene(menuSceneName);
    }

    public void Quit() {
        Application.Quit();
    }
}
