using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBuyItemUI : MonoBehaviour
{
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyBtn;


    private void Awake()
    {
        if (buyBtn != null)
        {
            buyBtn.onClick.AddListener(Buy);
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (buyBtn != null)
        {
            buyBtn.onClick.RemoveListener(Buy);
        }
    }

    private void RefreshUI()
    {
        if (itemData == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.icon;
        }

        if (nameText != null)
        {
            nameText.text = itemData.itemName;
        }

        if (priceText != null)
        {
            priceText.text = itemData.buyPrice.ToString();
        }
    }

    private void Buy()
    {
        if (itemData == null || itemIconPrefab == null)
        {
            return;
        }

        if (GoldManager.Instance == null)
        {
            return;
        }

        if (!GoldManager.Instance.Spend(itemData.buyPrice))
        {
            return;
        }

        if (InventorySystem.instance == null)
        {
            GoldManager.Instance.Add(itemData.buyPrice);
            return;
        }

        bool added = InventorySystem.instance.AddItem(itemIconPrefab, itemData);
        if (!added)
        {
            GoldManager.Instance.Add(itemData.buyPrice);
        }
    }
}
