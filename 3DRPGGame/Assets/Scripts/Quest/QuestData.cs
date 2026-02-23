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
    public string prerequisiteQuestId; // 선행 퀘스트 ID (비어있으면 조건 없음)

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards")]
    public int rewardExp = 0;
    public int rewardGold = 0;
    public InventoryItemData rewardItem;
}