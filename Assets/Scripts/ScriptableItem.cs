using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;

    // Properties for access
    public string ItemName => itemName;
    public Sprite Icon => icon;
}
