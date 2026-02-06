using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas != null)
            _canvas.enabled = false;

        if (fadePanel != null)
            fadePanel.alpha = 0f;
    }

    public void Show(string tip = "")
    {
        if (_canvas != null)
            _canvas.enabled = true;

        if (tipText != null && !string.IsNullOrEmpty(tip))
            tipText.text = tip;

        SetProgress(0f);
    }

    public void Hide()
    {
        if (_canvas != null)
            _canvas.enabled = false;
    }

    public void SetProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }

    public float FadeDuration => fadeDuration;
    public CanvasGroup FadePanel => fadePanel;
}
