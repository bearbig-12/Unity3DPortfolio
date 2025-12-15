using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReturnState : State
{
    private EnemyAI _enemy;
    public EnemyReturnState(EnemyAI enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        // Debug.Log("Entered Return State");
        _enemy.MoveTo(_enemy.homePos, _enemy.patrolSpeed);
    }

    public void Execute()
    {
        if(_enemy.GetDistanceToPlayer() <= _enemy.alertRange)
        {
            _enemy.StateMachine.ChangeState(_enemy.PatrolState);
            return;
        }

        float distHome = Vector3.Distance(_enemy.transform.position, _enemy.homePos);

        if (!_enemy._agent.pathPending && distHome <= _enemy.homeArriveDist)
        {
            _enemy.StateMachine.ChangeState(_enemy.IdleState);
        }
    }

    public void Exit()
    {
        // Debug.Log("Exited Return State");
    }
}
