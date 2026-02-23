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
        Greeting,
        Dialog,
        Buy,
        Sell,
        Farewell
    }

    [SerializeField] private GameObject dialogUI;
    [SerializeField] private Button buyBtn;
    [SerializeField] private Button sellBtn;
    [SerializeField] private Button exitBtn;

    public GameObject buyPanelUI;
    public GameObject sellPanelUI;


    [SerializeField] private Button questBtn;

    [Header("NPC Dialogue")]
    [SerializeField] private NPCDialogueUI npcDialogueUI;
    [SerializeField] private NPCDialogueData npcDialogueData;

    private ShopState state = ShopState.Closed;
    private Camera _mainCamera;
    private QuestGiver _questGiver;

    private bool isShop = false;

    public bool IsShop
    {
        get { return isShop; }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _questGiver = GetComponent<QuestGiver>();
        SetState(ShopState.Closed);
    }

    private void OnEnable()
    {
        buyBtn.onClick.AddListener(BuyMode);
        sellBtn.onClick.AddListener(SellMode);
        questBtn.onClick.AddListener(QuestMode); 
        exitBtn.onClick.AddListener(StopInteract);


    }

    private void OnDisable()
    {
        buyBtn.onClick.RemoveListener(BuyMode);
        sellBtn.onClick.RemoveListener(SellMode);
        questBtn.onClick.RemoveListener(QuestMode);
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
        if (interactUI == null || _mainCamera == null)
        {
            return;
        }

        interactUI.transform.position = transform.position + interactUIOffset;
        interactUI.transform.forward = _mainCamera.transform.forward;
    }

    public void Interact()
    {
        SetState(ShopState.Greeting);
    }

    public void StopInteract()
    {
        if (state != ShopState.Closed && state != ShopState.Farewell)
        {
            SetState(ShopState.Farewell);
        }
        else
        {
            SetState(ShopState.Closed);
        }
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
                HideNPCDialogue();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                isShop = false;

                break;
            case ShopState.Greeting:
                HideDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                isShop = true;
                ShowNPCGreeting();

                break;
            case ShopState.Dialog:
                HideNPCDialogue();
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
            case ShopState.Farewell:
                HideDialogUI();
                buyPanelUI.SetActive(false);
                sellPanelUI.SetActive(false);
                isShop = true;
                ShowFarewellDialogue();

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
                SetState(ShopState.Closed);
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

    private void QuestMode()
    {
        if (_questGiver != null)
            _questGiver.Interact();
    }

    private void ShowNPCGreeting()
    {
        if (npcDialogueUI == null || npcDialogueData == null)
        {
            OnGreetingComplete();
            return;
        }

        string greeting = "";
        if (npcDialogueData.greetingLines != null && npcDialogueData.greetingLines.Length > 0)
        {
            greeting = npcDialogueData.greetingLines[0];
        }

        npcDialogueUI.Show(npcDialogueData.npcName, greeting, OnGreetingComplete);
    }

    private void ShowFarewellDialogue()
    {
        if (npcDialogueUI == null || npcDialogueData == null)
        {
            OnFarewellComplete();
            return;
        }

        string farewell = "";
        if (npcDialogueData.farewellLines != null && npcDialogueData.farewellLines.Length > 0)
        {
            farewell = npcDialogueData.farewellLines[0];
        }

        npcDialogueUI.Show(npcDialogueData.npcName, farewell, OnFarewellComplete);
    }

    private void HideNPCDialogue()
    {
        if (npcDialogueUI != null)
        {
            npcDialogueUI.Hide();
        }
    }

    private void OnGreetingComplete()
    {
        if (state == ShopState.Greeting)
        {
            SetState(ShopState.Dialog);
        }
    }

    private void OnFarewellComplete()
    {
        if (state == ShopState.Farewell)
        {
            SetState(ShopState.Closed);
        }
    }
}
