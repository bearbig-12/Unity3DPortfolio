using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance { get; private set; }
    public CameraMovement cameraMovement;

    public GameObject inventorySysyemUI;
    [SerializeField] private List<ItemSlot> _slots = new List<ItemSlot>();
    [SerializeField] private Canvas _inventoryCanvas;


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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            cameraMovement.enabled = false;

            isOpen = !isOpen;
            inventorySysyemUI.SetActive(isOpen);
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            cameraMovement.enabled = true;


            isOpen = !isOpen;
            inventorySysyemUI.SetActive(isOpen);
        }
    }


    private void CacheSlots()
    {
        if (_slots == null)
        {
            _slots = new List<ItemSlot>();
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        if(itemPrefab == null)
        {
            Debug.LogWarning("Ivn Item prefab is NULL");
            return false;
        }

        if(_slots == null || _slots.Count == 0)
        {
            CacheSlots();
        }

        foreach (ItemSlot slot in _slots)
        {
            if (slot == null || slot.Item != null)
            {
                continue;
            }

            // 빈 슬롯에 아이템 아이콘 만들기
            GameObject instance = Instantiate(itemPrefab, slot.transform); 
            RectTransform rect = instance.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localPosition = Vector3.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = instance.transform.localScale;
            }
          

            DragDrop dragDrop = instance.GetComponent<DragDrop>();
            if(dragDrop != null)
            {
                if(_inventoryCanvas == null)
                {
                    if(inventorySysyemUI != null)
                    {
                        _inventoryCanvas = inventorySysyemUI.GetComponent<Canvas>();
                    }
                    if(_inventoryCanvas == null)
                    {
                        _inventoryCanvas = FindObjectOfType<Canvas>();
                    }
                }
                dragDrop.SetCanvas(_inventoryCanvas);
            }
            return true;
        }
        Debug.LogWarning("Ivn No empty slot available");
        return false;
    }
 
}
