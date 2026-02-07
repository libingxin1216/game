using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
      public bool droppedOnSlot;

    public Vector3 StartPosition { get; private set; }
    public Transform StartParent { get; private set; }
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StartPosition = transform.position;
        StartParent = transform.parent;
        transform.SetParent(transform.root); // 提升到Canvas顶层，确保拖拽时在最前
        canvasGroup.blocksRaycasts = false;
        droppedOnSlot = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 使用GraphicRaycaster来检测鼠标下的UI元素
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        CardSlotUI slot = null;
        foreach (var result in results)
        {
            slot = result.gameObject.GetComponent<CardSlotUI>();
            if (slot != null)
            {
                break;
            }
        }

        if (slot == null) // 如果没有拖放到卡槽上
        {
            transform.position = StartPosition;
            transform.SetParent(StartParent);
        }
    }
}