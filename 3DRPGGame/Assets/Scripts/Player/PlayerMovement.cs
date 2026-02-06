using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator _animator;
    Camera _camera;
    public CharacterController _characterController;
    CameraMovement _cam;

    private bool _baseStatsCached = false;

    public float speed = 3.0f;
    public float runSpeed = 10.0f;
    public float rollSpeed = 8.0f;

    public float finalSpeed;
    public bool isRunning;

    // ��׿��� ��� ī�޶� ȸ�� ���
    public bool toggleCameraRotation;
    public float smoothness = 10f;

    public bool isAttacking = false;
    public float _attackDelay = 0.3f;
    public float _attackTimer = 0.0f;
    private bool _attackActive = false;
    private HashSet<EnemyAI> _enemiesHit = new HashSet<EnemyAI>();

    private Vector3 _velocity;
    public float gravity = -20f;

    public Vector2 MoveInput { get; private set; }

    [Header("PlayerInfo")]
    public int _maxHealth = 100;
    public int _currentHealth;
    public HealthBar _healthbar;
    private int _baseMaxHealth;

    [Header("Stamina Info")]
    public int _maxStamina = 100;
    public int _currentStamina;
    public StaminaBar _staminaBar;
    private int _baseMaxStamina;

    float staminaTick = 1.0f;      // 1�ʸ��� ȸ��/����
    float staminaTimer = 0.0f;

    [Header("Weapon Info")]
    public Transform weaponRoot;// ���� ���� ��ġ
    public Transform weaponTip; // ���� �� ��ġ
    public float bladeRadius = 0.12f; // Į �β�
    public LayerMask enemyLayer;
    public int attackDamage = 20;
    private int _baseAttackDamage;

    // ��ų ����߿��� �ٸ� ���� ���ϰ�
    public bool isCastingSkill = false;

    [SerializeField] private float hitLockDuration = 0.5f; // Hit ���ϸ��̼� ��� �ð�
    [SerializeField] private float hitCooldown = 1f;
    private float _hitLockEndTime = 0f;
    private float _nextHitAllowedTime = 0f;
    private bool _isHitPlaying = false;

    private bool IsHitLocked => Time.time < _hitLockEndTime;

    public enum HealType
    {
        HP,
        Stamina
    }

    // State
    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerRollState RollState { get; private set; }
    public PlayerAttack1State Attack1State { get; private set; }
    public PlayerAttack2State Attack2State { get; private set; }
    public PlayerAttack3State Attack3State { get; private set; }


    private ShopKepper _shop;

    // Start is called before the first frame update
    void Start()
    {
        CacheBaseStats();

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

        _shop = FindObjectOfType<ShopKepper>();


        StateMachine.ChangeState(IdleState);
    }

    // Update is called once per frame
    void Update()
    {
        // ī�޶� ȸ�� ��� 
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

        if (IsHitLocked)
        {
            MoveInput = Vector2.zero;
            SetIdleAnim();
            return;  
        }



        // �޸���
        bool canRun = _currentStamina > 0;
        if ((canRun))
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
        }
        else
        {
            isRunning = false;
        }

        // �κ��丮 â�� ���� �ִ��� ����
        bool inventoryOpen = InventorySystem.instance != null && InventorySystem.instance.IsOpen;

        if (_attackTimer > 0)
        {
            _attackTimer -= Time.deltaTime;
        }

        if (inventoryOpen || _shop.IsShop || isCastingSkill)
        {
            MoveInput = Vector2.zero;
            SetIdleAnim();
            return; 
        }

        // �̵� �Է� �� ���� (WASD/�е� ��)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        MoveInput = new Vector2(h, v);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_currentStamina >= 15)
            {
                StateMachine.ChangeState(RollState);

            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            var currentState = StateMachine.GetState();
            // ���� ���� �ƴҶ��� ����1�� ��ȯ, ���� ���̸� �޺��� �Ѿ
            if (_currentStamina > 0 &&
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
            // �������� �ʰų� �ȱ��� �� ���¹̳� ȸ��
            else if (!isRunning && !isAttacking && !isRolling && _currentStamina < _maxStamina)
            {
                ChangeStamina(+5);
            }
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            TakeDamage(1);
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

        // ���� ���� ��/�� ����
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 moveDirection = forward * MoveInput.y + right * MoveInput.x;

        if (_characterController.isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f; // �ٴ� ���̱�
        }

        _velocity.y += gravity * Time.deltaTime;

        Vector3 FinalMove = (moveDirection.normalized * moveSpeed) + _velocity;

        // CharacterController�� ���� �̵�
        _characterController.Move(FinalMove * Time.deltaTime);


        // �ִϸ��̼� �Ķ���� ����
        float speedMul = isRunning ? 1.0f : 0.5f;

        // MoveInput.x = �¿�, MoveInput.y = �յ�
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

    // ���� �Է��� �ִ��� ���� (Idle �� Walk/Run ��ȯ�� ���)
    public bool HasMoveInput()
    {
        return MoveInput.sqrMagnitude > 0.01f;
    }

    // ���� �߿��� �̵� ���ϰ� ����
    public void SetAttacking(bool value)
    {
        isAttacking = value;

        if (value)
        {
            // ���� ������ �� �Է�/�ִ� �� ��� 0����
            MoveInput = Vector2.zero;
            _animator.SetFloat("MoveX", 0f, 0.1f, Time.deltaTime);
            _animator.SetFloat("MoveY", 0f, 0.1f, Time.deltaTime);
        }
    }

    private void EnterHitLock()
    {
        _hitLockEndTime = Time.time + hitLockDuration;
        _nextHitAllowedTime = Time.time + hitCooldown;
        _isHitPlaying = true;

        // ���� ���� ���� ����
        _attackActive = false;
        _enemiesHit.Clear();
        SetAttacking(false);

        if (StateMachine != null && IdleState != null)
            StateMachine.ChangeState(IdleState);

        _animator.ResetTrigger("Attack01");
        _animator.ResetTrigger("Attack02");
        _animator.ResetTrigger("Attack03");

        _animator.SetTrigger("Hit");
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _healthbar.SetHealth(_currentHealth);

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(damage, transform.position, DamageType.Normal);
        }

        if (Time.time >= _nextHitAllowedTime && !_isHitPlaying)
        {
            EnterHitLock();
        }
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
        if (_attackActive) return;   // �ߺ� ȣ�� ����
        _attackActive = true;
        _enemiesHit.Clear();     // ���� ���ݿ��� �ߺ� ��Ʈ ����
    }

    public void AnimEvent_AttackEnd()
    {
        _attackActive = false;
        _enemiesHit.Clear();
    }


    public void AnimEvent_SkillLockOn()
    {
        isCastingSkill = true;
        MoveInput = Vector2.zero;
        SetIdleAnim();
    }

    public void AnimEvent_SkillLockOff()
    {
        isCastingSkill = false;
    }

    public void AnimEvent_SkillAttackStart()
    {
        if (_attackActive) return;
        _attackActive = true;
        _enemiesHit.Clear();
    }

    public void AnimEvent_SkillAttackEnd()
    {
        _attackActive = false;
        _enemiesHit.Clear();
    }

    public void AnimEvent_HitEnd()
    {
        _isHitPlaying = false;
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

            // �̹� ���ݿ��� ó�� �´� ����
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

    public void SetWeaponHitPoints(Transform weaponInstance)
    {
        if (weaponInstance == null)
        {
            weaponRoot = null;
            weaponTip = null;
            return;
        }

        WeaponHitPoints points = weaponInstance.GetComponentInChildren<WeaponHitPoints>();

        if (points != null)
        {
            weaponRoot = points.weaponRoot;
            weaponTip = points.weaponTip;
        }

        if (weaponRoot == null || weaponTip == null)
        {
            Debug.LogWarning("Weapon hit points not found on " + weaponInstance.name);
        }
    }


    private void CacheBaseStats()
    {
        if (_baseStatsCached) return;
        _baseMaxHealth = _maxHealth;
        _baseMaxStamina = _maxStamina;
        _baseAttackDamage = attackDamage;
        _baseStatsCached = true;
    }

    public void ApplyLevelUpBonus(int healthBonus, int staminaBonus, int attackBonus)
    {
        CacheBaseStats();

        _maxHealth += healthBonus;
        _currentHealth += healthBonus;

        if (_healthbar != null)
        {
            _healthbar.SetMaxHealth(_maxHealth);
            _healthbar.SetHealth(_currentHealth);
        }

        _maxStamina += staminaBonus;
        _currentStamina += staminaBonus;

        if (_staminaBar != null)
        {
            _staminaBar.SetMaxStamina(_maxStamina);
            _staminaBar.SetStamina(_currentStamina);
        }

        attackDamage += attackBonus;
    }

    public void ApplyLevelFromProgress(int level, int healthPerLevel, int staminaPerLevel, int attackPerLevel)
    {
        CacheBaseStats();

        int bonusLevels = Mathf.Max(0, level - 1);

        _maxHealth = _baseMaxHealth + bonusLevels * healthPerLevel;
        _maxStamina = _baseMaxStamina + bonusLevels * staminaPerLevel;
        attackDamage = _baseAttackDamage + bonusLevels * attackPerLevel;

        _currentHealth = _maxHealth;
        _currentStamina = _maxStamina;

        if (_healthbar != null) _healthbar.SetMaxHealth(_maxHealth);
        if (_staminaBar != null) _staminaBar.SetMaxStamina(_maxStamina);
    }


    public void Heal(HealType type, int amount)
    {
        if (amount <= 0) return;

        if (type == HealType.HP)
        {
            _currentHealth += amount;
            if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
            if (_healthbar != null) _healthbar.SetHealth(_currentHealth);

            if (DamagePopupManager.Instance != null)
            {
                DamagePopupManager.Instance.ShowHeal(amount, transform.position);
            }
        }
        else if (type == HealType.Stamina)
        {
            ChangeStamina(amount);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (weaponRoot == null || weaponTip == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(weaponRoot.position, weaponTip.position);
        Gizmos.DrawWireSphere(weaponTip.position, bladeRadius);
    }
}
