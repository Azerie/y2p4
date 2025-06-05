using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EvidenceJournal : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;

    void Awake()
    {
        EvidenceManager.EvidenceJournalChanged += OnEvidenceJournalChanged;
    }
    private void OnEvidenceJournalChanged()
    {
        textField.text = "";
        List<string> texts = EvidenceManager.GetInstance().GetJournal();
        foreach (string text in texts)
        {
            textField.text += text + "\n";
        }
    }
}
