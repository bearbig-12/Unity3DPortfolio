using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _isLaunched = false;
    private PlayerMovement _player;
    private int _damage;

    public GameObject explosionPrefab;

    public float explosionRadius = 3.0f;
    public float fireBallSpeed = 12f;
    public float upwardBoost = 4f;

    public LayerMask obstacleMask;   // Wall/Ground


    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _player = FindObjectOfType<PlayerMovement>();

    }
    public void Launch(Vector3 targetPos, float lifeTime, int damage)
    {
       if(_rb == null) return;

        _damage = damage;



        Vector3 dir = (targetPos - transform.position).normalized;
        Vector3 velocity = dir * fireBallSpeed + Vector3.up * upwardBoost;
        _rb.velocity = velocity;
        _isLaunched = true;

        Destroy(gameObject,  lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_isLaunched) return;

        _isLaunched = false;

        int hitLayer = collision.collider.gameObject.layer;

        bool isPlayer = hitLayer == LayerMask.NameToLayer("Player");
        bool isObstacle = (hitLayer == LayerMask.NameToLayer("Ground") || hitLayer == LayerMask.NameToLayer("Wall"));

        if (isPlayer || isObstacle)
        {
            Vector3 hitPoint = transform.position;
            SpawnExplosion(hitPoint);
            ApplyExplosionDamage(hitPoint);
        }

        Destroy(gameObject, 0.2f);
    }

    private void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab == null) return;

        GameObject vfx = Instantiate(explosionPrefab, position, Quaternion.identity);

        var ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(vfx, 2f);
        }
    }


    private void ApplyExplosionDamage(Vector3 position)
    {
        if (_player == null) return;

        //Vector3 origin = position + Vector3.up * 0.5f;
        //Vector3 target = _player.position + Vector3.up * 0.5f;

        //RaycastHit hit;
        //if (Physics.Linecast(origin, target, out hit, obstacleMask, QueryTriggerInteraction.Ignore))
        //{
        //    return;
        //}

        //Vector3 origin = position + Vector3.up * 0.5f;
        //Vector3 dir = (_player.position + Vector3.up * 0.5f) - origin;
        Vector3 playerPos = _player.transform.position;
        Vector3 dir = playerPos - position;
        float dist = Vector3.Distance(position, playerPos);
        
        if (dist > explosionRadius) return;

        if (Physics.Raycast(position, dir.normalized, dist, obstacleMask))
            return;

        _player.TakeDamage(_damage);
    }


}
