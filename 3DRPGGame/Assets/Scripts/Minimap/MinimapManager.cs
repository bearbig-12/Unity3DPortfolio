using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance { get; private set; }

    [Header("Minimap Camera")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private float cameraHeight = 50f;
    [SerializeField] private float defaultZoom = 30f;
    [SerializeField] private float minZoom = 15f;
    [SerializeField] private float maxZoom = 60f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Player Tracking")]
    private Transform _playerTransform;

    [Header("Marker Prefabs")]
    [SerializeField] private GameObject playerMarkerPrefab;
    [SerializeField] private GameObject enemyMarkerPrefab;
    [SerializeField] private GameObject npcMarkerPrefab;

    [Header("Marker Settings")]
    [SerializeField] private float markerHeight = 45f;
    [SerializeField] private float markerScale = 2f;

    private float _currentZoom;
    private GameObject _playerMarker;
    private Dictionary<Transform, GameObject> _enemyMarkers = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, GameObject> _npcMarkers = new Dictionary<Transform, GameObject>();
    private readonly List<Transform> _toRemove = new List<Transform>();
    private float _enemyScanTimer;
    private const float EnemyScanInterval = 1.5f;

    private const string EnemyMarkerPoolKey = "minimap_enemy";
    private const string NpcMarkerPoolKey   = "minimap_npc";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _currentZoom = defaultZoom;
    }

    private void Start()
    {
        FindPlayer();
        SetupMinimapCamera();
        RegisterMarkerPools();
        CreatePlayerMarker();
        RefreshAllMarkers();
    }

    private void RegisterMarkerPools()
    {
        if (ObjectPoolManager.Instance == null) return;

        if (enemyMarkerPrefab != null)
            ObjectPoolManager.Instance.RegisterPrefab(EnemyMarkerPoolKey, enemyMarkerPrefab, 5);
        if (npcMarkerPrefab != null)
            ObjectPoolManager.Instance.RegisterPrefab(NpcMarkerPoolKey, npcMarkerPrefab, 3);
    }

    private void Update()
    {
        HandleZoomInput();
        RefreshEnemyMarkers();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
        UpdatePlayerMarker();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    private void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            minimapCamera = GetComponentInChildren<Camera>();
        }

        if (minimapCamera != null)
        {
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = _currentZoom;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    private void HandleZoomInput()
    {
        // M키로 줌 3단계 순환: minZoom → defaultZoom → maxZoom → minZoom
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_currentZoom == minZoom)
                _currentZoom = defaultZoom;
            else if (_currentZoom == defaultZoom)
                _currentZoom = maxZoom;
            else
                _currentZoom = minZoom;
        }

        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = Mathf.Lerp(
                minimapCamera.orthographicSize,
                _currentZoom,
                Time.deltaTime * 5f
            );
        }
    }

    private void UpdateCameraPosition()
    {
        if (minimapCamera == null || _playerTransform == null) return;

        Vector3 newPos = _playerTransform.position;
        newPos.y = cameraHeight;
        minimapCamera.transform.position = newPos;
    }

    private void CreatePlayerMarker()
    {
        if (_playerTransform == null) return;

        if (playerMarkerPrefab != null)
        {
            _playerMarker = Instantiate(playerMarkerPrefab, _playerTransform);
            SetMarkerLayer(_playerMarker);
        }
        else
        {
            _playerMarker = CreateDefaultMarker(Color.green, "PlayerMarker");
            _playerMarker.transform.SetParent(_playerTransform);
        }

        _playerMarker.transform.localPosition = new Vector3(0f, markerHeight, 0f);
        _playerMarker.transform.localScale = Vector3.one * markerScale;
    }

    private void UpdatePlayerMarker()
    {
        if (_playerMarker == null || _playerTransform == null) return;

        // 위치는 자식이라 자동 추적, 방향만 업데이트
        Vector3 forward = _playerTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f)
        {
            _playerMarker.transform.rotation = Quaternion.LookRotation(Vector3.down, forward);
        }
    }

    public void RefreshAllMarkers()
    {
        RefreshEnemyMarkers();
        RefreshNPCMarkers();
    }

    private void RefreshEnemyMarkers()
    {
        // 죽은 적 마커 제거
        _toRemove.Clear();
        foreach (var (enemyTransform, marker) in _enemyMarkers)
        {
            if (enemyTransform == null || !enemyTransform.gameObject.activeInHierarchy)
            {
                DespawnMarker(EnemyMarkerPoolKey, marker);
                _toRemove.Add(enemyTransform);
            }
            else
            {
                // EnemyAI 죽음 체크
                EnemyAI enemy = enemyTransform.GetComponent<EnemyAI>();
                if (enemy != null && enemy.isDead)
                {
                    DespawnMarker(EnemyMarkerPoolKey, marker);
                    _toRemove.Add(enemyTransform);
                }
            }
        }
        foreach (var t in _toRemove)
        {
            _enemyMarkers.Remove(t);
        }

        // 새로운 적 등록 - 매 프레임 FindObjectsOfType 방지
        _enemyScanTimer += Time.deltaTime;
        if (_enemyScanTimer < EnemyScanInterval) return;
        _enemyScanTimer = 0f;

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            if (enemy.isDead) continue;
            if (_enemyMarkers.ContainsKey(enemy.transform)) continue;

            RegisterEnemyMarker(enemy.transform);
        }
    }

    private void RefreshNPCMarkers()
    {
        // 기존 NPC 마커는 정적이므로 한 번만 등록
        if (_npcMarkers.Count > 0) return;

        // "NPC" 태그를 가진 오브젝트 찾기
        GameObject[] npcObjects = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var npcObj in npcObjects)
        {
            if (_npcMarkers.ContainsKey(npcObj.transform)) continue;
            RegisterNPCMarker(npcObj.transform);
        }
    }

    private void RegisterEnemyMarker(Transform target)
    {
        GameObject marker = SpawnMarker(enemyMarkerPrefab, EnemyMarkerPoolKey, Color.red, "EnemyMarker");
        marker.transform.SetParent(target);
        marker.transform.localPosition = new Vector3(0f, markerHeight, 0f);
        SetMarkerLayer(marker);
        marker.transform.localScale = Vector3.one * markerScale * 0.8f;
        _enemyMarkers[target] = marker;
    }

    private void RegisterNPCMarker(Transform target)
    {
        GameObject marker = SpawnMarker(npcMarkerPrefab, NpcMarkerPoolKey, Color.yellow, "NPCMarker");
        marker.transform.SetParent(target);
        marker.transform.localPosition = new Vector3(0f, markerHeight, 0f);
        SetMarkerLayer(marker);
        marker.transform.localScale = Vector3.one * markerScale * 0.8f;
        _npcMarkers[target] = marker;
    }

    private GameObject SpawnMarker(GameObject prefab, string poolKey, Color fallbackColor, string fallbackName)
    {
        if (prefab != null && ObjectPoolManager.Instance != null)
        {
            GameObject obj = ObjectPoolManager.Instance.Get(poolKey);
            if (obj != null)
                return obj;
        }
        return CreateDefaultMarker(fallbackColor, fallbackName);
    }

    private void DespawnMarker(string poolKey, GameObject marker)
    {
        if (marker == null) return;
        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.Return(poolKey, marker);
        else
            Destroy(marker);
    }

    private GameObject CreateDefaultMarker(Color color, string name)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.name = name;
        marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Collider 제거
        Collider col = marker.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // 머티리얼 설정
        Renderer rend = marker.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = color;
            rend.material = mat;
        }

        return marker;
    }

    private void SetMarkerLayer(GameObject marker)
    {
        int minimapLayer = LayerMask.NameToLayer("Minimap");
        if (minimapLayer != -1)
        {
            marker.layer = minimapLayer;
            foreach (Transform child in marker.transform)
            {
                child.gameObject.layer = minimapLayer;
            }
        }
    }

    // 외부에서 마커 등록/해제
    public void RegisterMarker(Transform target, MarkerType type)
    {
        switch (type)
        {
            case MarkerType.Enemy:
                if (!_enemyMarkers.ContainsKey(target))
                    RegisterEnemyMarker(target);
                break;
            case MarkerType.NPC:
                if (!_npcMarkers.ContainsKey(target))
                    RegisterNPCMarker(target);
                break;
        }
    }

    public void UnregisterMarker(Transform target, MarkerType type)
    {
        Dictionary<Transform, GameObject> dict = null;
        string poolKey = null;

        switch (type)
        {
            case MarkerType.Enemy:
                dict = _enemyMarkers;
                poolKey = EnemyMarkerPoolKey;
                break;
            case MarkerType.NPC:
                dict = _npcMarkers;
                poolKey = NpcMarkerPoolKey;
                break;
        }

        if (dict != null && dict.TryGetValue(target, out GameObject marker))
        {
            DespawnMarker(poolKey, marker);
            dict.Remove(target);
        }
    }

    public void SetZoom(float zoom)
    {
        _currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public float GetCurrentZoom()
    {
        return _currentZoom;
    }
}

public enum MarkerType
{
    Player,
    Enemy,
    NPC
}
