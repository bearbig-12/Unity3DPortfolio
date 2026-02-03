using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager Instance { get; private set; }

    // 등록된 몬스터들을 추적
    private Dictionary<string, EnemyAI> spawnedMonsters = new Dictionary<string, EnemyAI>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 씬에 배치된 몬스터들 등록
        RegisterExistingMonsters();
    }

    /// <summary>
    /// 씬에 이미 배치된 몬스터들 등록
    /// </summary>
    public void RegisterExistingMonsters()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>(true);
        foreach (var enemy in enemies)
        {
            if (!string.IsNullOrEmpty(enemy.enemyId) && !spawnedMonsters.ContainsKey(enemy.enemyId))
            {
                spawnedMonsters[enemy.enemyId] = enemy;
            }
        }
    }

    /// <summary>
    /// 모든 몬스터의 현재 상태를 수집하여 반환
    /// </summary>
    public List<MonsterSaveData> GetAllSaveData()
    {
        List<MonsterSaveData> saveDataList = new List<MonsterSaveData>();

        foreach (var kvp in spawnedMonsters)
        {
            if (kvp.Value != null)
            {
                MonsterSaveData data = kvp.Value.GetSaveData();
                saveDataList.Add(data);
            }
        }

        return saveDataList;
    }

    /// <summary>
    /// 저장된 상태를 모든 몬스터에 적용
    /// </summary>
    public void ApplyLoadedData(List<MonsterSaveData> loadedData)
    {
        if (loadedData == null) return;

        foreach (var data in loadedData)
        {
            if (spawnedMonsters.TryGetValue(data.monsterId, out EnemyAI enemy))
            {
                if (enemy != null)
                {
                    enemy.ApplyLoadedData(data);
                }
            }
        }
    }

    /// <summary>
    /// 특정 몬스터 가져오기
    /// </summary>
    public EnemyAI GetMonster(string monsterId)
    {
        if (spawnedMonsters.TryGetValue(monsterId, out EnemyAI enemy))
        {
            return enemy;
        }
        return null;
    }

    /// <summary>
    /// 모든 몬스터 리셋 (새 게임 시 - 씬 재로드 필요)
    /// </summary>
    public void ResetAllMonsters()
    {
        spawnedMonsters.Clear();
        // 씬 배치 방식에서는 씬 재로드로 리셋
    }

    /// <summary>
    /// 살아있는 몬스터 수 반환
    /// </summary>
    public int GetAliveMonsterCount()
    {
        int count = 0;
        foreach (var kvp in spawnedMonsters)
        {
            if (kvp.Value != null && !kvp.Value.isDead && kvp.Value.gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }
}
