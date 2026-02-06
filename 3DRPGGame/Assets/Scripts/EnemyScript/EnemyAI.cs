using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Monster Definition (SO)")]
    [SerializeField] protected MonsterDefinition definition;

    // 런타임 ID (definition이 없을 경우 폴백용)
    public string enemyId = "monster";

    public static event System.Action<EnemyAI> OnEnemyKilled;

    // 저장/로드 관련
    protected bool _isLoadedFromSave = false;


    public NavMeshAgent _agent;
    public Animator _animator;
    public Transform _player;

    [Header("HP")]
    public int currentHealth;
    public bool isDead = false;

    [Header("UI")]
    public HealthBar healthBar;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolArrived = 1.0f;

    [Header("Attack")]
    public float nextAttackTime = 0f;
    public EnemyDamageHitbox[] hitboxes;

    [Header("Return")]
    public Vector3 homePos;
    public float homeArriveDist = 1.0f;

    private PlayerProgress _progress;

    // SO에서 가져오는 프로퍼티들 (폴백값 포함)
    public int maxHealth => definition != null ? definition.maxHealth : 100;
    public int basicAttackDamage => definition != null ? definition.basicAttackDamage : 5;
    public int hardAttackDamage => definition != null ? definition.hardAttackDamage : 10;

    // 런타임 데미지 (공격 타입에 따라 변경됨)
    public int currentAttackDamage;
    public float attackCooldown => definition != null ? definition.attackCooldown : 1f;
    public float attackRange => definition != null ? definition.attackRange : 2f;
    public float alertRange => definition != null ? definition.alertRange : 15f;
    public float fovRange => definition != null ? definition.fovRange : 8f;
    public float fovAngle => definition != null ? definition.fovAngle : 90f;
    public float returnRange => definition != null ? definition.returnRange : 30f;
    public float patrolSpeed => definition != null ? definition.patrolSpeed : 3f;
    public float chaseSpeed => definition != null ? definition.chaseSpeed : 6f;
    public int expReward => definition != null ? definition.expReward : 30;
    public string basicAttackTrigger => definition != null ? definition.basicAttackTrigger : "BasicAttack";
    public string hardAttackTrigger => definition != null ? definition.hardAttackTrigger : "HardAttack";
    public Vector3 healthBarOffset => definition != null ? definition.healthBarOffset : new Vector3(0, 2.5f, 0);

    [Header("Hit Settings")]
    [SerializeField] private float hitCooldown = 5.0f;
    private float _nextHitAllowedTime = 0f;
    private bool _isHitPlaying = false;
    private bool _isHitLocked = false;

    private bool IsHitLocked => _isHitLocked;


    public StateMachine StateMachine { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public State AttackState { get; protected set; }
    public EnemyReturnState ReturnState { get; private set; }

    protected virtual void Awake()
    {
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }


        _agent.angularSpeed = 360f;     
        _agent.acceleration = 20f;      
        _agent.autoBraking = true;      
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // 저장된 데이터에서 로드된 경우 HP 초기화 스킵
        if (!_isLoadedFromSave)
        {
            currentHealth = maxHealth;
        }

        if(healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        // 홈 포지션 저장 (아직 설정되지 않은 경우)
        if (homePos == Vector3.zero)
        {
            homePos = transform.position;
        }

        GameObject _p = GameObject.FindGameObjectWithTag("Player");
        if (_p != null)
        {
            _player = _p.transform;
        }

        _progress = FindObjectOfType<PlayerProgress>();

        InitializeStates();

        StateMachine.ChangeState(IdleState);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isDead)
            return;

        if (IsHitLocked)
        {
            if (_agent != null) _agent.isStopped = true;
            _animator.SetBool("IsWalking", false);
            _animator.SetBool("IsRunning", false);
            return;
        }
        else
        {
            if (_agent != null) _agent.isStopped = false;
        }

        Move();
        StateMachine.Update();

    }

    protected virtual void LateUpdate()
    {
        if (healthBar != null)
        {
            healthBar.transform.position = transform.position + healthBarOffset;
            healthBar.transform.forward = Camera.main.transform.forward;
        }

    }

    protected virtual void Move()
    {
        float v = (_agent != null) ? _agent.velocity.magnitude : 0f;

        bool moving = v > 0.05f;                 // �����̳�
        bool running = (_agent != null && _agent.speed >= chaseSpeed - 0.1f); // �޸����(�ӵ��� ����)

        _animator.SetBool("IsWalking", moving);
        _animator.SetBool("IsRunning", moving && running);

    }

    protected virtual void InitializeStates()
    {
        StateMachine = new StateMachine();
        IdleState = new EnemyIdleState(this);
        PatrolState = new EnemyPatrolState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);
        ReturnState = new EnemyReturnState(this);
    }

    public void MoveTo(Vector3 target, float speed)
    {
        _agent.isStopped = false;
        _agent.speed = speed;

        Vector3 dir = (target - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
            dir.Normalize();

        float keepDistance = _agent.stoppingDistance; 
        Vector3 offsetTarget = target - dir * keepDistance;

        _agent.SetDestination(offsetTarget);
    }

    public float GetDistanceToPlayer()
    {
        if (_player == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, _player.position);
    }

    public bool IsPlayerOnSight()
    {
        if (_player == null) return false;

        float dist = GetDistanceToPlayer();
        if (dist > fovRange) return false;

        Vector3 dir = (_player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return true;
        dir.Normalize();

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle >= fovAngle / 2f) return false;

        RaycastHit hit;
        Vector3 eye = transform.position + Vector3.up;

        if (Physics.Raycast(eye, dir, out hit, fovRange))
        {
            if (hit.transform == _player)
                return true;
            if (hit.transform.IsChildOf(_player))
                return true;
        }

        return false;
    }

    public virtual float GetEngageRange()
    {
        return attackRange;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(damage, transform.position, DamageType.Normal);
        }

        if (Time.time >= _nextHitAllowedTime && !_isHitPlaying)
        {
            _nextHitAllowedTime = Time.time + hitCooldown;
            _isHitPlaying = true;
            _animator.SetTrigger("Hit");
        }

        OnDamaged(damage);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        _agent.isStopped = true;
        _animator.SetTrigger("Die");

        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;


        _progress.AddEXP(expReward);

        // 이벤트 콜
        OnEnemyKilled?.Invoke(this);

        StartCoroutine(DespawnAfter(2f));
    }

    private System.Collections.IEnumerator DespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Destroy 대신 SetActive(false)로 변경 - 저장 추적 가능
        gameObject.SetActive(false);
    }

    #region Save/Load Methods

    public virtual MonsterSaveData GetSaveData()
    {
        MonsterSaveData data = new MonsterSaveData();
        data.monsterId = definition != null ? definition.monsterId : enemyId;
        data.isDead = isDead || !gameObject.activeSelf;
        data.currentHealth = currentHealth;
        data.SetPosition(transform.position);
        data.SetHomePosition(homePos);
        data.currentStateName = GetCurrentStateName();
        data.bossPhase = 0;
        return data;
    }

    public virtual void ApplyLoadedData(MonsterSaveData data)
    {
        _isLoadedFromSave = true;
        enemyId = data.monsterId;
        currentHealth = data.currentHealth;
        homePos = data.GetHomePosition();

        if (data.isDead)
        {
            isDead = true;
            gameObject.SetActive(false);
        }
        else
        {
            // 위치 복원
            transform.position = data.GetPosition();
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.Warp(data.GetPosition());
            }

            gameObject.SetActive(true);
        }
    }

    public string GetCurrentStateName()
    {
        if (StateMachine == null || StateMachine.GetState() == null)
            return "Idle";
        return StateMachine.GetState().GetType().Name;
    }

    #endregion

    // 보스의 Phase 2를 위한 함수
    protected virtual void OnDamaged(int damage)
    {

    }


    public virtual void AnimEvent_AttackStart()
    {
        if (hitboxes == null) return;
        foreach (var h in hitboxes)
        {
            if (h != null)
            {
                h.SetActive(true);
            }
        }
            
    }

    public virtual void AnimEvent_AttackEnd()
    {
        if (hitboxes == null) return;
        foreach (var h in hitboxes)
        {
            if (h != null)
            {
                h.SetActive(false);
            }

        }
    }


    public void AnimEvent_HitStart()
    {
        _isHitLocked = true;
    }

    public void AnimEvent_HitEnd()
    {
        _isHitLocked = false;
        _isHitPlaying = false;
    }
    void OnDrawGizmos()
    {
        // alertRange (Idle -> Patrol)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        // fovRange (Patrol -> Chase �Ÿ�)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fovRange);

        // FOV ���� ǥ�� (��/�� ��輱)
        Vector3 origin = transform.position + Vector3.up * 1.0f;

        float half = fovAngle * 0.5f;
        Vector3 leftDir = Quaternion.Euler(0f, -half, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, half, 0f) * transform.forward;

        Gizmos.DrawLine(origin, origin + leftDir.normalized * fovRange);
        Gizmos.DrawLine(origin, origin + rightDir.normalized * fovRange);

        // (����) �þ� ��ä���� ���� �׷��ֱ� (����׿�)
        int steps = 24;
        Vector3 prev = origin + leftDir.normalized * fovRange;
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float ang = Mathf.Lerp(-half, half, t);
            Vector3 dir = Quaternion.Euler(0f, ang, 0f) * transform.forward;
            Vector3 next = origin + dir.normalized * fovRange;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
