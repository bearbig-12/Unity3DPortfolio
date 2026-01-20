using System;
using System.Collections.Generic;

[Serializable]
public class SkillDefinition
{
    public string id;
    public string name;
    public string parentId;
    public int unlockLevel;
    public int cost;
    public int maxRank = 1;
    public string description;
}

[Serializable]
public class SkillTreeDefinition
{
    public List<SkillDefinition> skills = new List<SkillDefinition>();
}

[Serializable]
public class SkillState
{
    public string id;
    public int rank;
}