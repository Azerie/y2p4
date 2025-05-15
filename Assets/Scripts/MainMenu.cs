using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string playSceneName = "SampleScene";
    [SerializeField] private string menuSceneName = "MainMenu";
    public void Play() {
        SceneManager.LoadScene(playSceneName);
    }
    public void Menu() {
        SceneManager.LoadScene(menuSceneName);
    }

    public void Quit() {
        Application.Quit();
    }
}
