
using UnityEngine;

public class BossAoeHitbox : MonoBehaviour
{
    public BossAI boss;
    public ParticleSystem aoeVfx;
    public float tickInterval = 1f;

    private bool _active;
    private float _nextTickTime;

    private void Reset()
    {
        if (boss == null)
        {
            boss = GetComponentInParent<BossAI>();
        }
        if (aoeVfx == null)
        {
            aoeVfx = GetComponentInChildren<ParticleSystem>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            _nextTickTime = 0f;
        }
    }

    private void TryDamage(Collider other)
    {
        if (!_active) return;

        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        if (Time.time >= _nextTickTime)
        {
            if (boss != null)
            {
                player.TakeDamage(boss.currentAttackDamage);
            }
            _nextTickTime = Time.time + tickInterval;
        }
    }

    public void SetActive(bool value)
    {
        Debug.Log("AOE VFX Active: " + value);
        _active = value;

        if (aoeVfx != null)
        {
            if (_active) aoeVfx.Play();
            else aoeVfx.Stop(true,
    ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (!_active)
        {
            _nextTickTime = 0f;
        }
    }

}
