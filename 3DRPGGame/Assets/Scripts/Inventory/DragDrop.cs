using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Canvas canvas;
    private RectTransform _rectTransform;
    // 드래그 중 투명도 조절 및 Raycast 차단을 위한 CanvasGroup
    private CanvasGroup _canvasGroup;

    public static GameObject itemBeingDragged; // 현재 드래그 중인 아이템
    Vector3 startPos;
    Transform startParent;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
    }


    public void SetCanvas(Canvas newCanvas)
    {
        if(newCanvas != null)
        {
            canvas = newCanvas;
        }
        else
        {
            Debug.LogWarning("DragDrop: Assigned canvas is null.");
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");


        _canvasGroup.alpha = 0.6f;
        // 드래그 중인 아이템의 UI가 Raycast를 받지 않도록 설정 (아래 슬롯이 드롭을 클릭할 수 있도록)
        _canvasGroup.blocksRaycasts = false;
        startPos = transform.position;
        startParent = transform.parent;
        transform.SetParent(transform.root);
        itemBeingDragged = gameObject;

    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        // eventData.delta : 마우스의 이동한 거리
        // canvas.scaleFactor : 캔버스의 스케일 비율 / UI가 Canvas의 스케일 영향을 받기 때문에 나눠줘야 정확한 위치로 이동
        _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;

        if(transform.parent == startParent || transform.parent == transform.root)
        {
            transform.position = startPos;
            transform.SetParent(startParent);
        }

        Debug.Log("OnEndDrag"); 
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

    }
}
