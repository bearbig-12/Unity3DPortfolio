using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    [SerializeField] private string skillId;
    [SerializeField] private Image icon;
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color learnedColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private TMP_Text rankText;

    private SkillUIController _controller;

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
}
