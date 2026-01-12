using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ShopSellItemUI : MonoBehaviour
{
    [SerializeField] private Transform sellSlotRoot;
    [SerializeField] private TextMeshProUGUI totalPriceText;
    [SerializeField] private Button sellBtn;

    private int cachedTotal = -1;

    private void OnEnable()
    {
        if (sellBtn != null)
        {
            sellBtn.onClick.AddListener(SellAll);
        }

        RefreshTotal();
    }

    private void OnDisable()
    {
        if (sellBtn != null)
        {
            sellBtn.onClick.RemoveListener(SellAll);
        }
    }

    private void Update()
    {
        int total = ComputeTotal();
        if (total != cachedTotal)
        {
            UpdateTotal(total);
        }
    }

    private int ComputeTotal()
    {
        Transform root = sellSlotRoot != null ? sellSlotRoot : transform;
        int total = 0;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform slot = root.GetChild(i);
            InventoryItemUI item = slot.GetComponentInChildren<InventoryItemUI>();
            if (item == null || item.ItemData == null)
            {
                continue;
            }

            total += item.ItemData.sellPrice;
        }

        return total;
    }

    private void UpdateTotal(int total)
    {
        cachedTotal = total;
        if (totalPriceText != null)
        {
            totalPriceText.text = total.ToString();
        }
    }

    private void RefreshTotal()
    {
        UpdateTotal(ComputeTotal());
    }

    private void SellAll()
    {
        int total = 0;
        Transform root = sellSlotRoot != null ? sellSlotRoot : transform;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform slot = root.GetChild(i);
            InventoryItemUI item = slot.GetComponentInChildren<InventoryItemUI>();
            if (item == null || item.ItemData == null)
            {
                continue;
            }

            total += item.ItemData.sellPrice;
            Destroy(item.gameObject);
        }

        if (total > 0 && GoldManager.Instance != null)
        {
            GoldManager.Instance.Add(total);
        }

        RefreshTotal();
    }
}
