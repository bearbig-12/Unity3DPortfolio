using UnityEngine;

/// <summary>
/// 풀 소속 정보를 저장하는 컴포넌트
///
/// 역할:
/// 1. 이 객체가 어느 풀에서 왔는지 기억한다 (PoolKey)
/// 2. 풀에 반환하는 기능을 제공한다 (ReturnToPool)
///
/// ObjectPoolManager가 객체 생성 시 자동으로 추가한다.
/// 프리팹에 미리 붙여둘 필요 없음.
/// </summary>
public class PooledObject : MonoBehaviour
{
    /// <summary>
    /// 이 객체가 속한 풀의 키 (예: "fireball", "explosion")
    /// 외부에서 읽기만 가능하고, 수정은 SetPoolKey()로만 가능하다.
    /// </summary>
    public string PoolKey { get; private set; }

    /// <summary>
    /// 풀 키를 설정한다.
    /// ObjectPoolManager가 객체 생성 시 호출한다.
    /// </summary>
    /// <param name="key">풀 식별자 (예: "fireball")</param>
    public void SetPoolKey(string key)
    {
        PoolKey = key;
    }

    /// <summary>
    /// 이 객체를 풀에 반환한다.
    ///
    /// 동작:
    /// 1. ObjectPoolManager가 존재하고 PoolKey가 설정되어 있으면 풀에 반환
    /// 2. 그렇지 않으면 안전하게 Destroy (풀 시스템 없이도 동작하도록)
    ///
    /// 사용 예시:
    /// _pooledObject.ReturnToPool();
    /// </summary>
    public void ReturnToPool()
    {
        // 풀 매니저가 존재하고, 풀 키가 설정되어 있으면 풀에 반환
        if (ObjectPoolManager.Instance != null && !string.IsNullOrEmpty(PoolKey))
        {
            ObjectPoolManager.Instance.Return(PoolKey, gameObject);
        }
        else
        {
            // 풀이 없으면 그냥 파괴 (안전장치)
            Destroy(gameObject);
        }
    }
}
