using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<QuestData> quests = new List<QuestData>();

    private Dictionary<string, QuestStatus> _status = new Dictionary<string, QuestStatus>();
    private Dictionary<string, int> _objectiveIndex = new Dictionary<string, int>();
    private Dictionary<string, int> _counts = new Dictionary<string, int>();

    private PlayerProgress _progress;
    private GoldManager _gold;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (var q in quests)
        {
            if (q == null || string.IsNullOrEmpty(q.questId)) continue;
            _status[q.questId] = QuestStatus.Available;
            _objectiveIndex[q.questId] = 0;
            _counts[q.questId] = 0;
        }
    }
    private void Start()
    {
        _progress = FindObjectOfType<PlayerProgress>();
        _gold = FindObjectOfType<GoldManager>();
    }
    private void OnEnable()
    {
        // 구독
        EnemyAI.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyAI.OnEnemyKilled -= OnEnemyKilled;
    }

    public QuestStatus GetStatus(string questId)
    {
        if (_status.TryGetValue(questId, out var s)) return s;
        return QuestStatus.Available;
    }

    public int GetCount(string questId)
    {
        if (_counts.TryGetValue(questId, out var c)) return c;
        return 0;
    }

    public int GetObjectiveIndex(string questId)
    {
        if (_objectiveIndex.TryGetValue(questId, out var i)) return i;
        return 0;
    }

    public QuestObjective GetCurrentObjective(string questId)
    {
        QuestData q = GetQuest(questId);
        if (q == null || q.objectives == null || q.objectives.Count == 0) return null;

        int idx = GetObjectiveIndex(questId);
        if (idx < 0 || idx >= q.objectives.Count) return null;

        return q.objectives[idx];
    }

    public bool CanStartQuest(string questId)
    {
        QuestData q = GetQuest(questId);
        if (q == null) return false;

        if (!string.IsNullOrEmpty(q.prerequisiteQuestId))
        {
            return GetStatus(q.prerequisiteQuestId) == QuestStatus.Completed;
        }
        return true;
    }

    public void StartQuest(string questId)
    {
        if (!_status.ContainsKey(questId)) return;
        if (_status[questId] != QuestStatus.Available) return;
        if (!CanStartQuest(questId)) return;

        _status[questId] = QuestStatus.InProgress;
        _objectiveIndex[questId] = 0;
        _counts[questId] = 0;
    }

    private void OnEnemyKilled(EnemyAI enemy)
    {
        if (enemy == null) return;

        string enemyId = enemy.enemyId;

        foreach (var q in quests)
        {
            if (q == null) continue;
            if (GetStatus(q.questId) != QuestStatus.InProgress) continue;

            QuestObjective obj = GetCurrentObjective(q.questId);
            if (obj == null) continue;
            if (obj.type != QuestObjectiveType.Kill) continue;
            if (obj.targetEnemyId != enemyId) continue;

            _counts[q.questId] += 1;

            if (_counts[q.questId] >= obj.requiredCount)
            {
                AdvanceObjective(q);
            }
        }
    }

    // 퀘스트 아이템 소유 확인 
    public bool SubmitRequiredItem(string questId)
    {
        if (InventorySystem.instance == null) return false;

        QuestObjective obj = GetCurrentObjective(questId);
        if (obj == null) return false;
        if (obj.type != QuestObjectiveType.RequiredItem) return false;
        if (string.IsNullOrEmpty(obj.requiredItemId)) return false;

        if (!InventorySystem.instance.HasItem(obj.requiredItemId)) return false;
        if (!InventorySystem.instance.RemoveItemById(obj.requiredItemId, 1)) return false;

        QuestData q = GetQuest(questId);
        if (q == null) return false;

        AdvanceObjective(q);
        return true;
    }

    private void AdvanceObjective(QuestData q)
    {
        string id = q.questId;

        _objectiveIndex[id] += 1;
        _counts[id] = 0;

        if (_objectiveIndex[id] >= q.objectives.Count)
        {
            CompleteQuest(q);
        }
    }

    private void CompleteQuest(QuestData q)
    {
        _status[q.questId] = QuestStatus.Completed;

        if (_progress != null && q.rewardExp > 0)
        {
            _progress.AddEXP(q.rewardExp);
        }

        if (_gold != null && q.rewardGold > 0)
        {
            _gold.Add(q.rewardGold);
        }

        if (q.rewardItem != null && InventorySystem.instance != null)
        {
            InventorySystem.instance.AddItemByData(q.rewardItem);
        }
    }

    private QuestData GetQuest(string questId)
    {
        foreach (var q in quests)
        {
            if (q != null && q.questId == questId) return q;
        }
        return null;
    }

    public QuestData GetCurrentQuest(List<QuestData> questOrder)
    {
        if (questOrder == null) return null;

        // 진행 중인 퀘스트 우선
        foreach (var q in questOrder)
        {
            if (q == null) continue;
            if (GetStatus(q.questId) == QuestStatus.InProgress) return q;
        }

        // 진행 가능한 퀘스트
        foreach (var q in questOrder)
        {
            if (q == null) continue;
            if (GetStatus(q.questId) == QuestStatus.Available && CanStartQuest(q.questId))
                return q;
        }

        return null;
    }

    public List<QuestSaveData> GetSaveData()
    {
        List<QuestSaveData> list = new List<QuestSaveData>();
        foreach (var q in quests)
        {
            if (q == null) continue;
            list.Add(new QuestSaveData
            {
                questId = q.questId,
                status = GetStatus(q.questId),
                currentObjectiveIndex = GetObjectiveIndex(q.questId),
                currentCount = GetCount(q.questId)
            });
        }
        return list;
    }

    public void ApplyLoadedData(List<QuestSaveData> data)
    {
        if (data == null) return;

        foreach (var d in data)
        {
            if (string.IsNullOrEmpty(d.questId)) continue;
            _status[d.questId] = d.status;
            _objectiveIndex[d.questId] = d.currentObjectiveIndex;
            _counts[d.questId] = d.currentCount;
        }
    }
}