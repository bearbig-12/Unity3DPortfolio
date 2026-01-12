using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    // ╫л╠шео
    public static GoldManager Instance { get; private set; }

    [SerializeField] private int startingGold = 100;
    [SerializeField] private TextMeshProUGUI goldText;

    public int Gold { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Gold = startingGold;
        UpdateUI();
    }

    public bool CanAfford(int amount)
    {
        return amount <= Gold;
    }

    public bool Spend(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!CanAfford(amount))
        {
            return false;
        }

        Gold -= amount;
        UpdateUI();
        return true;
    }
    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        UpdateUI();
    }

    public void SetGoldText(TextMeshProUGUI text)
    {
        goldText = text;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = Gold.ToString();
        }
    }
}
