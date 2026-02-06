using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour, IPoolable
{
    private Rigidbody _rb;
    private bool _isLaunched = false;
    private PlayerMovement _player;
    private int _damage;
    private Coroutine _despawnCoroutine;
    private PooledObject _pooledObject;

    public GameObject explosionPrefab;

    public float explosionRadius = 3.0f;
    public float fireBallSpeed = 12f;
    public float upwardBoost = 4f;

    public LayerMask obstacleMask;   // Wall/Ground


    // player cast��
    [SerializeField] private bool damagePlayer = true;
    [SerializeField] private LayerMask enemyMask;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _player = FindObjectOfType<PlayerMovement>();
        _pooledObject = GetComponent<PooledObject>();
    }

    public void OnSpawn()
    {
        _isLaunched = false;
        _damage = 0;
        damagePlayer = true;
        if (_rb != null)
            _rb.velocity = Vector3.zero;
    }

    public void OnDespawn()
    {
        if (_despawnCoroutine != null)
        {
            StopCoroutine(_despawnCoroutine);
            _despawnCoroutine = null;
        }
        _isLaunched = false;
        if (_rb != null)
            _rb.velocity = Vector3.zero;
    }

    public void SetPlayerOwner(LayerMask enemyLayer)
    {
        damagePlayer = false;
        enemyMask = enemyLayer;
    }


    public void Launch(Vector3 targetPos, float lifeTime, int damage)
    {
       if(_rb == null) return;

        _damage = damage;

        Vector3 dir = (targetPos - transform.position).normalized;
        Vector3 velocity = dir * fireBallSpeed + Vector3.up * upwardBoost;
        _rb.velocity = velocity;
        _isLaunched = true;

        _despawnCoroutine = StartCoroutine(DespawnAfterDelay(lifeTime));
    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_pooledObject != null)
        {
            _pooledObject.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_isLaunched) return;

        _isLaunched = false;

        int hitLayer = collision.collider.gameObject.layer;

        bool isPlayer = hitLayer == LayerMask.NameToLayer("Player");
        bool isObstacle = (hitLayer == LayerMask.NameToLayer("Ground") || hitLayer == LayerMask.NameToLayer("Wall"));
        bool isEnemy = collision.collider.GetComponentInParent<EnemyAI>() != null;

        if (isPlayer || isObstacle || isEnemy)
        {
            Vector3 hitPoint = transform.position;
            SpawnExplosion(hitPoint);
            ApplyExplosionDamage(hitPoint);
        }

        if (_despawnCoroutine != null)
        {
            StopCoroutine(_despawnCoroutine);
            _despawnCoroutine = null;
        }
        _despawnCoroutine = StartCoroutine(DespawnAfterDelay(0.2f));
    }

    private void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab == null) return;

        GameObject vfx = null;

        // 풀에서 가져오기 (PooledVFX가 자동으로 반환 처리함)
        if (ObjectPoolManager.Instance != null)
        {
            vfx = ObjectPoolManager.Instance.Get("explosion", position, Quaternion.identity);
        }

        // 풀이 없으면 기존 방식
        if (vfx == null)
        {
            vfx = Instantiate(explosionPrefab, position, Quaternion.identity);

            // 풀 없이 생성된 경우 자동 파괴
            var ps = vfx.GetComponent<ParticleSystem>();
            float duration = (ps != null) ? ps.main.duration + ps.main.startLifetime.constantMax : 2f;
            Destroy(vfx, duration);
        }
    }


    private void ApplyExplosionDamage(Vector3 position)
    {
        if (damagePlayer)
        {
            // 보스가 쏜 파이어볼 - 플레이어에게 데미지
            if (_player == null) return;

            Vector3 playerPos = _player.transform.position;
            Vector3 dir = playerPos - position;
            float dist = Vector3.Distance(position, playerPos);

            // 폭발 범위 밖이면 무시
            if (dist > explosionRadius) return;

            // 장애물에 막혀있으면 무시
            if (Physics.Raycast(position, dir.normalized, dist, obstacleMask))
                return;

            // 플레이어에게 데미지 적용
            _player.TakeDamage(_damage);
        }
        else
        {
            // 플레이어가 쏜 파이어볼 - 적에게 데미지
            Collider[] hits = Physics.OverlapSphere(position, explosionRadius, enemyMask);
            foreach (var h in hits)
            {
                EnemyAI enemy = h.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(_damage);
                }
            }
        }
    }

}
