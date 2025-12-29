
using UnityEngine;

public class BossAttackState : State
{
    private BossAI _boss;
    private PlayerMovement _player;

    private float _attackExitBuffer = 1.0f;
    private float _faceSpeed = 12f;

    public BossAttackState(BossAI boss )
    {
        _boss = boss;
    }

    public void Enter()
    {
        _boss._agent.isStopped = true;
        _boss._agent.ResetPath();
        _boss._agent.updateRotation = false;

        if (_boss._player != null)
        {
            _player = _boss._player.GetComponent<PlayerMovement>();
        }
    }

    public void Execute()
    {
        if (_boss._player == null)
        {
            _boss.StateMachine.ChangeState(_boss.ReturnState);
            return;
        }

        if (_boss._player != null)
        {
            _player = _boss._player.GetComponent<PlayerMovement>();
        }

        FacePlayer();

        var stateInfo = _boss._animator.GetCurrentAnimatorStateInfo(0);
        bool inAttackAnim = stateInfo.IsTag("Attack");
        bool isAttackAnimEnded = inAttackAnim && stateInfo.normalizedTime >= 0.9f;

        float dist = _boss.GetDistanceToPlayer();
        float engageRange = _boss.GetEngageRange();

        if (inAttackAnim)
        {
            _boss._agent.isStopped = true;
        }
        else
        {
            _boss._agent.isStopped = false;
        }

        if (dist > engageRange + _attackExitBuffer && (!inAttackAnim || isAttackAnimEnded))
        {
            ResetAttackTriggers();
            _boss.StateMachine.ChangeState(_boss.ChaseState);
            return;
        }

        if (dist <= engageRange + _attackExitBuffer && (!inAttackAnim || isAttackAnimEnded))
        {
            TryAttack(dist);
        }

    }

    public void Exit() 
    {
        _boss._agent.updateRotation = true;
        _boss._agent.isStopped = false;
    }


    private void FacePlayer()
    {
        Vector3 dir = _boss._player.position - _boss.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        _boss.transform.rotation = Quaternion.Slerp(
            _boss.transform.rotation,
            targetRot,
            Time.deltaTime * _faceSpeed
        );
    }


    private void TryAttack(float dist)
    {
        float relatedHP = 0;
        float stamina = 0;

        bool phase2 = _boss.CurrentPhase == BossAI.BossPhase.Phase2;

        if(_player != null)
        {
            relatedHP = _player._currentHealth - _boss.currentHealth;
            stamina = _player._currentStamina;
        }

        var s = FuzzyBossAttack.AttackProb(relatedHP, stamina, dist, phase2);

        if (dist > _boss.meleeRange) s.melee = 0f;
        if (dist > _boss.rangedRange) s.ranged = 0f;
        if (!phase2 || dist > _boss.aoeRange) s.aoe = 0f;
        
        float total = s.melee + s.ranged + s.aoe;
        if (total <= 0.001f) return;

        float r = UnityEngine.Random.Range(0f, total);

        if (r < s.melee)
        {
            TriggerAttack(BossAI.BossAttackType.Melee, _boss.meleeDamage, _boss.meleeTrigger);
        }
        else if (r < s.melee + s.ranged)
        {
            TriggerAttack(BossAI.BossAttackType.Ranged, _boss.rangedDamage, _boss.rangedTrigger);
        }
        else
        {
            TriggerAttack(BossAI.BossAttackType.Aoe, _boss.aoeDamage, _boss.aoeTrigger);
        }
    }

    private void TriggerAttack(BossAI.BossAttackType type, int damage, string trigger)
    {
        if (string.IsNullOrEmpty(trigger)) return;

        _boss.SetAttackType(type);
        _boss.attackDamage = damage;
        _boss.ConsumeAttackCooldown(type);
        if(type == BossAI.BossAttackType.Melee)
        {
            int idx = Random.Range(0, 2);
            _boss._animator.SetInteger("MeleeAttackIndex", idx);
            _boss._animator.SetTrigger(trigger);
        }
        else
        {
            _boss._animator.SetTrigger(trigger);
        }
    }

    private void ResetAttackTriggers()
    {
        if (!string.IsNullOrEmpty(_boss.meleeTrigger))
        {
            _boss._animator.ResetTrigger(_boss.meleeTrigger);
        }
        if (!string.IsNullOrEmpty(_boss.rangedTrigger))
        {
            _boss._animator.ResetTrigger(_boss.rangedTrigger);
        }
        if (!string.IsNullOrEmpty(_boss.aoeTrigger))
        {
            _boss._animator.ResetTrigger(_boss.aoeTrigger);
        }
    }
}
