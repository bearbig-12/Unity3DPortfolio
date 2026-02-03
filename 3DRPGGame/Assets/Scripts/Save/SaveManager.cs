using System;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private SkillTreeSystem skillTree;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private GoldManager goldManager;
    [SerializeField] private Transform player;

    [Header("Performance Logging")]
    [SerializeField] private bool enablePerformanceLog = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Load();
        }
    }

    public void Save(string savePointId = "")
    {
        Stopwatch sw = null;
        long beforeMem = 0;

        if (enablePerformanceLog)
        {
            sw = Stopwatch.StartNew();
            beforeMem = GC.GetTotalMemory(false);
        }

        SaveData data = new SaveData();

        data.currentExp = progress.CurrentExp;


        if (progress != null)
        {
            data.playerLevel = progress.Level;
            data.skillPoints = progress.SkillPoints;

        }

        if(goldManager != null)
        {
            data.gold = goldManager.Gold;
        }

        if(skillTree != null)
        {
            data.learnedSkills = skillTree.GetLearnedForSave();
        }

        if(inventory != null)
        {
            data.inventoryStacks = inventory.GetSavedStacks();
        }

        // 플레이어 위치 저장
        if (player != null)
        {
            data.SetPlayerPosition(player.position);
        }

        // 세이브 포인트 ID 저장
        data.lastSavePointId = savePointId;

        // 몬스터 상태 저장
        if (MonsterSpawnManager.Instance != null)
        {
            data.monsterStates = MonsterSpawnManager.Instance.GetAllSaveData();
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        if (enablePerformanceLog)
        {
            sw.Stop();
            long afterMem = GC.GetTotalMemory(false);
            long memUsed = (afterMem - beforeMem) / 1024;
            int monsterCount = data.monsterStates?.Count ?? 0;

            UnityEngine.Debug.Log($"[Save 성능] 시간: {sw.ElapsedMilliseconds}ms | 메모리: {memUsed}KB | 몬스터: {monsterCount}마리");
        }

        UnityEngine.Debug.Log("Saved to: " + SavePath);
    }

    public void SaveFromTrigger(string savePointId)
    {
        Save(savePointId);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            UnityEngine.Debug.Log("No save file found.");
            return;
        }

        Stopwatch sw = null;
        long beforeMem = 0;

        if (enablePerformanceLog)
        {
            sw = Stopwatch.StartNew();
            beforeMem = GC.GetTotalMemory(false);
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        progress.ApplyLoadedData(data.playerLevel, data.currentExp);
        progress.SkillPoints = data.skillPoints;

        if (progress != null)
        {
            progress.Level = data.playerLevel;
            progress.SkillPoints = data.skillPoints;
        }

        if (goldManager != null)
        {
            goldManager.SetGold(data.gold);
        }

        if (skillTree != null)
        {
            skillTree.LoadLearned(data.learnedSkills);
        }

        if (inventory != null)
        {
            StartCoroutine(LoadInventoryAfterClear(data));
        }

        // 플레이어 위치 복원
        if (player != null && (data.playerPosX != 0 || data.playerPosY != 0 || data.playerPosZ != 0))
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.position = data.GetPlayerPosition();
                cc.enabled = true;
            }
            else
            {
                player.position = data.GetPlayerPosition();
            }
        }

        // 몬스터 상태 복원
        if (MonsterSpawnManager.Instance != null && data.monsterStates != null)
        {
            MonsterSpawnManager.Instance.ApplyLoadedData(data.monsterStates);
        }

        if (enablePerformanceLog)
        {
            sw.Stop();
            long afterMem = GC.GetTotalMemory(false);
            long memUsed = (afterMem - beforeMem) / 1024;
            int monsterCount = data.monsterStates?.Count ?? 0;

            UnityEngine.Debug.Log($"[Load 성능] 시간: {sw.ElapsedMilliseconds}ms | 메모리: {memUsed}KB | 몬스터: {monsterCount}마리");
        }

        UnityEngine.Debug.Log("Loaded save.");
    }

    // ���� �����ӿ� ������ ���� �� �ε尡 �Ͼ�� �κ��丮�� �ƹ��͵� �ȶ߰Եȴ�.
    private IEnumerator LoadInventoryAfterClear(SaveData data)
    {
        //���� �������� ���� �����Ѵ�
        inventory.ClearInventory();
        yield return null;
        // ���� �� ���Կ� ����� ������ ����
        inventory.LoadFromSavedStacks(data.inventoryStacks);
    }
}
