using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    public InventoryItemData ItemData { get; private set; }
    public int Count { get; private set; } = 1;

    private void Awake()
    {
        if (iconImage == null) iconImage = GetComponent<Image>();
        if (countText == null) countText = GetComponentInChildren<TMP_Text>(true);
        UpdateCountText();
    }

    public void Init(InventoryItemData data, int count = 1)
    {
        ItemData = data;
        Count = Mathf.Max(1, count);
        if (iconImage != null && data != null)
        {
            iconImage.sprite = data.icon;
        }
        UpdateCountText();
    }
    public void AddCount(int amount)
    {
        Count = Mathf.Max(1, Count + amount);
        UpdateCountText();
    }

    public bool ConsumeItem()
    {
        if (Count <= 0) return false;
        Count -= 1;
        UpdateCountText();
        return Count > 0;
    }

    private void UpdateCountText()
    {
        if (countText == null) return;
        countText.text = Count > 1 ? Count.ToString() : "";
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
