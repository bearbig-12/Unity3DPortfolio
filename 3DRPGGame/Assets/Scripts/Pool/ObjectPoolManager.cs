using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀 매니저 (싱글톤)
///
/// 오브젝트 풀링이란?
/// - Instantiate/Destroy 대신 객체를 재사용하는 최적화 기법
/// - GC(가비지 컬렉션) 부담을 줄여 프레임 드랍 방지
///
/// 사용법:
/// 1. Inspector에서 Pool Configs 설정 (키, 프리팹, 초기 개수)
/// 2. 코드에서 Get/Return 호출
///    - 꺼내기: ObjectPoolManager.Instance.Get("fireball", position, rotation)
///    - 반환: ObjectPoolManager.Instance.Return("fireball", gameObject)
public class ObjectPoolManager : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스
    /// 어디서든 ObjectPoolManager.Instance로 접근 가능
    /// </summary>
    public static ObjectPoolManager Instance { get; private set; }

    /// <summary>
    /// 풀 설정 클래스
    /// Inspector에서 풀 정보를 설정할 때 사용
    /// </summary>
    [System.Serializable]
    public class PoolConfig
    {
        public string key;           // 풀 식별자 (예: "fireball")
        public GameObject prefab;    // 원본 프리팹
        public int initialSize = 5;  // 초기 생성 개수
    }

    [Header("풀 설정 (Inspector에서 설정)")]
    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();

    // 풀 저장소: Key로 해당 풀의 대기 객체들(Queue)에 접근
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

    // 프리팹 저장소: 풀이 비었을 때 새 객체 생성용
    private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

    // 비활성화된 객체들을 정리하는 컨테이너
    private Transform _poolContainer;

    #region 초기화

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환해도 유지
            InitializePools();
        }
        else
        {
            Destroy(gameObject);  // 중복 방지
        }
    }

    /// <summary>
    /// 모든 풀을 초기화한다.
    /// Inspector에서 설정한 풀들을 미리 생성해둔다.
    /// </summary>
    private void InitializePools()
    {
        // 풀 컨테이너 생성 (Hierarchy 정리용)
        _poolContainer = new GameObject("PoolContainer").transform;
        _poolContainer.SetParent(transform);

        // 설정된 모든 풀 생성
        foreach (var config in poolConfigs)
        {
            if (config.prefab == null || string.IsNullOrEmpty(config.key))
                continue;

            CreatePool(config.key, config.prefab, config.initialSize);
        }
    }

    #endregion

    #region 풀 생성


    public void CreatePool(string key, GameObject prefab, int initialSize)
    {
        // 이미 존재하는 풀이면 무시
        if (_pools.ContainsKey(key))
            return;

        // 프리팹 저장
        _prefabs[key] = prefab;

        // 빈 큐 생성
        _pools[key] = new Queue<GameObject>();

        // 초기 객체들 미리 생성
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject(key);
            obj.SetActive(false);        // 비활성화 (숨김)
            _pools[key].Enqueue(obj);    // 큐에 넣기
        }
    }


    private GameObject CreateNewObject(string key)
    {
        if (!_prefabs.ContainsKey(key))
            return null;

        // 프리팹으로 객체 생성 (PoolContainer 아래에)
        GameObject obj = Instantiate(_prefabs[key], _poolContainer);

        // PooledObject 컴포넌트 확인/추가
        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = obj.AddComponent<PooledObject>();

        // 풀 키 설정 (나중에 반환할 때 어느 풀인지 알기 위해)
        pooled.SetPoolKey(key);

        return obj;
    }

    #endregion

    #region Get (꺼내기)


    public GameObject Get(string key)
    {
        // 풀이 존재하는지 확인
        if (!_pools.ContainsKey(key))
        {
            Debug.LogWarning($"Pool '{key}' does not exist.");
            return null;
        }

        GameObject obj;

        // 풀에 대기 중인 객체가 있으면 꺼냄
        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
        }
        else
        {
            // 풀이 비었으면 새로 생성 (자동 확장)
            obj = CreateNewObject(key);
        }

        if (obj == null)
            return null;

        // 활성화 및 월드로 이동
        obj.SetActive(true);
        obj.transform.SetParent(null);

        // IPoolable 구현했으면 OnSpawn 호출 (초기화)
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
            poolable.OnSpawn();

        return obj;
    }

    /// <summary>
    /// 풀에서 객체를 꺼내고 위치/회전을 설정한다.
    /// </summary>
    public GameObject Get(string key, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Get(key);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    #endregion

    #region Return (반환)

    /// <summary>
    /// 객체를 풀에 반환한다.
    /// </summary>
    /// <param name="key">풀 식별자</param>
    /// <param name="obj">반환할 객체</param>
    public void Return(string key, GameObject obj)
    {
        if (obj == null)
            return;

        // 풀이 없으면 그냥 파괴
        if (!_pools.ContainsKey(key))
        {
            Destroy(obj);
            return;
        }

        // IPoolable 구현했으면 OnDespawn 호출 (정리)
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
            poolable.OnDespawn();

        // 비활성화 및 컨테이너로 이동
        obj.SetActive(false);
        obj.transform.SetParent(_poolContainer);

        // 큐에 다시 넣기
        _pools[key].Enqueue(obj);
    }

    #endregion

    #region 런타임 등록

    /// <summary>
    /// 런타임에 새로운 풀을 등록한다.
    /// Inspector에서 미리 설정하지 않은 풀을 코드에서 추가할 때 사용.
    ///
    /// 예시:
    /// ObjectPoolManager.Instance.RegisterPrefab("damagePopup", popupPrefab, 20);
    /// </summary>
    public void RegisterPrefab(string key, GameObject prefab, int initialSize = 5)
    {
        if (_prefabs.ContainsKey(key))
            return;

        CreatePool(key, prefab, initialSize);
    }

    #endregion
}
