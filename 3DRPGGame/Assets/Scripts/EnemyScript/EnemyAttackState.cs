using UnityEngine;

public class EnemyAttackState : State
{
    private EnemyAI _enemy;
    private PlayerMovement _player;

    // 공격 , 추적 경계에서 튀는 걸 막기 위한 버퍼
    private float _attackExitBuffer = 1.0f;
    // 공격 중 플레이어를 바라보는 회전 속도
    private float _faceSpeed = 12f;
    public EnemyAttackState(EnemyAI enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {

        _enemy._agent.isStopped = true;
        _enemy._agent.ResetPath();
        _enemy._agent.updateRotation = false;

        if (_enemy._player != null)
            _player = _enemy._player.GetComponent<PlayerMovement>();

        _enemy._animator.SetBool("IsAttacking", true);
    }

    public void Execute()
    {
        if (_enemy._player == null)
        {
            _enemy.StateMachine.ChangeState(_enemy.PatrolState);
            return;
        }

        // 플레이어 교체 혹은 리스폰 대비용
        if (_enemy._player != null)
        {
            _player = _enemy._player.GetComponent<PlayerMovement>();
        }

        FacePlayer();

        var stateInfo = _enemy._animator.GetCurrentAnimatorStateInfo(0);
        bool inAttackAnim = stateInfo.IsTag("Attack");
        bool isAttackAnimEnded = inAttackAnim && stateInfo.normalizedTime >= 0.9f;


        float dist = _enemy.GetDistanceToPlayer();

        // 공격 애니 중에는 항상 멈춤
        if (inAttackAnim)
        {
            _enemy._agent.isStopped = true;
        }
        else
        {
            _enemy._agent.isStopped = false;
        }

        // 공격 애니가 끝난 뒤에만 이탈 체크
        if (dist > _enemy.attackRange + _attackExitBuffer && (!inAttackAnim || isAttackAnimEnded))
        {
            ResetAttackTriggers(); 
            _enemy.StateMachine.ChangeState(_enemy.ChaseState);
            return;
        }

        // 쿨다운, 기존 공격 애니메이션이 종료되었다면 다음 공격
        if (dist <= _enemy.attackRange + _attackExitBuffer &&
              Time.time > _enemy.nextAttackTime && (!inAttackAnim || isAttackAnimEnded))
        {
            _enemy.nextAttackTime = Time.time + _enemy.attackCooldown;

            float relatedHP = 0f;
            float playerStamina = 0f;

            if (_player != null)
            {
                relatedHP = Mathf.Clamp(_player._currentHealth - _enemy.currentHealth, -100f, 100f);
                playerStamina = _player._currentStamina;
            }

            float hardProb = FuzzyAttack.AttackProb(relatedHP, playerStamina);
            bool isHard = Random.value < hardProb;

            if (isHard)
            {
                _enemy._animator.SetTrigger(_enemy.hardAttackTrigger);
                _enemy.currentAttackDamage = _enemy.hardAttackDamage;
            }
            else
            {
                _enemy._animator.SetTrigger(_enemy.basicAttackTrigger);
                _enemy.currentAttackDamage = _enemy.basicAttackDamage;
            }

        }
    }
    public void Exit()
    {
        _enemy._agent.updateRotation = true;
        _enemy._agent.isStopped = false;
        _enemy._animator.SetBool("IsAttacking", false);

    }


    private void FacePlayer()
    {
        Vector3 dir = _enemy._player.position - _enemy.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        _enemy.transform.rotation = Quaternion.Slerp(
            _enemy.transform.rotation,
            targetRot,
            Time.deltaTime * _faceSpeed
        ); 


    }
    private void ResetAttackTriggers()
    {
        if (!string.IsNullOrEmpty(_enemy.basicAttackTrigger))
        {
            _enemy._animator.ResetTrigger(_enemy.basicAttackTrigger);
        }
        if (!string.IsNullOrEmpty(_enemy.hardAttackTrigger))
        {
            _enemy._animator.ResetTrigger(_enemy.hardAttackTrigger);

        }
    }

}
