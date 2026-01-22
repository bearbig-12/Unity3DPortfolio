using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int playerLevel;
    public int skillPoints;
    public int gold;
    public int currentExp;
    public List<SkillState> learnedSkills = new List<SkillState>();
    public List<string> inventoryItemIds = new List<string>();
}