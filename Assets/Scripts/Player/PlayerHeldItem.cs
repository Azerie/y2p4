using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHeldItem : MonoBehaviour
{
    [SerializeField] private Item inventoryItem;
    [SerializeField] private GameObject heldItem;
    void Awake()
    {
        PlayerInventory.SelectedItemChanged += UpdateHeldItem;
    }

    void UpdateHeldItem()
    {
        if (PlayerInventory.GetInstance().GetSelectedItem() == inventoryItem)
        {
            heldItem.SetActive(true);
        }
        else
        {
            heldItem.SetActive(false);
        }
    }
}
