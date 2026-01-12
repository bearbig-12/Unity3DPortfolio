using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryContextMenu : MonoBehaviour
{
    public static InventoryContextMenu Instance { get; private set; }

    [SerializeField] private Image panelRoot;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;

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

        useButton.onClick.AddListener(OnUse);
        dropButton.onClick.AddListener(OnDrop);
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
}
