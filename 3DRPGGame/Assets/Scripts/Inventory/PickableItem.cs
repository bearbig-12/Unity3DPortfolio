
using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public Renderer itemRenderer;
    public Color highlightColor = Color.yellow;
    public float intensity = 2f;

    // 해당 아이템의 인벤토리 아이콘 프리팹
    [SerializeField] private GameObject _pickUpItemPrefab;

    private Color _originalEmission;
    private bool _playerInRange = false;

    private void Awake()
    {
        if(itemRenderer == null)
        {
            itemRenderer = GetComponentInChildren<Renderer>();
        }
        _originalEmission = itemRenderer.material.GetColor("_EmissionColor");


    }

    private void Update()
    {
        if(_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if(_pickUpItemPrefab != null)
            {
                // Add the item to the inventory
                if(InventorySystem.instance.AddItem(_pickUpItemPrefab))
                {
                    // Pick up the item
                    Debug.Log("Item Picked Up: " + gameObject.name);
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("PickUpItemPrefab is not assigned for " + gameObject.name);
                return;
            }




         
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Highlight the item
            itemRenderer.material.EnableKeyword("_EMISSION");
            itemRenderer.material.SetColor("_EmissionColor", highlightColor * intensity);
            _playerInRange = true;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Remove highlight
            itemRenderer.material.SetColor("_EmissionColor", _originalEmission);
            _playerInRange = false;
        }
    }


}
