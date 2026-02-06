using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    [Header("Same Scene Teleport")]
    [SerializeField] private Transform targetPoint;

    [Header("Scene Transition")]
    [SerializeField] private bool loadNewScene = false;
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // Scene transition mode
        if (loadNewScene && !string.IsNullOrEmpty(targetSceneName))
        {
            if (SceneLoadManager.Instance != null)
            {
                SceneLoadManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                // Fallback: direct scene load
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
            return;
        }

        // Same scene teleport mode
        if (targetPoint == null)
            return;

        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        other.transform.position = targetPoint.position;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}
