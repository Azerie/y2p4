using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // [SerializeField] GameObject UIEntryPrefab;
    [SerializeField] GameObject InventoryEntryContainer;

    // [Header("Layout Settings")]
    // [SerializeField] private Vector2 entryOffset = new Vector2(0, -50);
    // [SerializeField] private Vector2 startingPosition = Vector2.zero;

    [SerializeField] private float showTime = 5f;
    private float timer = 0;
    private bool isHidden = false;

    private void Awake()
    {
        PlayerInventory.InventoryChanged += DrawUI;
        PlayerInventory.SelectedItemChanged += DrawUI;
        DrawUI();
    }

    private void Update()
    {
        if (!isHidden)
        {
            timer += Time.deltaTime;
            if (timer >= showTime)
            {
                HideUI();
            }
        }
    }

    void HideUI()
    {
        foreach (Transform entry in InventoryEntryContainer.transform)
        {
            entry.gameObject.SetActive(false);
        }
        isHidden = true;
    }


    void DrawUI()
    {
        if (PlayerInventory.GetInstance() != null && this != null)
        {
            List<Item> items = PlayerInventory.GetInstance().GetItems().Keys.ToList();
            int startId = PlayerInventory.GetInstance().GetSelectedItemId();
            List<Item> itemsReordered = new List<Item>();
            for (int i = 0; i < items.Count; i++)
            {
                itemsReordered.Add(items[(i + startId) % items.Count]);
            }
            int counter = 0;
            foreach (Transform entry in InventoryEntryContainer.transform)
            {
                if (counter < itemsReordered.Count)
                {
                    entry.gameObject.SetActive(true);
                    DrawInventoryItem(itemsReordered[counter], entry);
                    counter++;
                }
                else
                {
                    entry.gameObject.SetActive(false);
                }
            }
        }
        timer = 0;
        isHidden = false;
    }

    void DrawInventoryItem(Item item, Transform entry)
    {
        Image itemImage = entry.Find("Image").GetComponent<Image>();
        TMP_Text itemName = entry.Find("Text").GetComponent<TMP_Text>();
        // TMP_Text itemNumber = entry.Find("Counter").GetComponent<TMP_Text>();
        // Debug.Log(item.ItemName);

        itemImage.sprite = item.Icon;
        itemName.text = item.ItemName;
    }
}
