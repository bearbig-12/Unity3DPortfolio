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
    public float alertRange = 10f; // Idle -> Patrol용 거리
    public float fovRange = 10f; // Patrol -> Chase용 거리
    public float fovAngle = 90f; // Enemy의 시야각
    public float attackRange = 3f; // Chase -> Attack (공격 유효거리)
    public float chaseBreakRange = 20f; // Chase -> Patrol 거리
    public float returnRange = 30f; // patrol -> Idle

    [Header("Speeds")]
    public float idleSpeed = 0f;
    public float patrolSpeed = 4f;
    public float chaseSpeed = 11f;


    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolArrived = 1.0f;



    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public float attackHitDelay = 0.2f; // 애니 타이밍 맞추기용
    public float attackHitRadius = 1.2f;
    public Transform attackHitPoint;
    public LayerMask playerLayer;
    public int attackDamage = 10;
    public float nextAttackTime = 0f;

    [Header("Return")]
    public Vector3 homePos;
    public float homeArriveDist = 1.0f;

    public StateMachine StateMachine { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyReturnState ReturnState { get; private set; }

    void Awake()
    {
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _agent.angularSpeed = 360f;     // 회전 속도(너무 낮으면 미끄러짐)
        _agent.acceleration = 20f;      // 가속(너무 낮으면 둔함)
        _agent.autoBraking = true;      // 코너에서 속도 줄이기
    }

    // Start is called before the first frame update
    void Start()
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

        StateMachine = new StateMachine();
        IdleState = new EnemyIdleState(this);
        PatrolState = new EnemyPatrolState(this);
        ChaseState = new EnemyChaseState(this);
        //AttackState = new EnemyAttackState(this);
        ReturnState = new EnemyReturnState(this);

        StateMachine.ChangeState(IdleState);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        StateMachine.Update();

    }

    void LateUpdate()
    {
        if (healthBar != null)
        {
            healthBar.transform.position = transform.position + healthBarOffset;
            healthBar.transform.forward = Camera.main.transform.forward;
        }

    }

    void Move()
    {
        float speed01 = 0f; // 애니메이터용 속도 0~1
        if (_agent != null && _agent.speed > 0.01f)
        {
            speed01 = _agent.velocity.magnitude / chaseSpeed;
            speed01 = Mathf.Clamp01(speed01);
        }
        _animator.SetFloat("Blend", speed01, 0.1f, Time.deltaTime);

    }

    public void MoveTo(Vector3 target, float speed)
    {
        _agent.isStopped = false;
        _agent.speed = speed;
        _agent.SetDestination(target);
    }

    public float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _player.position);
    }

    public bool IsPlayerOnSight()
    {
        if (GetDistanceToPlayer() > fovRange)
        {
            return false;
        }

        // 캐릭터가 살짝 위 혹은 아래에 있을 수 있으니 y축은 무시
        Vector3 directionToPlayer = (_player.position - transform.position);
        directionToPlayer.y = 0f;
        directionToPlayer.Normalize();

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // 시야각이 90도면 실제 시야는 좌45 우 45
        if (angleToPlayer < fovAngle / 2f)
        {
            RaycastHit hit;
            Vector3 enemyEyePos = transform.position + Vector3.up;

            if (Physics.Raycast(enemyEyePos, directionToPlayer, out hit, fovRange))
            {
                if (hit.transform == _player)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void TakeDamge(int damage)
    {
        if (isDead)
            return;
        currentHealth -= damage;
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        _agent.isStopped = true;
        _animator.SetTrigger("Die");
        // 추가: 콜라이더 비활성화, 아이템 드랍 등
        Collider col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }



    



    void OnDrawGizmos()
    {
        // alertRange (Idle -> Patrol)
        Gizmos.color = Color.cyan ;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        // fovRange (Patrol -> Chase 거리)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fovRange);

        // FOV 각도 표시 (좌/우 경계선)
        Vector3 origin = transform.position + Vector3.up * 1.0f;

        float half = fovAngle * 0.5f;
        Vector3 leftDir = Quaternion.Euler(0f, -half, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, half, 0f) * transform.forward;

        Gizmos.DrawLine(origin, origin + leftDir.normalized * fovRange);
        Gizmos.DrawLine(origin, origin + rightDir.normalized * fovRange);

        // (선택) 시야 부채꼴을 대충 그려주기 (디버그용)
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
