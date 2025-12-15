using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    private State _currentState;

    public void ChangeState(State newState)
    {
        if(_currentState != null)
        {
            _currentState.Exit();
        }
        _currentState = newState;

        if(_currentState != null)
        {
            _currentState.Enter();
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        _currentState?.Execute();
    }

    public State GetState()
    {
        return _currentState;
    }
}
