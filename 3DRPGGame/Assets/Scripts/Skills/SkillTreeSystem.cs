using System.Collections.Generic;
using UnityEngine;


public class SkillTreeSystem : MonoBehaviour
{

    [SerializeField] private SkillDatabase database;
    [SerializeField] private PlayerProgress progress;

    private Dictionary<string, SkillState> _learned = new Dictionary<string, SkillState>();

    public bool CanLearn(string id)
    {
        if (database == null || progress == null) return false;

        SkillDefinition newSkill = database.Get(id);

        if (newSkill == null) return false;

        if (progress.Level < newSkill.unlockLevel) return false;
        if (progress.SkillPoints < newSkill.cost) return false;

        if(!string.IsNullOrEmpty(newSkill.parentId))
        {
            if(!_learned.TryGetValue(newSkill.parentId, out var parentState) || parentState.rank <= 0)
            
            {
                return false;
            }
        }

        if (_learned.TryGetValue(id, out var state))
        {
            return state.rank < newSkill.maxRank;
        }




        return true;
    }

    public bool Learn(string id)
    {
        if (!CanLearn(id)) return false;

        SkillDefinition newSkill = database.Get(id);
        progress.SkillPoints -= newSkill.cost;

        if (_learned.TryGetValue(id, out var state))
        {
            state.rank += 1;
        }
        else
        {
            _learned[id] = new SkillState { id = id, rank = 1 };
        }

        return true;
    }

    public bool IsLearned(string id)
    {
        if (_learned.TryGetValue(id, out var state))
        {
            return state.rank > 0;
        }
        return false;
    }


    public List<SkillState> GetLearnedForSave()
    {
        return new List<SkillState>(_learned.Values);
    }

    public void LoadLearned(List<SkillState> learned)
    {
        _learned.Clear();
        if (learned == null) return;

        foreach (var state in learned)
        {
            if (string.IsNullOrEmpty(state.id)) continue;
            _learned[state.id] = state;
        }
    }
}
