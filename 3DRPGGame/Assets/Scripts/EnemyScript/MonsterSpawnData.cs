using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterSpawnEntry
{
    public string monsterId;
    public GameObject prefab;
    public Vector3 spawnPosition;
    public Vector3 homePosition;
    public float spawnRotationY;
}

[CreateAssetMenu(fileName = "NewMonsterSpawnData", menuName = "Spawning/MonsterSpawnData")]
public class MonsterSpawnData : ScriptableObject
{
    public string zoneId;
    public List<MonsterSpawnEntry> monsters = new List<MonsterSpawnEntry>();
}
