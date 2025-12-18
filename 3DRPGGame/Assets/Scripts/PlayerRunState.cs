using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : State
{
    private PlayerMovement _player;

    public PlayerRunState(PlayerMovement player)
    {
        _player = player;
    }

    public  void Enter()
    {
        //Debug.Log("Entered Run State");
        _player.finalSpeed = _player.runSpeed;
    }

    public  void Execute()
    {
        if(_player._currentStamina <= 0)
        {
            _player.isRunning = false;
            _player.StateMachine.ChangeState(_player.WalkState);
            return;
        }
        if(!_player.HasMoveInput())
        {
            _player.StateMachine.ChangeState(_player.IdleState);
        }
        else if(!_player.isRunning)
        {
            _player.StateMachine.ChangeState(_player.WalkState);
        }
        
        _player.finalSpeed = _player.runSpeed;
        _player.Move(_player.finalSpeed);
    }
    public  void Exit()
    {
       // Debug.Log("Exited Run State");
    }
}
