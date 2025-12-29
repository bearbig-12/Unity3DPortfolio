using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : State
{
    private EnemyAI _enemy;

    public EnemyIdleState(EnemyAI enemy)
    {
        _enemy = enemy;
    }

    

    public void Enter()
    {
        _enemy._agent.isStopped = true;
        _enemy._agent.ResetPath();
    }

    public void Execute()
    {
        if (_enemy.GetDistanceToPlayer() <= _enemy.alertRange)
        {
            if (_enemy is BossAI)
            {
                _enemy.StateMachine.ChangeState(_enemy.ChaseState);
            }
            else
            {
                _enemy.StateMachine.ChangeState(_enemy.PatrolState);
            }
        }
    }

    public void Exit()
    {
       // Debug.Log("Exited Idle State");
    }

}
