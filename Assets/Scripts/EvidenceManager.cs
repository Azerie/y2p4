using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EvidenceManager : MonoBehaviour
{
    [SerializeField] private GameObject newJournalEntryPopup;
    private List<Collectable> evidenceJournal = new List<Collectable>();
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
    public List<Collectable> GetJournal() { return evidenceJournal; }

    public void AddEntry(Collectable line)
    {
        evidenceJournal.Add(line);
        if (newJournalEntryPopup != null)
        {
            newJournalEntryPopup.SetActive(true);
        }
        EvidenceJournalChanged?.Invoke();
    }

    public void RemoveEntry(Collectable line)
    {
        if (evidenceJournal.Contains(line))
        {
            evidenceJournal.Remove(line);
            EvidenceJournalChanged?.Invoke();
        }
    }

    public void ClearJournal()
    {
        evidenceJournal = new List<Collectable>();
        EvidenceJournalChanged?.Invoke();
    }

    public void AddEvidence() {}
}
