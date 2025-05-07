using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject UIEntryPrefab;
    [SerializeField] GameObject InventoryEntryContainer;

    [Header("Layout Settings")]
    [SerializeField] private Vector2 entryOffset = new Vector2(0, -50);
    [SerializeField] private Vector2 startingPosition = Vector2.zero;

    private void Awake()
    {
        PlayerInventory.InventoryChanged += OnItemGained;
        OnItemGained();
    }

    void OnItemGained()
    {
        if(PlayerInventory.GetInstance() != null && this != null) {
            Vector2 currentPosition = startingPosition;
            foreach (Item item in PlayerInventory.GetInstance().GetItems().Keys)
            {
                GameObject entry = Instantiate(UIEntryPrefab, InventoryEntryContainer.transform);
                RectTransform entryTransform = entry.GetComponent<RectTransform>();
                entryTransform.anchoredPosition = currentPosition;
                currentPosition += entryOffset;

                Image shipPart = entry.transform.Find("Image").GetComponent<Image>();
                TMP_Text shipPartName = entry.transform.Find("Text").GetComponent<TMP_Text>();
                // Debug.Log(item.ItemName);

                shipPart.sprite = item.Icon;
                shipPartName.text = item.ItemName;
            }
        }
    }
}
