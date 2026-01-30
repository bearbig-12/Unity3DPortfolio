
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
    [SerializeField] private TrailRenderer trail;

    void Awake()
    {
        if (trail == null) trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.emitting = false;
    }

    public void SetTrail(Transform weaponRoot)
    {
        if (weaponRoot == null) { trail = null; return; }

        // 무기 하위에서 TrailRenderer 찾아서 연결
        trail = weaponRoot.GetComponentInChildren<TrailRenderer>(true);

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }
    }

    public void AnimEvent_TrailOn()
    {
        if (trail == null) return;
        trail.Clear();
        trail.emitting = true;
    }

    public void AnimEvent_TrailOff()
    {
        if (trail == null) return;
        trail.emitting = false;
    }
}
