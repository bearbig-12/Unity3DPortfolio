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

    // BossDefinition 캐스팅 헬퍼
    private BossDefinition BossDef => definition as BossDefinition;

    // SO에서 가져오는 보스 전용 프로퍼티들 (폴백값 포함)
    public float phase2HealthRatio => BossDef != null ? BossDef.phase2HealthRatio : 0.5f;
    public float meleeRange => BossDef != null ? BossDef.meleeRange : 2.5f;
    public float rangedRange => BossDef != null ? BossDef.rangedRange : 10f;
    public float aoeRange => BossDef != null ? BossDef.aoeRange : 6f;
    public float meleeCooldown => BossDef != null ? BossDef.meleeCooldown : 2f;
    public float rangedCooldown => BossDef != null ? BossDef.rangedCooldown : 3f;
    public float aoeCooldown => BossDef != null ? BossDef.aoeCooldown : 6f;
    public int meleeDamage => BossDef != null ? BossDef.meleeDamage : 10;
    public int rangedDamage => BossDef != null ? BossDef.rangedDamage : 8;
    public int aoeDamage => BossDef != null ? BossDef.aoeDamage : 5;
    public string meleeTrigger => BossDef != null ? BossDef.meleeTrigger : "MeleeAttack";
    public string rangedTrigger => BossDef != null ? BossDef.rangedTrigger : "RangedAttack";
    public string aoeTrigger => BossDef != null ? BossDef.aoeTrigger : "AOEAttack";
    public string phase2Trigger => BossDef != null ? BossDef.phase2Trigger : "Phase2";

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    private bool _isPhaseChanging = false;

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

    protected override void Start()
    {
        base.Start();
    }


    protected override void InitializeStates()
    {
        base.InitializeStates();
        AttackState = new BossAttackState(this);
        _rangedFireHandEffect = fireHandEffect.GetComponent<ParticleSystem>();
        _rangedFireHandEffect.Stop();
    }

    protected override void Update()
    {
        if (_isPhaseChanging)
        {
            _agent.isStopped = true;
            _animator.SetBool("IsWalking", false);
            _animator.SetBool("IsRunning", false);
            return;
        }
        base.Update();
    }

    public override void TakeDamage(int damage)
    {
        if (_isPhaseChanging) return;
        base.TakeDamage(damage);
    }

    protected override void OnDamaged(int damage)
    {
        float phase2Threshold = maxHealth * phase2HealthRatio;
        if (CurrentPhase == BossPhase.Phase1 && currentHealth <= phase2Threshold)
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
        _isPhaseChanging = true;
        _animator.SetBool("IsPhaseChanging", true);

        // 진행 중인 공격 트리거 초기화
        if (!string.IsNullOrEmpty(meleeTrigger)) _animator.ResetTrigger(meleeTrigger);
        if (!string.IsNullOrEmpty(rangedTrigger)) _animator.ResetTrigger(rangedTrigger);
        if (!string.IsNullOrEmpty(aoeTrigger)) _animator.ResetTrigger(aoeTrigger);

        if (!string.IsNullOrEmpty(phase2Trigger))
        {
            _animator.SetTrigger(phase2Trigger);
        }
    }

    // BossPhase2 애니메이션 마지막 프레임에 Animation Event로 호출
    public void AnimEvent_PhaseChangeEnd()
    {
        _isPhaseChanging = false;
        _animator.SetBool("IsPhaseChanging", false);
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
            // 새로운 파티클 효과를 시작하지 않고 현재 파티클 효과가 자연스럽게 소멸되도록
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

            GameObject obj = null;
            if (ObjectPoolManager.Instance != null)
            {
                obj = ObjectPoolManager.Instance.Get("fireball", spawnPos, Quaternion.identity);
            }
            if (obj == null)
            {
                obj = Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);
            }

            Collider FireBallColl = obj.GetComponent<Collider>();
            Collider BossColl = GetComponent<Collider>();

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

    #region Save/Load Overrides

    public override MonsterSaveData GetSaveData()
    {
        MonsterSaveData data = base.GetSaveData();
        data.bossPhase = (int)CurrentPhase;
        return data;
    }

    public override void ApplyLoadedData(MonsterSaveData data)
    {
        base.ApplyLoadedData(data);

        // 보스 페이즈 복원
        if (data.bossPhase > 0)
        {
            CurrentPhase = (BossPhase)data.bossPhase;
        }
    }

    #endregion
}
