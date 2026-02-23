using UnityEngine;

public class PlayerRollState : State
{
    private PlayerMovement _player;

    private float _rollTime = 1.15f;    // 롤링 최대 시간 (애니메이션 길이에 맞게 조정 필요)
    private float _elapsed = 0f;
    private Vector3 _rollDir;

    public PlayerRollState(PlayerMovement player)
    {
        _player = player;
    }

    public void Enter()
    {
        //Debug.Log("Entered Roll State");
        _elapsed = 0f;

        _player.ChangeStamina(-15);


        // 롤링 방향 결정 (입력 방향이 있으면 그 방향, 없으면 전방향)
        if (_player.HasMoveInput())
        {
            Vector3 forward = _player.transform.TransformDirection(Vector3.forward);
            Vector3 right = _player.transform.TransformDirection(Vector3.right);
            Vector2 input = _player.MoveInput.normalized;

            _rollDir = (forward * input.y + right * input.x).normalized;
        }
        else
        {
            _rollDir = _player.transform.forward;
        }

        _player._animator.SetTrigger("Roll");

        // 롤링 중에는 이동 파라미터를 0으로 초기화
        _player._animator.SetFloat("MoveX", 0f);
        _player._animator.SetFloat("MoveY", 0f);
    }

    public void Execute()
    {
        _elapsed += Time.deltaTime;

        _player._characterController.Move(_rollDir * _player.rollSpeed * Time.deltaTime);

        AnimatorStateInfo info = _player._animator.GetCurrentAnimatorStateInfo(0);
        bool animFinished = info.IsName("Roll") && info.normalizedTime >= 0.95f;

        if (_elapsed >= _rollTime || animFinished)
        {
            if (_player.HasMoveInput())
            {
                if (_player.isRunning)
                    _player.StateMachine.ChangeState(_player.RunState);
                else
                    _player.StateMachine.ChangeState(_player.WalkState);
            }
            else
            {
                _player.StateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public void Exit()
    {
       // Debug.Log("Exited Roll State");
    }
}
