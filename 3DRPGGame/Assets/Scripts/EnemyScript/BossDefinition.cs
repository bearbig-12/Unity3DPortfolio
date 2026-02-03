using UnityEngine;

[CreateAssetMenu(fileName = "NewBossDefinition", menuName = "Monster/BossDefinition")]
public class BossDefinition : MonsterDefinition
{
    [Header("Boss Phase")]
    public float phase2HealthRatio = 0.5f;

    [Header("Boss Attack Ranges")]
    public float meleeRange = 2.5f;
    public float rangedRange = 10f;
    public float aoeRange = 6f;

    [Header("Boss Cooldowns")]
    public float meleeCooldown = 2f;
    public float rangedCooldown = 3f;
    public float aoeCooldown = 6f;

    [Header("Boss Damage")]
    public int meleeDamage = 10;
    public int rangedDamage = 8;
    public int aoeDamage = 5;

    [Header("Boss Animator Triggers")]
    public string meleeTrigger = "MeleeAttack";
    public string rangedTrigger = "RangedAttack";
    public string aoeTrigger = "AOEAttack";
    public string phase2Trigger = "Phase2";
}
