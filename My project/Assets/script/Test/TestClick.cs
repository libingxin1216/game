// TestClick.cs - 放在BattleCard预制体上
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestClick : MonoBehaviour, IPointerClickHandler
{
    void Start()
    {
        Debug.Log($"TestClick脚本已附加到: {gameObject.name}");

        // 确保有Image组件
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
            Debug.Log("已启用Image的Raycast Target");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("=== 点击测试成功！ ===");
        Debug.Log($"点击了: {gameObject.name}");
        Debug.Log($"点击位置: {eventData.position}");

        // 简单视觉反馈
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.color = Color.red;
            Invoke("ResetColor", 0.5f);
        }
    }

    void ResetColor()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.color = Color.white;
        }
    }
}