using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private List<QuestData> questOrder = new();
    [SerializeField] private DialogueUI dialogue;

    private bool _playerInRange;

    private void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (QuestManager.Instance == null || dialogue == null) return;

        QuestData quest = QuestManager.Instance.GetCurrentQuest(questOrder);

        if (quest == null)
        {
            dialogue.Show("Quest", "There are no available quests.");
            return;
        }

        var status = QuestManager.Instance.GetStatus(quest.questId);

        if (status == QuestStatus.Available)
        {
            if (!QuestManager.Instance.CanStartQuest(quest.questId))
            {
                dialogue.Show("Quest", "Please complete the prerequisite quest first.");
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
                    turnedIn ? "Item submitted. Proceed to the next objective." : "Please bring the required item."
                );
                return;
            }

            if (obj != null && obj.type == QuestObjectiveType.Kill)
            {
                int c = QuestManager.Instance.GetCount(quest.questId);
                dialogue.Show($"Quest: {quest.title}", $"Progress: {c}/{obj.requiredCount}");
                return;
            }
        }

        dialogue.Show($"Quest: {quest.title}", "This quest has already been completed.");
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
