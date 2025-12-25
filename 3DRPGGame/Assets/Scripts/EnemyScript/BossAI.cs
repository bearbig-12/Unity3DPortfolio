using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : EnemyAI
{
    public enum BossPhase
    {
        Phase1,
        Phase2
    }

    public enum BossAttackType
    {
        Melee,
        Ranged,
        Aoe
    }

    [Header("Boss Phase")]
    public float phase2HealthRatio = 0.5f;
    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    [Header("Boss Attack Ranges")]
    public float meleeRange = 2.5f;
    public float rangedRange = 10f;
    public float aoeRange = 6f;

    [Header("Boss Cooldowns")]
    public float meleeCooldown = 2f;
    public float rangedCooldown = 4f;
    public float aoeCooldown = 6f;

    [Header("Boss Damage")]
    public int meleeDamage = 10;
    public int rangedDamage = 8;
    public int aoeDamage = 15;

    [Header("Boss Animator Triggers")]
    public string meleeTrigger = "BasicAttack";
    public string rangedTrigger = "RangedAttack";
    public string aoeTrigger = "AOEAttack";
    public string phase2Trigger = "Phase2";

    [Header("Boss Hitboxes")]
    public EnemyDamageHitbox[] meleeHitboxes;
    public EnemyDamageHitbox[] rangedHitboxes;
    public EnemyDamageHitbox[] aoeHitboxes;
    public BossAttackType CurrentAttackType { get; private set; } = BossAttackType.Melee;

    private float _nextMeleeTime;
    private float _nextRangedTime;
    private float _nextAoeTime;
    protected override void InitializeStates()
    {
        base.InitializeStates();
        AttackState = new BossAttackState(this);
    }
    protected override void OnDamaged(int damage)
    {
        if (CurrentPhase == BossPhase.Phase1 && currentHealth <= maxHealth * phase2HealthRatio)
        {
            CurrentPhase = BossPhase.Phase2;
            OnPhaseChanged();
        }
    }

    void OnPhaseChanged()
    {
        if (!string.IsNullOrEmpty(phase2Trigger))
        {
            _animator.SetTrigger(phase2Trigger);
        }
    }

    public bool IsAttackReady(BossAttackType type)
    {
        if (type == BossAttackType.Melee)
        {
            return Time.time >= _nextMeleeTime;
        }
        if (type == BossAttackType.Ranged)
        {
            return Time.time >= _nextRangedTime;
        } 
        return Time.time >= _nextAoeTime;
    }

    public void SetAttackType(BossAttackType type)
    {
        CurrentAttackType = type;
    }

    public override void AnimEvent_AttackStart()
    {
        SetHitboxes(true);
    }

    public override void AnimEvent_AttackEnd()
    {
        SetHitboxes(false);
    }
    private void SetHitboxes(bool active)
    {
        EnemyDamageHitbox[] set = null;
        if (CurrentAttackType == BossAttackType.Melee)
        {
            set = meleeHitboxes;
        }
        else if (CurrentAttackType == BossAttackType.Ranged)
        {
             set = rangedHitboxes;
        }
        else
        {
            set = aoeHitboxes;

        } 

        if (set == null) return;

        for (int i = 0; i < set.Length; i++)
        {
            if (set[i] != null)
            {
                set[i].SetActive(active);
            }
        }
    }

    public void ConsumeAttackCooldown(BossAttackType type)
    {
        if (type == BossAttackType.Melee)
        {
            _nextMeleeTime = Time.time + meleeCooldown;
        }
        else if (type == BossAttackType.Ranged)
        {
            _nextRangedTime = Time.time + rangedCooldown;
        }
        else 
        {
            _nextAoeTime = Time.time + aoeCooldown;
        }
    }
}
