using UnityEngine;
using UnityEngine.UI;

public class ShopKepper : MonoBehaviour
{
    public bool playerInRange = false;
    public bool isTalking = false;
    [SerializeField] private GameObject interactUI;
    [SerializeField] private Vector3 interactUIOffset = new Vector3(0f, 2.2f, 0f);

    private enum ShopState
    {
        Closed,
        Dialog,
        Buy,
        Sell
    }

    [SerializeField] private GameObject dialogUI;
    [SerializeField] private Button buyBtn;
    [SerializeField] private Button sellBtn;
    [SerializeField] private Button exitBtn;

    public GameObject buyPanelUI;
    public GameObject sellPanelUI;

    private ShopState state = ShopState.Closed;

    private bool isShop = false;

    public bool IsShop
    {
        get { return isShop; }
    }

    private void Start()
    {
        SetState(ShopState.Closed);
    }

    private void OnEnable()
    {
        buyBtn.onClick.AddListener(BuyMode);
        sellBtn.onClick.AddListener(SellMode);
        exitBtn.onClick.AddListener(StopInteract);


    }

    private void OnDisable()
    {
        buyBtn.onClick.RemoveListener(BuyMode);
        sellBtn.onClick.RemoveListener(SellMode);
        exitBtn.onClick.RemoveListener(StopInteract);
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (state == ShopState.Closed)
            {
                Interact();
            }
            else
            {
                StopInteract();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && state != ShopState.Closed)
        {
            StopInteract();
        }
    }

    private void LateUpdate()
    {
        if (interactUI == null)
        {
            return;
        }

        interactUI.transform.position = transform.position + interactUIOffset;
        interactUI.transform.forward = Camera.main.transform.forward;
    }

    public void Interact()
    {
        SetState(ShopState.Dialog);
    }

    public void StopInteract()
    {
        SetState(ShopState.Closed);
    }

    private void ShowDialogUI()
    {
        dialogUI.SetActive(true);
    }

    private void HideDialogUI()
    {
        dialogUI.SetActive(false);
    }

    private void BuyMode()
    {
        SetState(ShopState.Buy);
    }

    private void SellMode()
    {
        SetState(ShopState.Sell);
    }

    private void DialogMode()
    {
        SetState(ShopState.Dialog);
    }

    private void SetState(ShopState nextState)
    {
        state = nextState;
        if (state != ShopState.Closed)
        {
            isTalking = true;
        }
        else
        {
            isTalking = false;
        }

        if (state != ShopState.Closed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        switch (state)
        {
            case ShopState.Closed:
                HideDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                isShop = false;

                break;
            case ShopState.Dialog:
                ShowDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                isShop = true;

                break;
            case ShopState.Buy:
                HideDialogUI();
                buyPanelUI.SetActive(true);
                sellPanelUI.SetActive(false);
                isShop = true;

                break;
            case ShopState.Sell:
                HideDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(true);
                isShop = true;

                break;
        }

        UpdateInteractUI();

        if (InventorySystem.instance != null)
        {
            if (state == ShopState.Buy || state == ShopState.Sell)
            {
                InventorySystem.instance.SetInventoryOpen(true, false);
            }
            else
            {
                InventorySystem.instance.SetInventoryOpen(false, false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateInteractUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (state != ShopState.Closed)
            {
                StopInteract();
            }
            else
            {
                UpdateInteractUI();
            }
        }
    }

    private void UpdateInteractUI()
    {
        if (interactUI == null)
        {
            return;
        }

        bool shouldShow = playerInRange && state == ShopState.Closed;
        interactUI.SetActive(shouldShow);
    }
}