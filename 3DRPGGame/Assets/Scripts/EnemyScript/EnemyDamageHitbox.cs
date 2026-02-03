using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageHitbox : MonoBehaviour
{
    public EnemyAI _enemy;

    private bool _active;
    private HashSet<PlayerMovement> _hit = new HashSet<PlayerMovement>();

    void OnTriggerEnter(Collider other)
    {
        if (!_active) return;

        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        if (_hit.Add(player))
        {
            int dmg = _enemy.currentAttackDamage;
            player.TakeDamage(dmg);
        }
    }

    public void SetActive(bool value)
    {
        _active = value;
        if (!_active)
        {
            _hit.Clear();
        }
    }
}