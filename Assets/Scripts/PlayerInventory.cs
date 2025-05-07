using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<Item> items = new List<Item>();
    public static UnityAction InventoryChanged;
    public static PlayerInventory Instance;

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

    private void OnEnable() {
        InventoryChanged?.Invoke();
    }

    public static PlayerInventory GetInstance()
    {
        return Instance;
    }
    public List<Item> GetItems() { return items; }
    public bool HasItem(Item item)
    {
        var entry = Instance.items.Find(i => i == item);
        return entry != null;
    }

    public void AddItem(Item item)
    {
        var entry = Instance.items.Find(i => i == item);
        if (entry == null)
        {
            Instance.items.Add(item);
            InventoryChanged?.Invoke();
        }
    }

    public bool RemoveItem(Item item)
    {
        
        var entry = Instance.items.Find(i => i == item);
        if (entry != null)
        {
            Instance.items.Remove(entry);
            InventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
    public void ClearAllItems()
    {
        Instance.items.Clear();
        InventoryChanged?.Invoke();
    }
}
