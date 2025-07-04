using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvidence", menuName = "Collectable")]
public class Collectable : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string collectableName;
    [SerializeField] private string journalEntry;
    [SerializeField] private Sprite icon;

    [Header("Extra Journal Info")]
    [SerializeField] private bool bloodyBackground;
    [SerializeField] private bool important;
    [SerializeField, TextArea(3,9)] private string journalSummary;

    public string CollectableName => collectableName;
    public string JournalEntry => journalEntry;
    public Sprite Icon => icon;
    public bool BloodyBackground => bloodyBackground;
    public bool Important => important;
    public string JournalSummary => journalSummary;

}
