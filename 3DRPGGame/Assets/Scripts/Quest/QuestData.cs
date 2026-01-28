using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestObjectiveType
{
    Kill,
    RequiredItem
}

[Serializable]
public class QuestObjective
{
    public QuestObjectiveType type;

    // Kill
    public string targetEnemyId = "monster";
    public int requiredCount = 1;

    // TurnInItem
    public string requiredItemId = "";
}

[CreateAssetMenu(menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string title;
    [TextArea] public string description;

    [Header("Prerequisite")]
    public string prerequisiteQuestId; // ¼±Çà Äù½ºÆ® (¾øÀ¸¸é ºóÄ­)

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards")]
    public int rewardExp = 0;
    public int rewardGold = 0;
    public InventoryItemData rewardItem;
}