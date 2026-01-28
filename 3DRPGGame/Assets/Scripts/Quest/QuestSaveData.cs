using System;

[Serializable]
public class QuestSaveData
{
    public string questId;
    public QuestStatus status;
    public int currentObjectiveIndex;
    public int currentCount;
}