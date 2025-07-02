using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class anotherSceneSwitch : MonoBehaviour
{
    [Header("Scene Switch Settings")]
    public string sceneToLoad = "NextScene";
    public string triggeringTag = "Player"; // Tag of the object that triggers the scene switch

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag(triggeringTag))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
