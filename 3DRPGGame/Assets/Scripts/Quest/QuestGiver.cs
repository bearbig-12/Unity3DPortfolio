using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private QuestData quest;
    [SerializeField] private DialogueUI dialogue;

    private bool _playerInRange;

    private void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (quest == null || dialogue == null || QuestManager.Instance == null) return;

        var status = QuestManager.Instance.GetStatus(quest.questId);

        if (status == QuestStatus.Available)
        {
            if (!QuestManager.Instance.CanStartQuest(quest.questId))
            {
                dialogue.Show("Quest", "아직 할 일이 있어요.");
                return;
            }

            dialogue.Show(
                $"Quest: {quest.title}",
                quest.description,
                () => QuestManager.Instance.StartQuest(quest.questId),
                true
            );
            return;
        }

        if (status == QuestStatus.InProgress)
        {
            var obj = QuestManager.Instance.GetCurrentObjective(quest.questId);

            if (obj != null && obj.type == QuestObjectiveType.RequiredItem)
            {
                bool turnedIn = QuestManager.Instance.SubmitRequiredItem(quest.questId);
                dialogue.Show(
                    $"Quest: {quest.title}",
                    turnedIn ? "잘했어요. 다음 목표를 진행하세요." : "아이템을 가져와주세요."
                );
                return;
            }

            if (obj != null && obj.type == QuestObjectiveType.Kill)
            {
                int c = QuestManager.Instance.GetCount(quest.questId);
                dialogue.Show($"Quest: {quest.title}", $"진행중: {c}/{obj.requiredCount}");
                return;
            }
        }

        dialogue.Show($"Quest: {quest.title}", "완료된 퀘스트입니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _playerInRange = false;
    }
}