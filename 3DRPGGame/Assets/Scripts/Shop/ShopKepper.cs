using UnityEngine;
using UnityEngine.UI;

public class ShopKepper : MonoBehaviour
{
    public bool playerInRange = false;
    public bool isTalking = false;

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
        isTalking = state != ShopState.Closed;

        bool isOpen = state != ShopState.Closed;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        switch (state)
        {
            case ShopState.Closed:
                HideDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                break;
            case ShopState.Dialog:
                ShowDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                break;
            case ShopState.Buy:
                HideDialogUI();
                buyPanelUI.SetActive(true);
                sellPanelUI.SetActive(false);
                break;
            case ShopState.Sell:
                HideDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(true);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
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
        }
    }
}