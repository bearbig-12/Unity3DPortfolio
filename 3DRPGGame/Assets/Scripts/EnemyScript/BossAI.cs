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

    [Header("Boss Hitboxes")]
    public EnemyDamageHitbox[] meleeHitboxes;
    public BossAoeHitbox aoeHitbox;

    public BossAttackType CurrentAttackType { get; private set; } = BossAttackType.Melee;


    [Header("Particle Effects")]
    public GameObject fireHandEffect;
    ParticleSystem _phaseChangeEffect;
    ParticleSystem _rangedFireHandEffect;

    [Header("Boss Ranged Attack")]
    public GameObject fireBallPrefab;
    public Transform rightHandPos;
    public float fireBallLifeTime = 5f;
    public float aimHeightOffset = 1.0f; // 조준 높이

    private float _nextMeleeTime;
    private float _nextRangedTime;
    private float _nextAoeTime;
    protected override void InitializeStates()
    {
        base.InitializeStates();
        AttackState = new BossAttackState(this);
        _rangedFireHandEffect = fireHandEffect.GetComponent<ParticleSystem>();
        _rangedFireHandEffect.Stop();

    }
    protected override void OnDamaged(int damage)
    {
        if (CurrentPhase == BossPhase.Phase1 && currentHealth <= 50f)
        {
            Debug.Log("Boss Phase Changed to Phase 2");
            CurrentPhase = BossPhase.Phase2;
            OnPhaseChanged();
        }
    }

    public override float GetEngageRange()
    {
        return rangedRange;
    }

    public void OnPhaseChanged()
    {
        if (!string.IsNullOrEmpty(phase2Trigger))
        {
            //_animator.SetTrigger(phase2Trigger);
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
        SetAttackType(BossAttackType.Melee);
        SetHitboxes(true);
    }
    public override void AnimEvent_AttackEnd()
    {
        SetHitboxes(false);
    }

    public void AnimEvent_RangedAttackStart()
    {
        SetAttackType(BossAttackType.Ranged);
        if (_rangedFireHandEffect != null)
        {
            _rangedFireHandEffect.Play();
        }
    }

    public void AnimEvent_RangedAttackStop()
    {
        if (_rangedFireHandEffect != null)
        {
            // 새로운 파티클 효과는 나오지 않고 기존 파티클 효과는 자연스럽게 사라지도록
            _rangedFireHandEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void AnimEvent_ThrowFireBall()
    {
        if (fireBallPrefab != null && rightHandPos != null && _player != null)
        {
            Vector3 spawnPos = rightHandPos.position;
            Vector3 targetPos = _player.position;
            targetPos.y = _player.position.y + 1.0f; 

            GameObject obj = Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);

            Collider FireBallColl = obj.GetComponent<Collider>();
            Collider BossColl = GetComponent<Collider>();

            // 보스 콜라이더에 닿아서 터지는 거 방지
            if (FireBallColl != null && BossColl != null)
            {
                Physics.IgnoreCollision(FireBallColl, BossColl);
            }

            FireBall fireBall = obj.GetComponent<FireBall>();
            if (fireBall != null)
            {
                fireBall.Launch(targetPos, fireBallLifeTime, rangedDamage);
            }

        }
    }

    public void AnimEvent_AoeStart()
    {
        SetAttackType(BossAttackType.Aoe);
        SetHitbox(aoeHitbox, true);
    }

    public void AnimEvent_AoeEnd()
    {
        SetHitbox(aoeHitbox, false);
    }

    private void SetHitboxes(bool active)
    {
        EnemyDamageHitbox[] set = null;
        if (CurrentAttackType == BossAttackType.Melee)
        {
            SetHitboxes(meleeHitboxes, active);
        }
       
        else
        {
            SetHitbox(aoeHitbox, active);
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

    private void SetHitboxes(EnemyDamageHitbox[] hitBoxes, bool active)
    {
        if (hitBoxes == null) return;
        for (int i = 0; i < hitBoxes.Length; i++)
        {
            if (hitBoxes[i] != null)
            {
                hitBoxes[i].SetActive(active);
            }
        }
    }

    private void SetHitbox(BossAoeHitbox aoeHitbox, bool active)
    {
        if (aoeHitbox == null) return;
        aoeHitbox.SetActive(active);
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
