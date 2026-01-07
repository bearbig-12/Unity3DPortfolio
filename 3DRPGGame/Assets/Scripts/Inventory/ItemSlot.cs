using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    // 현재 슬롯에 들어있는 아이템(자식 오브젝트) 반환
    public GameObject Item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }
            return null;
        }
    }
    // 아이템이 슬롯에 드롭 되었을 때
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");

        if (DragDrop.itemBeingDragged == null)
        {
            return;
        }

        if (Item == null)
        {
            DragDrop.itemBeingDragged.transform.SetParent(transform);
            DragDrop.itemBeingDragged.transform.localPosition = Vector2.zero;
        }
    }
}
