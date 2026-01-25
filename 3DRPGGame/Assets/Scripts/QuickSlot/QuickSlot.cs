using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum QuickSlotType
{
    None,
    HpPotion,
    StaminaPotion,
    Skill
}

public class QuickSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;

    public QuickSlotType SlotType { get; private set; } = QuickSlotType.None;
    public string ItemId { get; private set; }
    public string SkillId { get; private set; }


    private void Awake()
    {
        if (iconImage == null)
        {
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage == null) iconImage = GetComponentInChildren<Image>();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = DragDrop.itemBeingDragged;
       
        if (dragged == null) return;

        // Inventory item
        InventoryItemUI itemUI = dragged.GetComponent<InventoryItemUI>();
        if (itemUI != null && itemUI.ItemData != null)
        {
            if (itemUI.ItemData.itemType != InventoryItemType.Consumable) return;

            ItemId = itemUI.ItemData.GetStableId();
            SkillId = null;

            SlotType = itemUI.ItemData.consumableType == ConsumableType.HP
                ? QuickSlotType.HpPotion
                : QuickSlotType.StaminaPotion;

            if (iconImage != null) iconImage.sprite = itemUI.ItemData.icon;
            return;
        }

        // Skill drag 
        CurrentDragSkill draggedSkill = dragged.GetComponent<CurrentDragSkill>();
        if (draggedSkill != null)
        {
            SlotType = QuickSlotType.Skill;
            SkillId = draggedSkill.skillId;
            ItemId = null;

            Image draggedIcon = dragged.GetComponent<Image>();
            if (iconImage != null && draggedIcon != null) iconImage.sprite = draggedIcon.sprite;
        }
    }

    public void UseSlot()
    {
        if (SlotType == QuickSlotType.None) return;

        if (SlotType == QuickSlotType.HpPotion || SlotType == QuickSlotType.StaminaPotion)
        {
            if (InventorySystem.instance != null && !string.IsNullOrEmpty(ItemId))
            {
                InventorySystem.instance.TryConsumeForQuickSlot(ItemId);
            }
            return;
        }

        if (SlotType == QuickSlotType.Skill)
        {
            PlayerSkillCaster caster = FindObjectOfType<PlayerSkillCaster>();
            if (caster != null && !string.IsNullOrEmpty(SkillId))
            {
                caster.TryUseSkill(SkillId);
            }
        }
    }
}
