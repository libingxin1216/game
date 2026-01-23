using UnityEngine;
using UnityEngine.UI;
using System;

public class CardButtonUI : MonoBehaviour
{
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
        cardCostText.text = $"消耗:{data.cost}";
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

    void OnButtonClicked()
    {
        onClickCallback?.Invoke(cardData);
    }

    public CardData GetCard()
    {
        return cardData;
    }
}