using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class anotherSceneSwitch : MonoBehaviour
{
    [Header("Scene Switch Settings")]
    [SerializeField] private string goodSceneToLoad = "NextScene";
    [SerializeField] private string badSceneToLoad = "NextScene";
    [SerializeField] private int requiredEvidenceNumber = 3;
    [SerializeField] private Item requiredItem;
    private string triggeringTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag(triggeringTag))
        {
            if (requiredItem != null && PlayerInventory.GetInstance().HasItem(requiredItem))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (EvidenceManager.GetInstance().GetJournal().Count >= requiredEvidenceNumber)
                {
                    SceneManager.LoadScene(goodSceneToLoad);
                }
                else
                {
                    SceneManager.LoadScene(badSceneToLoad);
                }
            }
        }
    }
}
