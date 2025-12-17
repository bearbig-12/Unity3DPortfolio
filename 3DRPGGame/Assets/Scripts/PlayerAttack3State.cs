using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack3State : State
{
    private PlayerMovement _player;
    private bool _isNextAttackQueued = false;



    public PlayerAttack3State(PlayerMovement player)
    {
        _player = player;
    }

    public void Enter()
    {
        Debug.Log("Entered Attack3 State");
        _player.SetAttacking(true);

        _player.ChangeStamina(-10);


        _player._animator.SetTrigger("Attack03");
        // 구르는 동안 이동  파라미터를 0으로 잠궈준다
        _player._animator.SetFloat("MoveX", 0f);
        _player._animator.SetFloat("MoveY", 0f);
    }

    public void Execute()
    {
        AnimatorStateInfo info = _player._animator.GetCurrentAnimatorStateInfo(0);
        if ( info.normalizedTime >= 1f)
        {
            _player._attackTimer = _player._attackDelay;
            // 공격 끝나면 대기 상태로 복귀
            if (_player.HasMoveInput())
            {
                if (_player.isRunning)
                {
                    _player.StateMachine.ChangeState(_player.RunState);
                }
                else
                {
                    _player.StateMachine.ChangeState(_player.WalkState);

                }
            }
            else
            {
                _player.StateMachine.ChangeState(_player.IdleState);
            }


        }
    }


    public void Exit()
    {
        Debug.Log("Exited Attack3 State");
        _player.SetAttacking(false);
        _isNextAttackQueued = false;
    }
}
