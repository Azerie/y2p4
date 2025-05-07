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

                Image itemImage = entry.transform.Find("Image").GetComponent<Image>();
                TMP_Text itemName = entry.transform.Find("Text").GetComponent<TMP_Text>();
                // TMP_Text itemNumber = entry.transform.Find("Counter").GetComponent<TMP_Text>();
                // Debug.Log(item.ItemName);

                itemImage.sprite = item.Icon;
                itemName.text = item.ItemName;
            }
        }
    }
}
