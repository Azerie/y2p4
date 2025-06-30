using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EvidenceJournal : MonoBehaviour
{
    void Awake()
    {
        EvidenceManager.EvidenceJournalChanged += DrawUI;
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

    void DrawUI()
    {
        if (EvidenceManager.GetInstance() != null && this != null)
        {
            List<Collectable> items = EvidenceManager.GetInstance().GetJournal();
            int counter = 0;
            foreach (Transform entry in transform)
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
        TMP_Text entryTitle = entry.Find("Title").GetComponent<TMP_Text>();
        TMP_Text entryText = entry.Find("Text").GetComponent<TMP_Text>();
        // TMP_Text itemNumber = entry.Find("Counter").GetComponent<TMP_Text>();
        // Debug.Log(item.ItemName);

        entryTitle.text = item.CollectableName;
        entryText.text = item.JournalEntry;
    }
}
