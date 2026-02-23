using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니맵 초기 설정을 도와주는 에디터 유틸리티 컴포넌트
/// 이 컴포넌트를 빈 게임오브젝트에 추가하고 SetupMinimap() 메서드를 실행하면
/// 미니맵에 필요한 기본 구조를 자동으로 생성합니다.
/// </summary>
public class MinimapSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [SerializeField] private int renderTextureSize = 512;
    [SerializeField] private float minimapUISize = 200f;
    [SerializeField] private Vector2 minimapUIPosition = new Vector2(-20f, -20f);

    [Header("Created References (자동 생성됨)")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RenderTexture minimapRenderTexture;
    [SerializeField] private RawImage minimapRawImage;

    /// <summary>
    /// 컨텍스트 메뉴에서 실행하거나 에디터 버튼으로 호출
    /// </summary>
    [ContextMenu("Setup Minimap")]
    public void SetupMinimap()
    {
        CreateMinimapLayer();
        CreateMinimapCamera();
        CreateMinimapUI();
        CreateMinimapManager();

        Debug.Log("[MinimapSetup] 미니맵 설정 완료!");
        Debug.Log("추가 설정이 필요합니다:");
        Debug.Log("1. Edit > Project Settings > Tags and Layers에서 'Minimap' 레이어 추가");
        Debug.Log("2. MinimapManager의 마커 프리팹 설정 (선택사항)");
        Debug.Log("3. 미니맵 카메라의 Culling Mask에서 불필요한 레이어 제외");
    }

    private void CreateMinimapLayer()
    {
        int layer = LayerMask.NameToLayer("Minimap");
        if (layer == -1)
        {
            Debug.LogWarning("[MinimapSetup] 'Minimap' 레이어를 수동으로 추가해주세요: Edit > Project Settings > Tags and Layers");
        }
    }

    private void CreateMinimapCamera()
    {
        // RenderTexture 생성
        minimapRenderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 16);
        minimapRenderTexture.name = "MinimapRenderTexture";

        // 카메라 오브젝트 생성
        GameObject camObj = new GameObject("MinimapCamera");
        camObj.transform.SetParent(transform);
        camObj.transform.localPosition = new Vector3(0f, 50f, 0f);
        camObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        minimapCamera = camObj.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 30f;
        minimapCamera.targetTexture = minimapRenderTexture;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        minimapCamera.depth = -1; // 메인 카메라보다 먼저 렌더링

        // Minimap 레이어 설정
        int minimapLayer = LayerMask.NameToLayer("Minimap");
        if (minimapLayer != -1)
        {
            // 기본 레이어들 + Minimap 레이어
            minimapCamera.cullingMask = (1 << 0) | (1 << minimapLayer); // Default + Minimap
        }

        Debug.Log("[MinimapSetup] 미니맵 카메라 생성 완료");
    }

    private void CreateMinimapUI()
    {
        // Canvas 찾기 또는 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 미니맵 컨테이너 생성
        GameObject minimapContainer = new GameObject("MinimapContainer");
        minimapContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = minimapContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1f, 1f);
        containerRect.anchorMax = new Vector2(1f, 1f);
        containerRect.pivot = new Vector2(1f, 1f);
        containerRect.anchoredPosition = minimapUIPosition;
        containerRect.sizeDelta = new Vector2(minimapUISize, minimapUISize);

        // 배경 (원형 마스크용)
        GameObject maskObj = new GameObject("MinimapMask");
        maskObj.transform.SetParent(minimapContainer.transform, false);

        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;

        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.color = Color.white;
        // 원형 스프라이트가 필요하면 나중에 설정

        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // RawImage (미니맵 텍스처 표시)
        GameObject rawImageObj = new GameObject("MinimapImage");
        rawImageObj.transform.SetParent(maskObj.transform, false);

        RectTransform rawImageRect = rawImageObj.AddComponent<RectTransform>();
        rawImageRect.anchorMin = Vector2.zero;
        rawImageRect.anchorMax = Vector2.one;
        rawImageRect.offsetMin = Vector2.zero;
        rawImageRect.offsetMax = Vector2.zero;

        minimapRawImage = rawImageObj.AddComponent<RawImage>();
        minimapRawImage.texture = minimapRenderTexture;

        // 테두리 (프레임)
        GameObject frameObj = new GameObject("MinimapFrame");
        frameObj.transform.SetParent(minimapContainer.transform, false);

        RectTransform frameRect = frameObj.AddComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = new Vector2(-5f, -5f);
        frameRect.offsetMax = new Vector2(5f, 5f);

        Image frameImage = frameObj.AddComponent<Image>();
        frameImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        frameImage.raycastTarget = false;

        // 프레임을 뒤로 보내기
        frameObj.transform.SetAsFirstSibling();

        Debug.Log("[MinimapSetup] 미니맵 UI 생성 완료");
    }

    private void CreateMinimapManager()
    {
        // 기존 MinimapManager 확인
        MinimapManager existingManager = FindObjectOfType<MinimapManager>();
        if (existingManager != null)
        {
            Debug.Log("[MinimapSetup] 기존 MinimapManager가 있습니다.");
            return;
        }

        // MinimapManager 추가
        MinimapManager manager = gameObject.AddComponent<MinimapManager>();

        // 카메라 연결 (SerializedField이므로 런타임에서는 별도 설정 필요)
        Debug.Log("[MinimapSetup] MinimapManager 생성 완료 - Inspector에서 카메라 참조를 연결하세요");
    }

    private void OnValidate()
    {
        renderTextureSize = Mathf.Clamp(renderTextureSize, 128, 2048);
        minimapUISize = Mathf.Clamp(minimapUISize, 100f, 500f);
    }
}
