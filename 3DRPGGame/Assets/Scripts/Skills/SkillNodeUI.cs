using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SkillNodeUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [SerializeField] private string skillId;
    [SerializeField] private Image icon;
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color learnedColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private Canvas canvas;

    private SkillUIController _controller;
    private GameObject _dragIcon;


    public void SetController(SkillUIController controller)
    {
        _controller = controller;
    }

    public void Refresh()
    {
        if (_controller == null || _controller.Database == null || _controller.SkillTree == null) return;

        SkillDefinition def = _controller.Database.Get(skillId);
        if (def == null) return;

        bool learned = _controller.SkillTree.GetLearnedForSave().Exists(s => s.id == skillId && s.rank > 0);
        bool canLearn = _controller.SkillTree.CanLearn(skillId);

        if(icon != null)
        {
            if(learned)
            {
                icon.color = learnedColor;
            }
            else
            {
                if(canLearn)
                {
                    icon.color = availableColor;
                }
                else
                {
                    icon.color = lockedColor;
                }
            }
        }
        if (rankText != null)
        {
            int rank;
            if (learned)
            {
                rank = 1;
            }
            else
            {
                rank = 0;
            }

            rankText.text = rank + "/" + def.maxRank;
        }

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_controller == null) return;
        _controller.ShowTooltip(skillId, transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_controller == null) return;
        _controller.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_controller == null) return;
        _controller.TryLearn(skillId);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_controller.SkillTree.IsLearned(skillId)) return;

        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        _dragIcon = new GameObject("SkillDragIcon");
        _dragIcon.transform.SetParent(canvas.transform, false);

        var image = _dragIcon.AddComponent<Image>();
        if (icon != null) image.sprite = icon.sprite;

        var cg = _dragIcon.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.8f;

        var draggedSkill = _dragIcon.AddComponent<CurrentDragSkill>();
        draggedSkill.skillId = skillId;

        DragDrop.itemBeingDragged = _dragIcon;
        _dragIcon.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIcon != null)
        {
            _dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDrop.itemBeingDragged = null;

        if (_dragIcon != null)
        {
            Destroy(_dragIcon);
        }
    }

}
