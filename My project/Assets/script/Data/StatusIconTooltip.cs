using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusIconTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public StatusType statusType;

    [SerializeField] private GameObject tooltipPrefab;
    private GameObject currentTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (StatusEffectManager.Instance == null) return;

        if (tooltipPrefab != null)
        {
            currentTooltip = Instantiate(tooltipPrefab, transform);
            currentTooltip.transform.localPosition = new Vector3(0, 50, 0);

            Text tooltipText = currentTooltip.GetComponentInChildren<Text>();
            if (tooltipText != null)
            {
                string description = StatusEffectManager.Instance.GetStatusDescription(statusType);
                tooltipText.text = $"{GetStatusName(statusType)}\n{description}";
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }

    string GetStatusName(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return "¸¯ÀÃ";
            case StatusType.Strong: return "Ç¿×³";
            case StatusType.Weak: return "Ë¥Èõ";
            case StatusType.Shield: return "»¤¶Ü";
            default: return "×´Ì¬";
        }
    }
}