using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator _animator;
    Camera _camera;
    public CharacterController _characterController;
    CameraMovement _cam;


    public float speed = 3.0f;
    public float runSpeed = 10.0f;
    public float rollSpeed = 8.0f;

    public float finalSpeed;
    public bool isRunning;

    // 배그에서 토글 카메라 회전 기능
    public bool toggleCameraRotation;
    public float smoothness = 10f;

    public bool isAttacking = false;
    public float _attackDelay = 0.3f;
    public float _attackTimer = 0.0f;
    private bool _attackActive = false;
    private HashSet<EnemyAI> _enemiesHit = new HashSet<EnemyAI>();

    public Vector2 MoveInput { get; private set; }

    [Header("PlayerInfo")]
    public int _maxHealth = 100;
    public int _currentHealth;
    public HealthBar _healthbar;

    [Header("Stamina Info")]
    public int _maxStamina = 100;
    public int _currentStamina;
    public StaminaBar _staminaBar;

    float staminaTick = 1.0f;      // 1초마다 회복/감소
    float staminaTimer = 0.0f;

    [Header("Weapon Info")]
    public Transform weaponRoot;// 무기 시작 위치
    public Transform weaponTip; // 무기 끝 위치
    public float bladeRadius = 0.12f; // 칼 두께
    public LayerMask enemyLayer;
    public int attackDamage = 20;

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
        _cam = FindObjectOfType<CameraMovement>();

        _currentHealth = _maxHealth;
        _healthbar.SetMaxHealth(_maxHealth);

        _currentStamina = _maxStamina;
        _staminaBar.SetMaxStamina(_maxStamina);

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            _cam.ToggleLockOn(transform); 
        }

        // 달리기
        bool canRun = _currentStamina > 0;
        if ((canRun))
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
        }
        else
        {
            isRunning = false;
        }


        // 이동 입력 축 저장 (WASD/패드 등)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        MoveInput = new Vector2(h, v);

        if(_attackTimer > 0)
        {
            _attackTimer -= Time.deltaTime;
        }
   
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(_currentStamina >= 15)
            {
                StateMachine.ChangeState(RollState);

            }
        }
        else if(Input.GetMouseButtonDown(0))
        {
            var currentState = StateMachine.GetState();
            // 공격 중이 아닐때만 공격1로 전환, 공격 중이면 콤보로 넘어감
            if(_currentStamina > 0 && 
                _attackTimer <= 0 &&
                !(currentState == Attack1State) && 
                !(currentState == Attack2State) && 
                !(currentState == Attack3State))
            {
                StateMachine.ChangeState(Attack1State);
            }
        
        }
      
    
        staminaTimer += Time.deltaTime;

        if (staminaTimer >= staminaTick)
        {
            staminaTimer = 0f;

            bool isRolling = StateMachine.GetState() == RollState;

            if (isRunning && HasMoveInput() && _currentStamina > 0)
            {
                ChangeStamina(-5);
            }
            // 움직이지 않거나 걷기일 때 스태미나 회복
            else if (!isRunning && !isAttacking && !isRolling && _currentStamina < _maxStamina)
            {
                ChangeStamina(+5);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(10);
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

        if(_attackActive)
        {
            WeaponHitCheck();
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

    void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _healthbar.SetHealth(_currentHealth);
    }

    public void ChangeStamina(int amount)
    {
        _currentStamina += amount;

        if (_currentStamina < 0)
            _currentStamina = 0;
        if (_currentStamina > _maxStamina)
            _currentStamina = _maxStamina;

        _staminaBar.SetStamina(_currentStamina);
    }
    public void AnimEvent_AttackStart()
    {
        _attackActive = true;
        _enemiesHit.Clear();     // 같은 공격에서 중복 히트 방지
    }
    public void WeaponHitCheck()
    {
        if(weaponRoot == null || weaponTip == null)
        {
            return;
        }

        Vector3 start = weaponRoot.position;
        Vector3 end = weaponTip.position;

        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);


        RaycastHit[] hits = Physics.SphereCastAll(start, bladeRadius, direction, distance, enemyLayer);
        foreach (var h in hits)
        {
            var enemy = h.collider.GetComponentInParent<EnemyAI>();
            if (enemy == null)
            {
                continue;
            }

            // 이번 공격에서 처음 맞는 적만
            if (_enemiesHit.Add(enemy))
            {
                Debug.Log("Hit Enemy: " + enemy.name);
                enemy.TakeDamage(attackDamage);
            }
        }
        //foreach (var hit in hits)
        //{
        //    if (hit.collider.CompareTag("Enemy"))
        //    {
        //        Debug.Log("Enemy Hit!! : " + hit.collider.name);
        //    }
        //}
    }

    // 공격 끝 프레임 (선택)
    public void AnimEvent_AttackEnd()
    {
        _attackActive = false;
        _enemiesHit.Clear();
    }


    void OnDrawGizmosSelected()
    {
        if (weaponRoot == null || weaponTip == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(weaponRoot.position, weaponTip.position);
        Gizmos.DrawWireSphere(weaponTip.position, bladeRadius);
    }
}
