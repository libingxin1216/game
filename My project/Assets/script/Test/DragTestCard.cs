using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 最简单的拖拽测试脚本
public class DragTestCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private Transform startParent;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Debug.Log($"拖拽测试卡牌已初始化: {gameObject.name}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("=== 开始拖拽测试 ===");

        startPosition = transform.position;
        startParent = transform.parent;

        // 设置为Canvas的子对象，确保在最上层
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
        }

        // 拖拽效果
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        Debug.Log($"开始拖拽 {gameObject.name}");
        Debug.Log($"起始位置: {startPosition}");
        Debug.Log($"起始父对象: {startParent?.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 使用屏幕坐标转换
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint
        );

        transform.position = worldPoint;

        // 调试：显示拖拽位置
        //Debug.Log($"拖拽位置: {eventData.position}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"结束拖拽 {gameObject.name}");

        // 恢复显示
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 返回原位
        transform.SetParent(startParent);
        transform.position = startPosition;

        Debug.Log($"返回原位");
    }
}