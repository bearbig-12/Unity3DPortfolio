using System.IO;
using UnityEngine;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private SkillTreeSystem skillTree;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private GoldManager goldManager;

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

    public void Save()
    {
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


        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Saved to: " + SavePath);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found.");
            return;
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



        Debug.Log("Loaded save.");
    }

    // 같은 프레임에 아이템 삭제 및 로드가 일어나면 인벤토리에 아무것도 안뜨게된다.
    private IEnumerator LoadInventoryAfterClear(SaveData data)
    {
        //기존 아이템을 먼저 삭제한다
        inventory.ClearInventory();
        yield return null;
        // 이제 빈 슬롯에 저장된 스택을 복원
        inventory.LoadFromSavedStacks(data.inventoryStacks);
    }
}
