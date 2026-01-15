using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent _agent;
    public Animator _animator;
    public Transform _player;

    [Header("HP")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    [Header("UI")]
    public HealthBar healthBar;
    public Vector3 healthBarOffset = new Vector3(0, 2.5f, 0);

    [Header("Range")]
    public float alertRange = 15f; // Idle -> Patrol
    public float fovRange = 8f; // Patrol -> Chase
    public float fovAngle = 90f; // Enemy 시야각
    public float attackRange = 2f; // Chase -> Attack 
    public float returnRange = 30f; // patrol -> Idle

    [Header("Speeds")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 6f;


    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolArrived = 1.0f;



    [Header("Attack")]
    public float attackCooldown = 1f;
    public int attackDamage = 10;
    public float nextAttackTime = 0f;
    public EnemyDamageHitbox[] hitboxes;

    [Header("Enemy Animator Triggers")]
    public string basicAttackTrigger = "BasicAttack";
    public string hardAttackTrigger = "HardAttack";


    [Header("Return")]
    public Vector3 homePos;
    public float homeArriveDist = 1.0f;




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
        currentHealth = maxHealth;

        if(healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        homePos = transform.position;

        GameObject _p = GameObject.FindGameObjectWithTag("Player");
        if (_p != null)
        {
            _player = _p.transform;
        }

        InitializeStates();

        StateMachine.ChangeState(IdleState);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(isDead)
            return;

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

        OnDamaged(damage);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        _agent.isStopped = true;
        _animator.SetTrigger("Die");

        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(DespawnAfter(2f));
    }

    private System.Collections.IEnumerator DespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

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
