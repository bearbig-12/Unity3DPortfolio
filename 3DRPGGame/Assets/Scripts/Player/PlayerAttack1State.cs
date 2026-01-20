using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack1State : State
{
    private PlayerMovement _player;
    private bool _isNextAttackQueued = false;

    private float comboStart = 0.2f;
    private float comboEnd = 0.7f;

    public PlayerAttack1State(PlayerMovement player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.SetAttacking(true);

        _player.ChangeStamina(-10);

        _player._animator.SetTrigger("Attack01");
        // 구르는 동안 이동  파라미터를 0으로 잠궈준다
        _player._animator.SetFloat("MoveX", 0f);
        _player._animator.SetFloat("MoveY", 0f);
    }

    public void Execute()
    {
        AnimatorStateInfo info = _player._animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Melee_Attack01"))
        {
            float normalizedTime = info.normalizedTime % 1; // Loop 방지
            // 콤보 입력 허용 구간 체크
            if (normalizedTime >= comboStart && normalizedTime <= comboEnd)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _isNextAttackQueued = true;
                }
            }
        }
       


        if (info.IsName("Melee_Attack01") && info.normalizedTime >= 0.8f)
        {
            if (_isNextAttackQueued)
            {
                _player.StateMachine.ChangeState(_player.Attack2State);
            }
            else
            {
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
    }

    public void Exit()
    {
        _player.SetAttacking(false);
        _isNextAttackQueued = false;
    }


}
