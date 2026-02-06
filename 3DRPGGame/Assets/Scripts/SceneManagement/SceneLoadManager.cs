using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private LoadingTips loadingTips;

    [Header("Settings")]
    [SerializeField] private float minLoadingTime = 1.5f;

    private bool _isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (_isLoading)
            return;

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        if (_isLoading)
            return;

        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        _isLoading = true;

        string tip = loadingTips != null ? loadingTips.GetRandomTip() : "";

        // Show loading screen
        if (loadingScreen != null)
        {
            loadingScreen.Show(tip);

            // Fade in
            yield return StartCoroutine(Fade(0f, 1f));
        }

        float startTime = Time.unscaledTime;

        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Update progress
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingScreen != null)
                loadingScreen.SetProgress(progress);

            // Scene is ready when progress reaches 0.9
            if (asyncLoad.progress >= 0.9f)
            {
                // Ensure minimum loading time
                float elapsed = Time.unscaledTime - startTime;
                if (elapsed < minLoadingTime)
                {
                    yield return new WaitForSecondsRealtime(minLoadingTime - elapsed);
                }

                if (loadingScreen != null)
                    loadingScreen.SetProgress(1f);

                yield return new WaitForSecondsRealtime(0.2f);

                // Activate scene
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Wait a frame for scene to fully load
        yield return null;

        // Fade out
        if (loadingScreen != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
            loadingScreen.Hide();
        }

        _isLoading = false;
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        string sceneName = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        yield return LoadSceneAsync(sceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (loadingScreen == null || loadingScreen.FadePanel == null)
            yield break;

        float duration = loadingScreen.FadeDuration;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            loadingScreen.FadePanel.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        loadingScreen.FadePanel.alpha = to;
    }

    public bool IsLoading => _isLoading;
}
