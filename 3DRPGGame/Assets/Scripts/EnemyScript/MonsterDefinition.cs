using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterDefinition", menuName = "Monster/MonsterDefinition")]
public class MonsterDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterId;
    public string monsterName;

    [Header("HP")]
    public int maxHealth = 100;

    [Header("Combat")]
    public int basicAttackDamage = 5;
    public int hardAttackDamage = 10;
    public float attackCooldown = 1f;
    public float attackRange = 2f;

    [Header("Detection Range")]
    public float alertRange = 15f;
    public float fovRange = 8f;
    public float fovAngle = 90f;
    public float returnRange = 30f;

    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 6f;

    [Header("Rewards")]
    public int expReward = 30;

    [Header("Animation Triggers")]
    public string basicAttackTrigger = "BasicAttack";
    public string hardAttackTrigger = "HardAttack";

    [Header("UI")]
    public Vector3 healthBarOffset = new Vector3(0, 2.5f, 0);
}
