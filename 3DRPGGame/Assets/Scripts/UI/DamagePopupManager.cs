using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 2f, 0);
    [SerializeField] private float randomOffset = 0.3f;

    private const string POOL_KEY = "damagePopup";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (damagePopupPrefab != null && ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.RegisterPrefab(POOL_KEY, damagePopupPrefab, 20);
        }
    }

    public void ShowDamage(int amount, Vector3 position, DamageType type = DamageType.Normal)
    {
        Vector3 spawnPos = position + spawnOffset;
        spawnPos += new Vector3(
            Random.Range(-randomOffset, randomOffset),
            Random.Range(0, randomOffset),
            Random.Range(-randomOffset, randomOffset)
        );

        GameObject popup = null;

        if (ObjectPoolManager.Instance != null)
        {
            popup = ObjectPoolManager.Instance.Get(POOL_KEY, spawnPos, Quaternion.identity);
        }

        if (popup == null && damagePopupPrefab != null)
        {
            popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
        }

        if (popup == null) return;

        DamagePopup dp = popup.GetComponent<DamagePopup>();
        if (dp != null)
        {
            dp.Setup(amount, type);
        }
    }

    public void ShowCritical(int amount, Vector3 position)
    {
        ShowDamage(amount, position, DamageType.Critical);
    }

    public void ShowHeal(int amount, Vector3 position)
    {
        ShowDamage(amount, position, DamageType.Heal);
    }
}
