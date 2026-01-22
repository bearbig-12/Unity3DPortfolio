using System.IO;
using UnityEngine;

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
            data.inventoryItemIds = inventory.GetSavedItmeIDs();
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
            inventory.LoadFromSavedIds(data.inventoryItemIds);
        }

        Debug.Log("Loaded save.");
    }
}
