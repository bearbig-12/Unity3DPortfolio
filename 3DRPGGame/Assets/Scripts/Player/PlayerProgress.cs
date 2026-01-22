using TMPro;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public int Level = 1;
    public int SkillPoints = 1;
    public int CurrentExp = 0;


    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private int level1To2Exp = 50;
    [SerializeField] private int level2To3Exp = 100;
    [SerializeField] private int expPerKill = 30;

    [Header("Level Up Bonuses")]
    [SerializeField] private int attackPerLevel = 5;
    [SerializeField] private int healthPerLevel = 10;
    [SerializeField] private int staminaPerLevel = 10;

    [SerializeField] private PlayerMovement player;

    [SerializeField] private ExpBar expBar;
    [SerializeField] private TMP_Text levelText;

    [SerializeField] private SkillUIController skillUI;

    private void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerMovement>();
        ApplyLevelFromProgress();

        if (expBar == null) expBar = FindObjectOfType<ExpBar>();
        if (levelText == null) levelText = FindObjectOfType<TMP_Text>();

        if (skillUI == null) skillUI = FindObjectOfType<SkillUIController>();

        UpdateExpUI();
    }
    public void AddSkillPoints(int amount)
    {
        SkillPoints += amount;
        if (SkillPoints < 0) SkillPoints = 0;
    }

    public void AddEXP(int amount)
    {
        if (Level >= maxLevel) return;
        CurrentExp += amount;
        while(Level < maxLevel)
        {
            int required = GetExpToNextLevel();
            if (required <= 0 || CurrentExp < required) break;

            CurrentExp -= required;
            Level += 1;
            LevelUp();
        }

        if(Level >= maxLevel)
        {
            CurrentExp = 0;
        }

        UpdateExpUI();
    }

    public int GetExpToNextLevel()
    {
        if (Level == 1) return level1To2Exp;
        if (Level == 2) return level2To3Exp;
        return 0;
    }

    public void ApplyLoadedData(int level, int exp)
    {
        Level = Mathf.Clamp(level, 1, maxLevel);
        CurrentExp = exp;

        if (Level >= maxLevel) CurrentExp = 0;

        ApplyLevelFromProgress();
        UpdateExpUI();
    }

    public int GetExpPerKill()
    {
        return expPerKill;
    }

    private void LevelUp()
    {
        SkillPoints += 1;
        if (player != null)
        {
            player.ApplyLevelUpBonus(healthPerLevel, staminaPerLevel, attackPerLevel);
        }

        if (skillUI != null)
        {
            skillUI.RefreshAllNodes();
        }
        UpdateExpUI();
    }

    private void ApplyLevelFromProgress()
    {
        if (player != null)
        {
            player.ApplyLevelFromProgress(Level, healthPerLevel, staminaPerLevel, attackPerLevel);
        }
    }

    private void UpdateExpUI()
    {
        int required = GetExpToNextLevel();
        if (required <= 0)
        {
            if (expBar != null) expBar.SetMaxExp(1);
            if (expBar != null) expBar.SetExp(1);
        }
        else
        {
            if (expBar != null) expBar.SetMaxExp(required);
            if (expBar != null) expBar.SetExp(CurrentExp);
        }

        if (levelText != null) levelText.text = Level.ToString();
    }

}