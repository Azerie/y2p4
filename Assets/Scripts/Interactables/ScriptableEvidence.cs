using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvidence", menuName = "Collectable")]
public class Collectable : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string collectableName;
    [SerializeField] private string journalEntry;
    [SerializeField] private Sprite icon;

    public string CollectableName => collectableName;
    public string JournalEntry => journalEntry;
    public Sprite Icon => icon;
}
