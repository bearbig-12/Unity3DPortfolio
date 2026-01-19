using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

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