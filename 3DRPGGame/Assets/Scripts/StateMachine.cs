using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private State _currentState;
    public void ChangeState(State newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }
        _currentState = newState;

        if(_currentState != null)
        {
            _currentState.Enter();
        }
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
    public void Update()
    {
        _currentState?.Execute();
    }

    public State GetState()
    {
        return _currentState;
    }
}
