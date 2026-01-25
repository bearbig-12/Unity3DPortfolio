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

    public List<InventoryItemStack> inventoryStacks = new List<InventoryItemStack>();
}

[Serializable]
public class InventoryItemStack
{
    public string id;
    public int count;
}
