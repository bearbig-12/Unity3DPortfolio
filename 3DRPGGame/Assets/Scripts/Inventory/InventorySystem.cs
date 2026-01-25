using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance { get; private set; }
    public CameraMovement cameraMovement;

    public GameObject inventorySysyemUI;
    [SerializeField] private List<ItemSlot> _slots = new List<ItemSlot>();
    [SerializeField] private Canvas _inventoryCanvas;
    [SerializeField] private Transform weaponEquipRoot; // 플레이어가 무기를 장착 위치


    [SerializeField]  private GameObject itemIconPrefab; // 인벤 아이콘 프리팹

    // 플레이어 기본 무기
    [SerializeField] private InventoryItemData starterWeaponData; 
    [SerializeField] private GameObject starterWeaponInstance;  

    private PlayerMovement _player;

    private InventoryItemData _equippedWeaponData;
    private GameObject _equippedWeaponInstance;

    private bool isOpen = false;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isOpen = false;
        _player = FindObjectOfType<PlayerMovement>();

        if (_equippedWeaponInstance == null && starterWeaponInstance != null)
        {
            _equippedWeaponInstance = starterWeaponInstance;
            _equippedWeaponData = starterWeaponData;

            if (_player != null)
            {
                _player.SetWeaponHitPoints(_equippedWeaponInstance.transform);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetInventoryOpen(!isOpen, true);
        }
    }

    private void CacheSlots()
    {
        if (_slots == null)
        {
            _slots = new List<ItemSlot>();
        }
    }

    public void SetInventoryOpen(bool open, bool controlCursor)
    {
        if (isOpen == open)
        {
            return;
        }

        isOpen = open;
        inventorySysyemUI.SetActive(isOpen);

        if (cameraMovement != null)
        {
            cameraMovement.enabled = !isOpen;
        }

        if (controlCursor)
        {
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }
    }

    public bool AddItem(GameObject itemPrefab, InventoryItemData data)
    {
        Debug.Log("AddItem called. slots=" + (_slots != null ? _slots.Count : -1));
        if (itemPrefab == null || data == null)
        {
            Debug.LogWarning("Inv Item prefab or data is NULL");
            return false;
        }

        if (_slots == null || _slots.Count == 0)
        {
            CacheSlots();
        }


        // stack consumables
        if (data.itemType == InventoryItemType.Consumable)
        {
            foreach (ItemSlot slot in _slots)
            {
                if (slot == null || slot.Item == null) continue;

                InventoryItemUI ui = slot.Item.GetComponent<InventoryItemUI>();
                if (ui != null && ui.ItemData != null &&
                    ui.ItemData.GetStableId() == data.GetStableId())
                {
                    ui.AddCount(1);
                    return true;
                }
            }
        }

        foreach (ItemSlot slot in _slots)
        {
            if (slot == null || slot.Item != null)
            {
                continue;
            }

            GameObject instance = Instantiate(itemPrefab, slot.transform);
            RectTransform rect = instance.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localPosition = Vector3.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            InventoryItemUI itemUI = instance.GetComponent<InventoryItemUI>();

            if (itemUI != null)
            {
                itemUI.Init(data);
            }

            DragDrop dragDrop = instance.GetComponent<DragDrop>();

            if (dragDrop != null)
            {
                if (_inventoryCanvas == null)
                {
                    Debug.LogWarning("Inventory Canvas is not assigned.");
                }
                else
                {
                    dragDrop.SetCanvas(_inventoryCanvas);
                }
            }

            return true;
        }

        Debug.LogWarning("Inv no empty slot available.");
        return false;
    }

    public void UseItem(InventoryItemUI itemUI)
    {
        if (itemUI == null || itemUI.ItemData == null) return;

        InventoryItemData data = itemUI.ItemData;

        if (data.itemType == InventoryItemType.Consumable)
        {
            if (_player != null)
            {
                if (data.consumableType == ConsumableType.HP)
                {
                    _player.Heal(HealType.HP, data.healAmount);
                }
                else if (data.consumableType == ConsumableType.Stamina)
                {
                    _player.Heal(HealType.Stamina, data.healAmount);
                }
            }

            // stack 감소
            bool hasRemaining = itemUI.ConsumeItem();
            if (!hasRemaining) Destroy(itemUI.gameObject);
        }
        else if (data.itemType == InventoryItemType.Weapon)
        {
            EquipWeaponFromItemUI(itemUI);
        }
    }

    public bool TryConsumeForQuickSlot(string itemId)
    {
        if (_slots == null) return false;

        foreach (var slot in _slots)
        {
            if (slot == null || slot.Item == null) continue;
            InventoryItemUI ui = slot.Item.GetComponent<InventoryItemUI>();
            if (ui != null && ui.ItemData != null)
            {
                if (ui.ItemData.itemType == InventoryItemType.Consumable &&
                    ui.ItemData.GetStableId() == itemId)
                {
                    UseItem(ui);
                    return true;
                }
            }
        }

        return false;
    }

    public void DropItem(InventoryItemUI itemUI)
    {
        if (itemUI == null) return;
        Destroy(itemUI.gameObject);
    }

    private void EquipWeaponFromItemUI(InventoryItemUI itemUI)
    {
        if (weaponEquipRoot == null) return;

        InventoryItemData newData = itemUI.ItemData;
        Transform slotTransform = itemUI.transform.parent;

        InventoryItemData oldData = _equippedWeaponData;

        EquipWeaponPrefab(newData.weaponPrefab);
        _equippedWeaponData = newData;

        Destroy(itemUI.gameObject);

        if (oldData != null && slotTransform != null)
        {
            CreateIconInSlot(oldData, slotTransform);
        }
    }

    private void EquipWeaponPrefab(GameObject weaponPrefab)
    {
        if (weaponPrefab == null) return;

        if (_equippedWeaponInstance != null)
        {
            Destroy(_equippedWeaponInstance);
        }

        _equippedWeaponInstance = Instantiate(
            weaponPrefab,
            weaponEquipRoot.position,
            weaponEquipRoot.rotation,
            weaponEquipRoot
        );

        _equippedWeaponInstance.transform.localPosition = Vector3.zero;
        _equippedWeaponInstance.transform.localScale = Vector3.one;


        if (_player != null)
        {
            _player.SetWeaponHitPoints(_equippedWeaponInstance.transform);
        }
    }

    private void CreateIconInSlot(InventoryItemData data, Transform slotTransform)
    {
        if (itemIconPrefab == null) return;

        GameObject icon = Instantiate(itemIconPrefab, slotTransform);
        ApplyRectDefaults(icon);

        InventoryItemUI itemUI = icon.GetComponent<InventoryItemUI>();
        if (itemUI != null) itemUI.Init(data);

        DragDrop dragDrop = icon.GetComponent<DragDrop>();
        if (dragDrop != null && _inventoryCanvas != null)
        {
            dragDrop.SetCanvas(_inventoryCanvas);
        }
    }

    private void ApplyRectDefaults(GameObject instance)
    {
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        else
        {
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }
    }

    public List<InventoryItemStack> GetSavedStacks()
    {
        List<InventoryItemStack> stacks = new List<InventoryItemStack>();

        foreach(var slot in _slots)
        {
            if (slot == null || slot.Item == null) continue;

            InventoryItemUI ui = slot.Item.GetComponent<InventoryItemUI>();
            if (ui != null && ui.ItemData != null)
            {
                stacks.Add(new InventoryItemStack {id = ui.ItemData.GetStableId(), count = ui.Count });
            }

        }

        return stacks;
        
    }

    public void LoadFromSavedStacks(List<InventoryItemStack> stacks)
    {
        if (stacks == null) return;
        if (itemIconPrefab == null)
        {
            Debug.LogWarning("itemIconPrefab is not assigned.");
            return;
        }

        foreach (var s in stacks)
        {
            if (s == null || string.IsNullOrEmpty(s.id)) continue;
            InventoryItemData data = InventoryItemDatabase.Instance.GetById(s.id);

            int count = s.count;

            for (int i = 0; i < count; ++i)
            {
                AddItem(itemIconPrefab, data);
            }

        }

    }





    public void ClearInventory()
    {
        if (_slots == null) return;

        foreach (var slot in _slots)
        {
            if (slot == null || slot.Item == null) continue;
            Destroy(slot.Item);
        }
    }

    //public List<string> GetSavedItmeIDs()
    //{
    //    List<string> ids = new List<string>();

    //    foreach (var slot in _slots)
    //    {
    //        if (slot == null || slot.Item == null) continue;
    //        InventoryItemUI ui = slot.Item.GetComponent<InventoryItemUI>();
    //        if(ui != null && ui.ItemData != null)
    //        {
    //            ids.Add(ui.ItemData.GetStableId());
    //        }
    //    }

    //    return ids;
    //}

    //public void LoadFromSavedIds(List<string> ids)
    //{
    //    if (ids == null) return;
    //    if(itemIconPrefab == null)
    //    {
    //        Debug.LogWarning("itemIconPrefab is not assigned.");
    //        return;
    //    }


    //    foreach(var id in ids)
    //    {
    //        InventoryItemData data = InventoryItemDatabase.Instance.GetById(id);
    //        if (data != null)
    //        {
    //            AddItem(itemIconPrefab, data);
    //        }
    //    }
    //}


}