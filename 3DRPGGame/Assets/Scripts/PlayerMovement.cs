using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator _animator;
    Camera _camera;
    public CharacterController _characterController;

    public float speed = 3.0f;
    public float runSpeed = 10.0f;
    public float rollSpeed = 8.0f;

    public float finalSpeed;
    public bool isRunning;

    // 배그에서 토글 카메라 회전 기능
    public bool toggleCameraRotation;
    public float smoothness = 10f;

    public bool isAttacking = false;

    public Vector2 MoveInput { get; private set; }

    // State
    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerRollState RollState { get; private set; }
    public PlayerAttack1State Attack1State { get; private set; }
    public PlayerAttack2State Attack2State { get; private set; }
    public PlayerAttack3State Attack3State { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main;
        _characterController = this.GetComponent<CharacterController>();

        StateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        RunState = new PlayerRunState(this);
        RollState = new PlayerRollState(this);  
        Attack1State = new PlayerAttack1State(this);
        Attack2State = new PlayerAttack2State(this);
        Attack3State = new PlayerAttack3State(this);

        StateMachine.ChangeState(IdleState);
    }

    // Update is called once per frame
    void Update()
    {
        // 카메라 회전 토글 
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            toggleCameraRotation = true;
        }
        else
        {
            toggleCameraRotation = false;
        }

        // 달리기
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // 이동 입력 축 저장 (WASD/패드 등)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        MoveInput = new Vector2(h, v);

    
   
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StateMachine.ChangeState(RollState);
        }
        else if(Input.GetMouseButtonDown(0))
        {
            var currentState = StateMachine.GetState();
            // 공격 중이 아닐때만 공격1로 전환, 공격 중이면 콤보로 넘어감
            if(!(currentState == Attack1State) && !(currentState == Attack2State) && !(currentState == Attack3State))
            {
                StateMachine.ChangeState(Attack1State);
            }
        
        }
      




        StateMachine.Update();

    }
    void LateUpdate()
    {
        if(toggleCameraRotation != true)
        {
            Vector3 playerRotate = Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate), Time.deltaTime * smoothness);

        }
    }

    public void Move(float moveSpeed)
    {

        // 월드 기준 전/우 방향
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 moveDirection = forward * MoveInput.y + right * MoveInput.x;

        // CharacterController로 실제 이동
        _characterController.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);

        //// 애니메이션 블렌드 값 계산 (Idle/Walk/Run에서 다르게 사용 가능)
        //float percent = (isRunning ? 1.0f : 0.5f) * moveDirection.magnitude;
        //_animator.SetFloat("Blend", percent, 0.1f, Time.deltaTime);

        // 애니메이션 파라미터 설정
        float speedMul = isRunning ? 1.0f : 0.5f;

        // MoveInput.x = 좌우, MoveInput.y = 앞뒤
        float animX = MoveInput.x * speedMul;
        float animY = MoveInput.y * speedMul;

        _animator.SetFloat("MoveX", animX, 0.1f, Time.deltaTime);
        _animator.SetFloat("MoveY", animY, 0.1f, Time.deltaTime);

    }


    public void SetIdleAnim()
    {
        _animator.SetFloat("MoveX", 0f, 0.1f, Time.deltaTime);
        _animator.SetFloat("MoveY", 0f, 0.1f, Time.deltaTime);
    }

    // 현재 입력이 있는지 여부 (Idle → Walk/Run 전환에 사용)
    public bool HasMoveInput()
    {
        return MoveInput.sqrMagnitude > 0.01f;
    }

    // 공격 중에는 이동 못하게 설정
    public void SetAttacking(bool value)
    {
        isAttacking = value;

        if (value)
        {
            // 공격 시작할 때 입력/애니 값 잠깐 0으로
            MoveInput = Vector2.zero;
            _animator.SetFloat("MoveX", 0f, 0.1f, Time.deltaTime);
            _animator.SetFloat("MoveY", 0f, 0.1f, Time.deltaTime);
        }
    }
}
