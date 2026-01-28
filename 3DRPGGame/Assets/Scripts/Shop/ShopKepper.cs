using System.Collections.Generic;
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


    [SerializeField] private Button questBtn;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private List<QuestData> questOrder = new List<QuestData>();

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

    private void QuestMode()
    {
        Debug.Log("QuestMode called");

        if (QuestManager.Instance == null || dialogueUI == null)
            return;

        QuestData q = QuestManager.Instance.GetCurrentQuest(questOrder);

        if (q == null)
        {
            dialogueUI.Show("Quest", "There are no quests available right now.");
            return;
        }

        var status = QuestManager.Instance.GetStatus(q.questId);

        if (status == QuestStatus.Available)
        {
            dialogueUI.Show(
                $"Quest: {q.title}",
                q.description,
                () => QuestManager.Instance.StartQuest(q.questId),
                true
            );
            return;
        }

        if (status == QuestStatus.InProgress)
        {
            var obj = QuestManager.Instance.GetCurrentObjective(q.questId);

            if (obj != null && obj.type == QuestObjectiveType.RequiredItem)
            {
                bool requiredItem = QuestManager.Instance.SubmitRequiredItem(q.questId);
                dialogueUI.Show(
                    $"Quest: {q.title}",
                    requiredItem ? "Great. Proceed to the next objective." : "Please bring the required item."
                );
                return;
            }

            if (obj != null && obj.type == QuestObjectiveType.Kill)
            {
                int c = QuestManager.Instance.GetCount(q.questId);
                dialogueUI.Show($"Quest: {q.title}", $"Progress: {c}/{obj.requiredCount}");
                return;
            }
        }

        dialogueUI.Show($"Quest: {q.title}", "This quest is already completed.");
    }
}
