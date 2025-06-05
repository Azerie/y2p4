using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EvidenceManager : MonoBehaviour
{
    private List<string> evidenceJournal = new List<string>();
    public static UnityAction EvidenceJournalChanged;
    public static EvidenceManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EvidenceJournalChanged?.Invoke();
    }

    public static EvidenceManager GetInstance()
    {
        return Instance;
    }
    public List<string> GetJournal() { return evidenceJournal; }

    public void AddLine(string line)
    {
        evidenceJournal.Add(line);
        EvidenceJournalChanged?.Invoke();
    }

    public void RemoveLine(string line)
    {
        if (evidenceJournal.Contains(line))
        {
            evidenceJournal.Remove(line);
            EvidenceJournalChanged?.Invoke();
        }
    }

    public void ClearJournal()
    {
        evidenceJournal = new List<string>();
        EvidenceJournalChanged?.Invoke();
    }

    public void AddEvidence() {}
}
