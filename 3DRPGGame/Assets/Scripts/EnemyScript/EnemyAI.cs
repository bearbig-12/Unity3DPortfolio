using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent _agent;
    public Animator _animator;
    public Transform _player;

    [Header("Range")]
    public float alertRange = 10f; // Idle -> Patrol용 거리
    public float fovRange = 10f; // Patrol -> Chase용 거리
    public float fovAngle = 90f; // Enemy의 시야각
    public float attackRange = 3f; // Chase -> Attack (공격 유효거리)
    public float chaseBreakRange = 20f; // Chase -> Patrol 거리
    public float returnRange = 30f; // patrol -> Idle

    [Header("Speeds")]
    public float idleSpeed = 0f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    p

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
        if(_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        if(_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        homePos = transform.position;

        GameObject _p = GameObject.FindGameObjectWithTag("Player");
        if(_p != null)
        {
            _player = _p.transform;
        }

        StateMachine = new StateMachine();
        IdleState = new EnemyIdleState(this);
        PatrolState = new EnemyPatrolState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);
        ReturnState = new EnemyReturnState(this);

        StateMachine.ChangeState(IdleState);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        StateMachine.Update();

    }

    void Move()
    {
        float speed01 = 0f; // 애니메이터용 속도 0~1
        if (_agent != null && _agent.speed > 0.01f)
        {
            speed01 = _agent.velocity.magnitude / _agent.speed;
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
        if(GetDistanceToPlayer() > fovRange)
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

  
}
