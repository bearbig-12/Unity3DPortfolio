using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour, IPoolable
{
    [Header("Animation Settings")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float scalePunch = 1.2f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.2f);

    private TextMeshProUGUI _text;
    private CanvasGroup _canvasGroup;
    private PooledObject _pooledObject;

    private float _timer;
    private Vector3 _startScale;
    private Camera _mainCamera;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _pooledObject = GetComponent<PooledObject>();
        _startScale = transform.localScale;
        _mainCamera = Camera.main;
    }

    public void OnSpawn()
    {
        _timer = 0f;
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;
        transform.localScale = _startScale * scalePunch;
    }

    public void OnDespawn()
    {
        _timer = 0f;
    }

    public void Setup(int amount, DamageType type)
    {
        if (_text == null) return;

        string prefix = type == DamageType.Heal ? "+" : "";
        _text.text = prefix + amount.ToString();

        switch (type)
        {
            case DamageType.Critical:
                _text.color = criticalColor;
                _text.fontSize = 48;
                break;
            case DamageType.Heal:
                _text.color = healColor;
                _text.fontSize = 36;
                break;
            default:
                _text.color = normalColor;
                _text.fontSize = 36;
                break;
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Scale animation (punch -> normal)
        float scaleT = Mathf.Clamp01(_timer / 0.2f);
        transform.localScale = Vector3.Lerp(_startScale * scalePunch, _startScale, scaleT);

        // Fade out
        float fadeT = Mathf.Clamp01(_timer / fadeDuration);
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f - fadeT;

        // Billboard - face camera
        if (_mainCamera != null)
            transform.forward = _mainCamera.transform.forward;

        // Return to pool when done
        if (_timer >= fadeDuration)
        {
            if (_pooledObject != null)
                _pooledObject.ReturnToPool();
            else
                Destroy(gameObject);
        }
    }
}
