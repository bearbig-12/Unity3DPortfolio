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
        Debug.Log("[EnemyAttack] Enter");

        _enemy._agent.isStopped = true;
        _enemy._agent.ResetPath();
        _enemy._agent.updateRotation = false;

        if (_enemy._player != null)
            _player = _enemy._player.GetComponent<PlayerMovement>();

        SetHitboxes(true);
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
        bool isAttackAnim = stateInfo.IsTag("Attack");
        bool isAttackAnimEnded = isAttackAnim && stateInfo.normalizedTime >= 0.9f;

        // 공격 중이 아니고 공격거리 벗어나면 추적으로 전환
        if (!isAttackAnim && _enemy.GetDistanceToPlayer() > _enemy.attackRange + _attackExitBuffer)
        {
            _enemy._animator.ResetTrigger("BasicAttack");
            _enemy._animator.ResetTrigger("HardAttack");
            _enemy._animator.CrossFade("Movement", 0.05f, 0);

            _enemy.StateMachine.ChangeState(_enemy.ChaseState);
            return;
        }

        // 쿨다운, 기존 공격 애니메이션이 종료되었다면 다음 공격
        if (Time.time > _enemy.nextAttackTime && (!isAttackAnim || isAttackAnimEnded))
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
                int idx = Random.Range(0, 3);
                _enemy._animator.SetInteger("HardAttackIndex", idx);
                _enemy._animator.SetTrigger("HardAttack");
                _enemy.attackDamage = 20;
            }
            else
            {
                int idx = Random.Range(0, 2);
                _enemy._animator.SetInteger("BasicAttackIndex", idx);
                _enemy._animator.SetTrigger("BasicAttack");
                _enemy.attackDamage = 10;
            }

        }
    }
    public void Exit()
    {
        _enemy._agent.updateRotation = true;
        _enemy._agent.isStopped = false;
        SetHitboxes(false);

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

    private void SetHitboxes(bool value)
    {
        if (_enemy.hitboxes == null) return;
        foreach (var h in _enemy.hitboxes)
        {
            if (h != null)
            {
                h.SetActive(value);
            }
        }
    }
}
