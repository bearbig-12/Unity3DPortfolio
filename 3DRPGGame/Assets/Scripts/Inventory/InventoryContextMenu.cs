using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryContextMenu : MonoBehaviour
{
    public static InventoryContextMenu Instance { get; private set; }

    [SerializeField] private Image panelRoot;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private GraphicRaycaster _raycaster;

    private InventoryItemUI _currentItem;

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

        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }

        if (_raycaster == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null) _raycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        useButton.onClick.AddListener(OnUse);
        dropButton.onClick.AddListener(OnDrop);
    }

    private void Update()
    {
        if (panelRoot == null || !panelRoot.gameObject.activeSelf) return;
        if (!Input.GetMouseButtonDown(0)) return;

        if (!IsPointerOverContextOrItem())
        {
            Hide();
        }
    }

    public void Show(InventoryItemUI itemUI, Vector2 screenPos)
    {
        _currentItem = itemUI;

        if(panelRoot != null)
        {
            panelRoot.transform.position = screenPos;
            panelRoot.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }
        _currentItem = null;
    }

    private void OnUse()
    {
        if (_currentItem == null) return;
        InventorySystem.instance.UseItem(_currentItem);
        Hide();
    }

    private void OnDrop()
    {
        if (_currentItem == null) return;
        InventorySystem.instance.DropItem(_currentItem);
        Hide();
    }

    private bool IsPointerOverContextOrItem()
    {
        if (_raycaster == null || EventSystem.current == null) return false;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        _raycaster.Raycast(data, results);

        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (panelRoot != null && go.transform.IsChildOf(panelRoot.transform)) return true;
            if (go.GetComponentInParent<InventoryItemUI>() != null) return true;
        }

        return false;
    }
}
