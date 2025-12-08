using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : State
{
    private PlayerMovement _player;

    public PlayerWalkState(PlayerMovement player)
    {
        _player = player;
    }

    public  void Enter()
    {
        Debug.Log("Entered Walk State");
        _player.finalSpeed = _player.speed;
    }

    public  void Execute()
    {
        if(!_player.HasMoveInput())
        {
            _player.StateMachine.ChangeState(_player.IdleState);
        }
        else if(_player.isRunning)
        {
            _player.StateMachine.ChangeState(_player.RunState);
        }
        
        _player.finalSpeed = _player.speed;
        _player.Move(_player.finalSpeed);
    }
    public  void Exit()
    {
        Debug.Log("Exited Walk State");
    }
}
