using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    public InventoryItemData ItemData { get; private set; }

    private void Awake()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
    }

    public void Init(InventoryItemData data)
    {
        ItemData = data;
        if (iconImage != null && data != null)
        {
            iconImage.sprite = data.icon;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ItemData == null)
        {
            return;
        }

        InventoryContextMenu.Instance.Show(this, eventData.position);
    }
}
