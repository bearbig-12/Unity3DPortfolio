using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : State
{
    private EnemyAI _enemy;
    private PlayerMovement _player;
    private bool _startedAttack = false;

    public EnemyAttackState(EnemyAI enemy)
    {
        _enemy = enemy;


    }

    public void Enter()
    {
        _enemy._agent.isStopped = true;
        _enemy._agent.ResetPath();  

        if(_enemy._player != null)
        {
            _player = _enemy._player.GetComponent<PlayerMovement>();
        }

        _startedAttack = false;
    }

public void Execute()
    {
        if (_enemy.isDead) return;
        if (_enemy._player == null)
        {
            _enemy.StateMachine.ChangeState(_enemy.PatrolState);
            return;
        }

        // 공격거리 벗어나면 다시 추격
        if (_enemy.GetDistanceToPlayer() > _enemy.attackRange)
        {
            _enemy.StateMachine.ChangeState(_enemy.ChaseState);
            return;
        }

        if (Time.time < _enemy.nextAttackTime) return;

        Vector3 dir = _enemy._player.position - _enemy.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir.normalized);
            _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, look, Time.deltaTime * 12f);
        }

        // 공격 시작
        if (!_startedAttack)
        {
            _startedAttack = true;
            _enemy.nextAttackTime = Time.time + _enemy.attackCooldown;

            float relatedHP = 0f;
            float playerSta = 50f;

            if (_player != null)
            {
                relatedHP = Mathf.Clamp(_player._currentHealth - _enemy.currentHealth, -100f, 100f);
                playerStamina = _player._currentStamina;
            }

            float attackValue = FuzzyAttack.AttackProb(relatedHP, playerStamina);
            // 공격 확률 계산
            bool isHard = Random.value < attackValue;

            if (isHard)
            {
                // 강공격
                _enemy._animator.SetTrigger("Attack_Hard");
                _enemy.attackDamage = 20; 
            }
            else
            {
                // 일반공격
                _enemy._animator.SetTrigger("Attack_Basic");
                _enemy.attackDamage = 10;
            }
        }

        // 애니가 끝나면 자동 복귀
        AnimatorStateInfo info = _enemy._animator.GetCurrentAnimatorStateInfo(0);

        // 아래 IsName은 너 애니 state 이름에 맞춰 바꿔야 함
        // "Enemy_Attack_Basic", "Enemy_Attack_Hard" 같은 실제 state 이름
        bool inBasic = info.IsName("Enemy_Attack_Basic");
        bool inHard = info.IsName("Enemy_Attack_Hard");

        if ((inBasic || inHard) && info.normalizedTime >= 0.95f)
        {
            _enemy.StateMachine.ChangeState(_enemy.ChaseState);
        }
    }

    public void Exit()
    {
        _startedAttack = false;

    }


}
