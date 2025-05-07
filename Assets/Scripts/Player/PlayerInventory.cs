using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Dictionary<Item, int> items = new Dictionary<Item, int>();
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
    public Dictionary<Item, int> GetItems() { return items; }
    public bool HasItem(Item item)
    {
        return Instance.items.ContainsKey(item);
    }

    public void AddItem(Item item)
    {
        if (Instance.items.ContainsKey(item))
        {
            Instance.items[item]++;
        }
        else
        {
            Instance.items.Add(item, 1);
        }
        InventoryChanged?.Invoke();
    }

    public bool RemoveItem(Item item)
    {
        if (Instance.items.ContainsKey(item))
        {
            Instance.items[item]--;
            if(Instance.items[item] == 0) {
                Instance.items.Remove(item);
            }
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
