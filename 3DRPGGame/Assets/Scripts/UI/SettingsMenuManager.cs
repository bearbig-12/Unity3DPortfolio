using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// ESC 키로 열리는 설정창.
/// 게임 저장/로드, 화면 설정, 볼륨 설정을 담당합니다.
/// </summary>
public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    // ─────────────────────────────────────────────────────────────────────
    // Inspector 필드
    // ─────────────────────────────────────────────────────────────────────

    [Header("UI - 메인 패널")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button loadBtn;
    [SerializeField] private Button quitBtn;
    // TODO: 메인 메뉴 씬 완성 후 연결
    // [SerializeField] private Button mainMenuBtn;
    [SerializeField] private TMP_Text feedbackText;

    [Header("UI - 화면 설정")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("UI - 볼륨 설정")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("References")]
    [SerializeField] private SaveManager saveManager;
    /// <summary>
    /// AudioMixer를 연결하면 BGM/SFX 볼륨을 개별 조절합니다.
    /// AudioMixer에 "BGMVolume", "SFXVolume" 파라미터를 Expose 해주세요.
    /// 없으면 마스터 볼륨(AudioListener)만 동작합니다.
    /// </summary>
    [SerializeField] private AudioMixer audioMixer;

    // ─────────────────────────────────────────────────────────────────────
    // 상수
    // ─────────────────────────────────────────────────────────────────────

    // 해상도 프리셋
    private static readonly (int w, int h)[] ResolutionPresets =
    {
        (1280,  720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
    };

    // PlayerPrefs 키
    private const string KEY_MASTER     = "Vol_Master";
    private const string KEY_BGM        = "Vol_BGM";
    private const string KEY_SFX        = "Vol_SFX";
    private const string KEY_FULLSCREEN = "Screen_Fullscreen";
    private const string KEY_RESOLUTION = "Screen_Resolution";

    // ─────────────────────────────────────────────────────────────────────
    // 내부 변수
    // ─────────────────────────────────────────────────────────────────────

    private SkillUIController _skillUI;
    private ShopKepper[]      _shopKeepers;
    private float             _feedbackTimer;

    // ─────────────────────────────────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        _skillUI     = FindObjectOfType<SkillUIController>();
        _shopKeepers = FindObjectsOfType<ShopKepper>();

        if (saveManager == null)
            saveManager = FindObjectOfType<SaveManager>();

        // 버튼 이벤트
        if (continueBtn != null) continueBtn.onClick.AddListener(CloseSettings);
        if (saveBtn     != null) saveBtn.onClick.AddListener(SaveGame);
        if (loadBtn     != null) loadBtn.onClick.AddListener(LoadGame);
        if (quitBtn     != null) quitBtn.onClick.AddListener(QuitGame);

        // 화면 설정 초기화
        InitResolutionDropdown();
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        // 볼륨 슬라이더 초기화
        InitVolumeSliders();

        // 초기 상태
        if (feedbackText  != null) feedbackText.gameObject.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        IsPaused = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 매 프레임
    // ─────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                CloseSettings();
            else if (!IsAnyUIOpen())
                OpenSettings();
        }

        if (_feedbackTimer > 0f)
        {
            _feedbackTimer -= Time.unscaledDeltaTime;
            if (_feedbackTimer <= 0f && feedbackText != null)
                feedbackText.gameObject.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 설정창 열기 / 닫기
    // ─────────────────────────────────────────────────────────────────────

    public void OpenSettings()
    {
        IsPaused       = true;
        Time.timeScale = 0f;

        if (settingsPanel != null) settingsPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void CloseSettings()
    {
        IsPaused       = false;
        Time.timeScale = 1f;

        if (settingsPanel != null) settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 게임 버튼 동작
    // ─────────────────────────────────────────────────────────────────────

    public void SaveGame()
    {
        if (saveManager == null) return;
        saveManager.Save();
        ShowFeedback("Game Saved.");
    }

    public void LoadGame()
    {
        if (saveManager == null) return;

        IsPaused       = false;
        Time.timeScale = 1f;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        saveManager.Load();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────
    // 화면 설정
    // ─────────────────────────────────────────────────────────────────────

    private void InitResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        foreach (var res in ResolutionPresets)
            options.Add($"{res.w} x {res.h}");

        resolutionDropdown.AddOptions(options);

        // 저장된 인덱스 불러오기 (기본값: 1920x1080 = index 1)
        int savedIndex = PlayerPrefs.GetInt(KEY_RESOLUTION, 1);
        resolutionDropdown.SetValueWithoutNotify(savedIndex);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        // 시작 시 적용
        ApplyResolution(savedIndex);
    }

    private void OnResolutionChanged(int index)
    {
        PlayerPrefs.SetInt(KEY_RESOLUTION, index);
        ApplyResolution(index);
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= ResolutionPresets.Length) return;

        var (w, h) = ResolutionPresets[index];
        bool isFullscreen = fullscreenToggle != null && fullscreenToggle.isOn;
        Screen.SetResolution(w, h, isFullscreen);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        PlayerPrefs.SetInt(KEY_FULLSCREEN, isFullscreen ? 1 : 0);
        Screen.fullScreen = isFullscreen;
    }

    private void InitFullscreenToggle()
    {
        if (fullscreenToggle == null) return;
        bool saved = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.SetIsOnWithoutNotify(saved);
        Screen.fullScreen = saved;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 볼륨 설정
    // ─────────────────────────────────────────────────────────────────────

    private void InitVolumeSliders()
    {
        // 마스터 볼륨
        if (masterVolumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
            masterVolumeSlider.SetValueWithoutNotify(saved);
            ApplyMasterVolume(saved);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // BGM 볼륨
        if (bgmVolumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat(KEY_BGM, 1f);
            bgmVolumeSlider.SetValueWithoutNotify(saved);
            ApplyMixerVolume("BGMVolume", saved);
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        // SFX 볼륨
        if (sfxVolumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat(KEY_SFX, 1f);
            sfxVolumeSlider.SetValueWithoutNotify(saved);
            ApplyMixerVolume("SFXVolume", saved);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_MASTER, value);
        ApplyMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_BGM, value);
        ApplyMixerVolume("BGMVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_SFX, value);
        ApplyMixerVolume("SFXVolume", value);
    }

    private void ApplyMasterVolume(float value)
    {
        // AudioListener.volume: 0~1 범위로 전체 볼륨 조절
        AudioListener.volume = value;
    }

    /// <summary>
    /// AudioMixer 파라미터에 0~1 값을 dB로 변환하여 적용합니다.
    /// AudioMixer가 없으면 아무 동작도 하지 않습니다.
    /// </summary>
    private void ApplyMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null) return;

        // 0이면 무음(-80dB), 그 외 log10 변환
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat(parameterName, dB);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 내부 유틸
    // ─────────────────────────────────────────────────────────────────────

    private bool IsAnyUIOpen()
    {
        if (InventorySystem.instance != null && InventorySystem.instance.IsOpen)
            return true;

        if (_skillUI != null && _skillUI.IsOpen())
            return true;

        if (_shopKeepers != null)
        {
            foreach (var shop in _shopKeepers)
                if (shop != null && shop.isTalking) return true;
        }

        return false;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        _feedbackTimer = 2f;
    }
}
