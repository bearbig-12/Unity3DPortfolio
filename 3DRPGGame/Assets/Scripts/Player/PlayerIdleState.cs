using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : State
{
    private PlayerMovement _player;

    public PlayerIdleState(PlayerMovement player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.SetIdleAnim();

    }

    public void Execute()
    {
        if(_player.HasMoveInput())
        {
            if(_player.isRunning)
            {
                _player.StateMachine.ChangeState(_player.RunState);
            }
            else
            {
                _player.StateMachine.ChangeState(_player.WalkState);
            }

        }

        _player.SetIdleAnim();

    }

    public void Exit()
    {

    }

}
