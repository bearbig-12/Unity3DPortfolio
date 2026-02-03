using System;
using UnityEngine;

[Serializable]
public class MonsterSaveData
{
    public string monsterId;
    public bool isDead;
    public int currentHealth;
    public float posX, posY, posZ;
    public float homePosX, homePosY, homePosZ;
    public string currentStateName;
    public int bossPhase;  // 보스 전용

    public Vector3 GetPosition()
    {
        return new Vector3(posX, posY, posZ);
    }

    public void SetPosition(Vector3 pos)
    {
        posX = pos.x;
        posY = pos.y;
        posZ = pos.z;
    }

    public Vector3 GetHomePosition()
    {
        return new Vector3(homePosX, homePosY, homePosZ);
    }

    public void SetHomePosition(Vector3 pos)
    {
        homePosX = pos.x;
        homePosY = pos.y;
        homePosZ = pos.z;
    }
}
