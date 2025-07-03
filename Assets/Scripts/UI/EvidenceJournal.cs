using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EvidenceJournal : MonoBehaviour
{
    [SerializeField] private RectTransform journalEntries;
    [SerializeField] private GameObject evidenceJournalCanvas;
    [SerializeField] private GameObject mainPauseCanvas;
    private PauseMenu pauseMenu;
    private bool isEnabled = false;
    void Awake()
    {
        pauseMenu = GetComponent<PauseMenu>();
        // EvidenceManager.EvidenceJournalChanged += DrawUI;
    }
    // private void OnEvidenceJournalChanged()
    // {
    //     textField.text = "";
    //     List<string> texts = EvidenceManager.GetInstance().GetJournal();
    //     foreach (string text in texts)
    //     {
    //         textField.text += text + "\n";
    //     }
    // }

    void OpenJournal()
    {
        pauseMenu.Pause();
        mainPauseCanvas.SetActive(false);
        evidenceJournalCanvas.SetActive(true);
        DrawUI();
        isEnabled = true;
    }

    void CloseJournal()
    {
        pauseMenu.Resume();
        mainPauseCanvas.SetActive(true);
        evidenceJournalCanvas.SetActive(false);
        isEnabled = false;
    }

    public void OnJournalButton()
    {
        if (isEnabled)
        {
            CloseJournal();
        }
        else
        {
            OpenJournal();
        }
    }

    void DrawUI()
    {
        if (EvidenceManager.GetInstance() != null && this != null)
        {
            List<Collectable> items = EvidenceManager.GetInstance().GetJournal();
            int counter = 0;
            foreach (Transform entry in journalEntries)
            {
                if (counter < items.Count)
                {
                    entry.gameObject.SetActive(true);
                    DrawJournalEntry(items[counter], entry);
                    counter++;
                }
                else
                {
                    entry.gameObject.SetActive(false);
                }
            }
        }

    }

    void DrawJournalEntry(Collectable item, Transform entry)
    {
        TMP_Text entryTitle = entry.Find("Button").Find("Title").GetComponent<TMP_Text>();
        TMP_Text entryText = entry.Find("Text").GetComponent<TMP_Text>();
        // TMP_Text itemNumber = entry.Find("Counter").GetComponent<TMP_Text>();
        // Debug.Log(item.ItemName);

        entryTitle.text = item.CollectableName;
        entryText.text = item.JournalEntry;
    }

    public void DisableAllText()
    {
        foreach (Transform entry in transform)
        {
            entry.Find("Text").gameObject.SetActive(false);
        }
    }
}
