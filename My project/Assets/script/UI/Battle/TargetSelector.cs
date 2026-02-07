using UnityEngine;
using UnityEngine.EventSystems;

public class TargetSelector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private RectTransform rectTransform;
    private Canvas canvas;

    public CharacterBattleUI owner; // 所属角色UI

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 开始拖拽
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 检查是否拖拽到有效的角色头像上
        CharacterBattleUI target = GetTargetUnderMouse(eventData);

        if (target != null && target != owner) // 不能指向自己
        {
            // 通知BattleManager
            BattleManager.Instance.SetCharacterTarget(owner.GetCharacterData(), target.GetCharacterData());
            // 可以在这里添加视觉效果，比如画一条线
        }

        // 无论是否成功，都返回原位
        rectTransform.anchoredPosition = originalPosition;
    }

    private CharacterBattleUI GetTargetUnderMouse(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerCurrentRaycast.gameObject;
        if (draggedObject != null)
        {            
            return draggedObject.GetComponentInParent<CharacterBattleUI>();
        }
        return null;
    }
}