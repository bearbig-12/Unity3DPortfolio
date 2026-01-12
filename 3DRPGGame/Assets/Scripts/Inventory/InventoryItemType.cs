using UnityEngine;

public enum InventoryItemType
{
    Weapon,
    Consumable
}

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public InventoryItemType itemType;

    [Header("Price")]
    public int buyPrice = 0;
    public int sellPrice = 0;

    [Header("Consumable")]
    public int healAmount = 50;

    [Header("Weapon")]
    public GameObject weaponPrefab;
}
