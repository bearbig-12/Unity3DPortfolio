using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTooltipPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text maxRankText;
    [SerializeField] private TMP_Text unlockText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

    private void Awake()
    {
        if (panelRoot == null) panelRoot = transform as RectTransform;
        Hide();
    }

    public void Show(SkillDefinition def, SkillTreeSystem tree, PlayerProgress progress, RectTransform anchor)
    {
        if (def == null || panelRoot == null) return;

        if (nameText != null) nameText.text = def.name;
        if (descText != null) descText.text = def.description;
        if (maxRankText != null) maxRankText.text = "Max Rank : " + def.maxRank;
        if (unlockText != null) unlockText.text = "UnlockLevel : " + def.unlockLevel;
        if (damageText != null) damageText.text = "Damage : " + def.damage;

        panelRoot.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.gameObject.SetActive(false);
    }
}
