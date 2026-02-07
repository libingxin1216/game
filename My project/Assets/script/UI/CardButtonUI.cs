using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(CardDragHandler))]
public class CardButtonUI : MonoBehaviour
{
    public bool isHandCard = true; // 默认为手牌卡
    private bool isSelectedForDiscard = false;
    [Header("UI组件")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Text cardNameText;
    [SerializeField] private Text cardCostText;
    [SerializeField] private Text cardTypeText;
    [SerializeField] private Button button;

    private CardData cardData;
    private Action<CardData> onClickCallback;

    public void Initialize(CardData data, Action<CardData> callback)
    {
        cardData = data;
        onClickCallback = callback;

        cardNameText.text = data.cardName;
        cardCostText.text = $"费用:{data.cost}";
        cardTypeText.text = GetCardTypeString(data.cardType);

        // 设置背景颜色
        cardBackground.color = data.cardColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);
    }

    string GetCardTypeString(CardType type)
    {
        switch (type)
        {
            case CardType.Attack: return "攻击";
            case CardType.Defense: return "防御";
            case CardType.Heal: return "治疗";
            case CardType.Status: return "状态";
            case CardType.Buff: return "增益";
            case CardType.Special: return "特殊";
            default: return "未知";
        }
    }

    public void ToggleSelectionForDiscard()
    {
        isSelectedForDiscard = !isSelectedForDiscard;
        // Update visual state (e.g., change color, show an icon)
        GetComponent<Image>().color = isSelectedForDiscard ? Color.yellow : Color.white;
    }

    void OnButtonClicked()
    {
        if (CardConsumptionUI.Instance.IsConsumptionPanelActive)
        {
            CardConsumptionUI.Instance.SelectCardForDiscard(cardData);
            ToggleSelectionForDiscard();
        }
        else
        {
            if (isHandCard)
            {
                onClickCallback?.Invoke(cardData);
            }
        }
    }

    public CardData GetCardData()
    {
        return cardData;
    }
}