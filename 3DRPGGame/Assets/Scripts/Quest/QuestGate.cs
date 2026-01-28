using UnityEngine;

public class QuestGate : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;

    [SerializeField] private string requiredQuestId = "q_find_key";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CharacterController controller = other.GetComponent<CharacterController>();

        if (QuestManager.Instance != null &&
            QuestManager.Instance.GetStatus(requiredQuestId) == QuestStatus.Completed)
        {
            if (controller != null)
            {
                controller.enabled = false;
            }

            other.transform.position = targetPoint.position;

            if (controller != null)
            {
                controller.enabled = true;
            }
            return; 
        }

        Debug.Log("아직 던전에 들어갈 수 없습니다.");
    }
}