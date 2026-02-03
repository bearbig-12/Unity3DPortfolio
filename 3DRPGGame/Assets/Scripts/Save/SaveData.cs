using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int playerLevel;
    public int skillPoints;
    public int gold;
    public int currentExp;
    public List<SkillState> learnedSkills = new List<SkillState>();
    public List<string> inventoryItemIds = new List<string>();

    public List<InventoryItemStack> inventoryStacks = new List<InventoryItemStack>();

    // 몬스터 상태 저장
    public List<MonsterSaveData> monsterStates = new List<MonsterSaveData>();

    // 플레이어 위치 저장
    public float playerPosX, playerPosY, playerPosZ;
    public string lastSavePointId;

    public UnityEngine.Vector3 GetPlayerPosition()
    {
        return new UnityEngine.Vector3(playerPosX, playerPosY, playerPosZ);
    }

    public void SetPlayerPosition(UnityEngine.Vector3 pos)
    {
        playerPosX = pos.x;
        playerPosY = pos.y;
        playerPosZ = pos.z;
    }
}

[Serializable]
public class InventoryItemStack
{
    public string id;
    public int count;
}
