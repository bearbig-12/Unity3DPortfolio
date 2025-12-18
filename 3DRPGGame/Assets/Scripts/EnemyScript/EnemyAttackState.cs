using UnityEngine;

public class EnemyAttackState : State
{
    private EnemyAI _enemy;
    private PlayerMovement _player;

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
    }

    public void Execute()
    {
        if (_enemy._player == null)
        {
            _enemy.StateMachine.ChangeState(_enemy.PatrolState);
            return;
        }

        bool inAttackAnim = _enemy._animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        if (!inAttackAnim && _enemy.GetDistanceToPlayer() > _enemy.attackRange + 1.0f)
        {
            _enemy._animator.ResetTrigger("BasicAttack");
            _enemy._animator.ResetTrigger("HardAttack");
            _enemy._animator.CrossFade("Movement", 0.05f, 0);

            _enemy.StateMachine.ChangeState(_enemy.ChaseState);
            return;
        }


 

        float relatedHP = 0f;
        float playerStamina = 0f;

        if (_player != null)
        {
            relatedHP = Mathf.Clamp(_player._currentHealth - _enemy.currentHealth, -100f, 100f);
            playerStamina = _player._currentStamina;
        }

        float hardProb = FuzzyAttack.AttackProb(relatedHP, playerStamina);
        bool isHard = Random.value < hardProb;

        Debug.Log($"[EnemyAttack] FIRE hard={isHard} hardProb={hardProb:F2}");
        if (Time.time > _enemy.nextAttackTime)
        {

            _enemy.nextAttackTime = Time.time + _enemy.attackCooldown;

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
    }
}
