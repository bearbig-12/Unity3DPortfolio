using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : State
{
    private EnemyAI _enemy;
    private int index = 0;

    public EnemyPatrolState(EnemyAI enemy)
    {
        _enemy = enemy;
        index = 0;
    }

    public void Enter()
    {
       if(_enemy.patrolPoints == null || _enemy.patrolPoints.Length == 0)
       {
            Debug.LogWarning("No patrol points assigned for EnemyPatrolState.");
            return;
       }

       index = Mathf.Clamp(index, 0, _enemy.patrolPoints.Length - 1);
        _enemy.MoveTo(_enemy.patrolPoints[index].position, _enemy.patrolSpeed);
    }

    public void Execute()
    {
        if (_enemy.GetDistanceToPlayer() > _enemy.returnRange)
        {
            _enemy.StateMachine.ChangeState(_enemy.ReturnState);
            return;
        }

        if (_enemy.IsPlayerOnSight())
        {
            _enemy.StateMachine.ChangeState(_enemy.ChaseState);
            return;
        }
        // 웨이포인트에 도착했는지 확인하고 다음 위치 설정
        if (!_enemy._agent.pathPending && _enemy._agent.remainingDistance <= _enemy.patrolArrived)
        {
            index = (index + 1) % _enemy.patrolPoints.Length;
            _enemy.MoveTo(_enemy.patrolPoints[index].position, _enemy.patrolSpeed);
        }
    }

    void Exit()
    {
        // Debug.Log("Exited Patrol State");
    }

}
