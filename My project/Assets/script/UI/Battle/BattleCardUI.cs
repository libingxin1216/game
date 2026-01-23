using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleCardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("卡牌数据")]
    public CardData cardData;
    public CharacterData ownerCharacter;

    [Header("UI组件")]
    public Image cardBackground;
    public Text cardNameText;
    public Text cardCostText;
    public Text cardDescriptionText;

    [Header("点击效果")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private bool isSelected = false;

    void Start()
    {
        InitializeDisplay();
    }

    void InitializeDisplay()
    {
        if (cardData == null) return;

        if (cardNameText != null)
            cardNameText.text = cardData.cardName;

        if (cardCostText != null)
            cardCostText.text = $"消耗:{cardData.cost}";

        if (cardDescriptionText != null)
            cardDescriptionText.text = cardData.description;

        if (cardBackground != null)
        {
            cardBackground.color = cardData.cardColor;
            cardBackground.raycastTarget = true; // 确保可以点击
        }

        // 确保所有文本都可以点击（如果需要）
        Graphic[] graphics = GetComponentsInChildren<Graphic>();
        foreach (var graphic in graphics)
        {
            graphic.raycastTarget = true;
        }
    }

    public void Initialize(CardData data, CharacterData owner)
    {
        cardData = data;
        ownerCharacter = owner;

        Debug.Log($"初始化卡牌: {data.cardName}, 所有者: {owner.characterName}");

        InitializeDisplay();
    }

    // 点击卡牌
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ownerCharacter == null || ownerCharacter.isDead)
        {
            Debug.LogWarning("所有者无效或已死亡，无法使用卡牌");
            return;
        }

        if (CardSelectionSystem.Instance != null && CardSelectionSystem.Instance.IsSelecting())
        {
            Debug.Log("正在选择卡槽中，不能选择新卡牌");
            return;
        }

        Debug.Log($"点击卡牌: {cardData.cardName}");

        // 显示卡牌被选中效果
        SetSelected(true);

        // 显示卡槽选择面板
        ShowSlotSelection();
    }

    // 显示卡槽选择
    void ShowSlotSelection()
    {
        if (ownerCharacter == null) return;

        // 只能选择当前角色的卡槽
        if (CardSelectionSystem.Instance != null)
        {
            CardSelectionSystem.Instance.ShowSlotSelection(
                cardData,
                ownerCharacter,
                ownerCharacter, // 目标角色是自己
                OnCardSlotSelected
            );
        }
    }

    // 卡槽选择回调
    void OnCardSlotSelected(CardData card, CharacterData target, int slotIndex)
    {
        Debug.Log($"卡槽选择回调: {card.cardName} -> {target.characterName} 的卡槽 {slotIndex}");

        // 移除选中效果
        SetSelected(false);

        // 放置卡牌到卡槽
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.PlaceCardInSlot(card, target, slotIndex);

            // 从手牌中移除（卡牌UI会被销毁）
            if (ownerCharacter.handCards.Contains(card))
            {
                ownerCharacter.handCards.Remove(card);
                Destroy(gameObject);
            }
        }
    }

    // 设置选中状态
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (cardBackground != null)
        {
            cardBackground.color = selected ? selectedColor : cardData.cardColor;
        }
    }

    // 鼠标悬停效果
    public void OnPointerEnter()
    {
        if (cardBackground != null && !isSelected)
        {
            cardBackground.color = Color.Lerp(cardData.cardColor, Color.white, 0.2f);
        }
    }

    public void OnPointerExit()
    {
        if (cardBackground != null && !isSelected)
        {
            cardBackground.color = cardData.cardColor;
        }
    }

    public CardData GetCard()
    {
        return cardData;
    }
}