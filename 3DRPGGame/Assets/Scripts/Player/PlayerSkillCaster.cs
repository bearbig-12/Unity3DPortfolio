using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillCaster : MonoBehaviour
{

    [SerializeField] private SkillDatabase database;
    [SerializeField] private SkillTreeSystem skillTree;
    [SerializeField] private PlayerMovement player;

    [Header("Weapon Enhancement")]
    [SerializeField] private int weaponEnhancementBonus = 10;
    [SerializeField] private float weaponEnhancementDuration = 10f;

    [Header("Weapon Enhancement Effects")]
    [SerializeField] private GameObject weaponEnhanceVfx;   

    [Header("Fire Ball")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private Transform fireBallSpawn;
    [SerializeField] private float fireBallLifeTime = 5f;
    [SerializeField] private float fireBallRange = 10f;
    [SerializeField] private float aimHeightOffset = 1.0f;
    [SerializeField] private LayerMask enemyMask;

    private readonly Dictionary<string, float> _nextUseTime = new Dictionary<string, float>();
    private bool _weaponBuffActive;
    private GameObject _activeBuffVfx;

    private void Awake()
    {
        if (database == null) database = FindObjectOfType<SkillDatabase>();
        if (skillTree == null) skillTree = FindObjectOfType<SkillTreeSystem>();
        if (player == null) player = FindObjectOfType<PlayerMovement>();
    }


    public bool TryUseSkill(string id)
    {
        if (database == null || skillTree == null) return false;
        if (!skillTree.IsLearned(id)) return false;

        SkillDefinition def = database.Get(id);
        if (def == null) return false;

        float now = Time.time;
        if (_nextUseTime.TryGetValue(id, out float next) && now < next) return false;

        if (!ExecuteSkill(def)) return false;

        _nextUseTime[id] = now + def.cooldown;

        Debug.Log("Use Skill: " + def.name);
        return true;
    }

    private bool ExecuteSkill(SkillDefinition def)
    {
        if (def.id == "weapon_enhancement")
        {
            if (_weaponBuffActive) return false;
            StartCoroutine(WeaponEnhancementRoutine());
            return true;
        }

        if (def.id == "energy_ball" || def.id == "advanced_energy_ball")
        {
            return CastFireBall(def.damage);
        }

        if (def.id == "power_slash" || def.id == "advanced_power_slash")
        {
            return CastMeleeSkill();
        }

        return false;
    }

    private IEnumerator WeaponEnhancementRoutine()
    {
        if (player == null) yield break;

        _weaponBuffActive = true;

        if (weaponEnhanceVfx != null)
        {
            _activeBuffVfx = Instantiate(weaponEnhanceVfx, player.transform);
            _activeBuffVfx.transform.localPosition = Vector3.zero;
            _activeBuffVfx.transform.localRotation = Quaternion.identity;
        }

        player.attackDamage += weaponEnhancementBonus;

        yield return new WaitForSeconds(weaponEnhancementDuration);

        player.attackDamage -= weaponEnhancementBonus;

        if (_activeBuffVfx != null)
        {
            Destroy(_activeBuffVfx);
            _activeBuffVfx = null;
        }

        _weaponBuffActive = false;
    }

    private bool CastFireBall(int damage)
    {
        if (player == null || fireBallPrefab == null) return false;

        TriggerSkillAnimation("SpellCast");

        Transform spawn = fireBallSpawn != null ? fireBallSpawn : player.transform;
        Vector3 spawnPos = spawn.position;

        Vector3 targetPos = player.transform.position + player.transform.forward * fireBallRange;
        targetPos.y = player.transform.position.y + aimHeightOffset;

        GameObject obj = Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);

        Collider fireBallColl = obj.GetComponent<Collider>();
        Collider playerColl = player.GetComponent<Collider>();
        if (fireBallColl != null && playerColl != null)
        {
            Physics.IgnoreCollision(fireBallColl, playerColl);
        }

        FireBall fireBall = obj.GetComponent<FireBall>();
        if (fireBall == null) return false;

        fireBall.SetPlayerOwner(enemyMask);
        fireBall.Launch(targetPos, fireBallLifeTime, damage);

        return true;
    }

    private bool CastMeleeSkill()
    {
        if (player == null) return false;

        TriggerSkillAnimation("MeleeSkill");
        return true;
    }

    private void TriggerSkillAnimation(string triggerName)
    {
        if (player == null || player._animator == null) return;

        player._animator.SetTrigger(triggerName);
    }
}
