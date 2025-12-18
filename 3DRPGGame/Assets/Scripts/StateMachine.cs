using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private State _currentState;
    public void ChangeState(State next)
    {
        if (next == null)
        {
            Debug.LogError("[StateMachine] next is NULL");
            return;
        }

        if (_currentState == next) return;

        _currentState?.Exit();
        _currentState = next;
        _currentState.Enter();
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
