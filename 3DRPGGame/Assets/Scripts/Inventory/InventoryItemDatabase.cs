using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemDatabase : MonoBehaviour
{
    public static InventoryItemDatabase Instance { get; private set; }

    private Dictionary<string, InventoryItemData> _items = new Dictionary<string, InventoryItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadAll();
    }

    private void LoadAll()
    {
        _items.Clear();

        InventoryItemData[] all = Resources.LoadAll<InventoryItemData>("Items");
        foreach(var item in all)
        {
            string id = item.GetStableId();
            if(!string.IsNullOrEmpty(id))
            {
                _items[id] = item;
            }

        }

        Debug.Log("Loaded items: " + string.Join(", ", _items.Keys));
    }
    public InventoryItemData GetById(string id)
    {
        if (_items.TryGetValue(id, out var item))
        {
            return item;
        }
        return null;
    }

}
