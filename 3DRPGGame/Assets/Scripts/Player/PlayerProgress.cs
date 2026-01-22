using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public int Level = 1;
    public int SkillPoints = 1;

    public void AddSkillPoints(int amount)
    {
        SkillPoints += amount;
        if (SkillPoints < 0) SkillPoints = 0;
    }
}