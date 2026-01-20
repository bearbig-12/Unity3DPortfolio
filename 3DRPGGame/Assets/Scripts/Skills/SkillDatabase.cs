using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDatabase : MonoBehaviour
{

    private Dictionary<string, SkillDefinition> _skills = new Dictionary<string, SkillDefinition>();

    public IReadOnlyDictionary<string, SkillDefinition> Skills => _skills;

    private void Awake()
    {
        LoadFromResources();
    }

    private void LoadFromResources()
    {
        _skills.Clear();
        TextAsset json = Resources.Load<TextAsset>("SkillTree/skills");
        if (json == null)
        {
            Debug.LogWarning("skills.json not found in Resources/SkillTree");
            return;
        }

        SkillTreeDefinition tree = JsonUtility.FromJson<SkillTreeDefinition>(json.text);
        if (tree == null || tree.skills == null)
        {
            Debug.LogWarning("skills.json is invalid");
            return;
        }

        foreach (var skill in tree.skills)
        {
            if (string.IsNullOrEmpty(skill.id)) continue;
            _skills[skill.id] = skill;
        }
    }

    // 딕셔너리 안에서 특정 스킬 찾아오기
    public SkillDefinition Get(string id)
    {
        if(_skills.TryGetValue(id, out var skill))
        {
            return skill;
        }
        return null;
    }
}
