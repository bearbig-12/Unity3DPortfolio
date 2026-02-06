using System.Collections;
using UnityEngine;

/// <summary>
/// 풀링되는 VFX(파티클) 전용 컴포넌트
///
/// 파티클 재생이 끝나면 자동으로 풀에 반환한다.
/// FireBall처럼 다른 객체에 의존하지 않고 스스로 반환을 처리한다.
///
/// 사용법:
/// 1. Explosion 프리팹에 이 스크립트 추가
/// 2. ObjectPoolManager에 "explosion" 키로 등록
/// </summary>
public class PooledVFX : MonoBehaviour, IPoolable
{
    private ParticleSystem _particleSystem;
    private PooledObject _pooledObject;
    private Coroutine _returnCoroutine;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _pooledObject = GetComponent<PooledObject>();
    }

    public void OnSpawn()
    {
        // 파티클 재생
        if (_particleSystem != null)
        {
            _particleSystem.Clear();
            _particleSystem.Play();
        }

        // 자동 반환 시작
        float duration = GetDuration();
        _returnCoroutine = StartCoroutine(ReturnAfterDelay(duration));
    }

    public void OnDespawn()
    {
        // 코루틴 정지
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }

        // 파티클 정지
        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private float GetDuration()
    {
        if (_particleSystem == null)
            return 2f;

        var main = _particleSystem.main;
        return main.duration + main.startLifetime.constantMax;
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_pooledObject != null)
        {
            _pooledObject.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
