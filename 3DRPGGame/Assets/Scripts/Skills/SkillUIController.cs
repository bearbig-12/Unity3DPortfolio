using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Button exitBtn;
    [SerializeField] private SkillTreeSystem skillTree;
    [SerializeField] private SkillDatabase database;
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private TMP_Text skillPointsText;
    [SerializeField] private SkillTooltipPanel tooltip;

    public SkillDatabase Database => database;
    public SkillTreeSystem SkillTree => skillTree;

    private SkillNodeUI[] _nodes;

    private void Awake()
    {
        if (skillTree == null) skillTree = FindObjectOfType<SkillTreeSystem>();
        if (database == null) database = FindObjectOfType<SkillDatabase>();
        if (progress == null) progress = FindObjectOfType<PlayerProgress>();
    }

    private void OnEnable()
    {
        if (exitBtn != null) exitBtn.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (exitBtn != null) exitBtn.onClick.RemoveListener(Close);
    }

    private void Start()
    {
        CacheNodes();
        Close();
        RefreshAllNodes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) Toggle();
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpen()) Close();
    }

    public void Toggle()
    {
        if(IsOpen())
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if(skillPanel != null)
        {
            skillPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshAllNodes();
        }
    }

    public void Close()
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            HideTooltip();
        }
    }

    public bool IsOpen()
    {

        return skillPanel != null && skillPanel.activeSelf;
    }

    public void RefreshAllNodes()
    {
     

        if (skillPointsText != null && progress != null)
        {
            skillPointsText.text = progress.SkillPoints.ToString();
        }

        foreach(var node in _nodes)
        {
            node.Refresh();
        }

    }

    public void TryLearn(string id)
    {
        if (skillTree == null) return;

        if (skillTree.Learn(id))
        {
            RefreshAllNodes();
        }
    }

    public void ShowTooltip(string id, RectTransform anchor)
    {
        if (tooltip == null || database == null || skillTree == null) return;

        SkillDefinition def = database.Get(id);
        if (def == null) return;

        tooltip.Show(def, skillTree, progress, anchor);
    }

    public void HideTooltip()
    {
        if (tooltip != null) tooltip.Hide();
    }



    private void CacheNodes()
    {
        Transform root = skillPanel != null ? skillPanel.transform : transform;
        _nodes = root.GetComponentsInChildren<SkillNodeUI>(true);

        foreach (var node in _nodes)
        {
            node.SetController(this);
        }
    }

}
