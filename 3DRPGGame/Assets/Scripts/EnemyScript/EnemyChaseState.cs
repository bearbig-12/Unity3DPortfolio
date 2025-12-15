using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : State
{
    private EnemyAI _enemy;

    public EnemyChaseState(EnemyAI enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        // Debug.Log("Entered Chase State");
        if(_enemy._player != null)
        {
            _enemy.MoveTo(_enemy._player.position, _enemy.chaseSpeed);
        }
    }

    public void Execute()
    {
        if (_enemy.GetDistanceToPlayer() > _enemy.returnRange)
        {
            _enemy.StateMachine.ChangeState(_enemy.ReturnState);
            return;
        }
        if (!_enemy.IsPlayerOnSight())
        {
            _enemy.StateMachine.ChangeState(_enemy.PatrolState);
            return;
        }
        // 플레이어 위치로 계속 이동
        if(_enemy._player != null)
        {
            _enemy.MoveTo(_enemy._player.position, _enemy.chaseSpeed);
        }
    }

    public void Exit()
    {
        // Debug.Log("Exited Chase State");
    }
}
