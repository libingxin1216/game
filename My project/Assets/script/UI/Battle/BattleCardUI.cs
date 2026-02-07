using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleCardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("")]
    public CardData cardData;
    public CharacterData ownerCharacter;

    [Header("UI")]
    public Image cardBackground;
    public Text cardNameText;
    public Text cardCostText;
    public Text cardDescriptionText;

    [Header("Ч")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color canBeDiscardedColor = Color.green; // 可以被丢弃的卡牌颜色
    public Color selectedForDiscardColor = Color.blue; // 被选中用于丢弃的卡牌颜色

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
            cardCostText.text = $":{cardData.cost}";

        if (cardDescriptionText != null)
            cardDescriptionText.text = cardData.description;

        if (cardBackground != null)
        {
            cardBackground.color = cardData.cardColor;
            cardBackground.raycastTarget = true; // ȷԵ
        }

        // ȷıԵҪ
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

        Debug.Log($"ʼ: {data.cardName}, : {owner.characterName}");

        InitializeDisplay();
    }

    // 
    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果在弃牌模式下，点击卡牌是用于选择弃牌
        if (BattleManager.Instance != null && (BattleManager.Instance.IsInDiscardMode || (CardConsumptionUI.Instance != null && CardConsumptionUI.Instance.IsConsumptionPanelActive)))
        {
            BattleManager.Instance.OnCardSelectedForDiscard(cardData);
            return;
        }

        // 正常点击卡牌的逻辑
        if (ownerCharacter == null || ownerCharacter.isDead)
        {
            Debug.LogWarning("角色已死亡或无效，无法使用卡牌");
            return;
        }

        if (CardSelectionSystem.Instance != null && CardSelectionSystem.Instance.IsSelecting())
        {
            Debug.Log("正在选择卡槽，无法选择新卡牌");
            return;
        }

        Debug.Log($"点击了卡牌: {cardData.cardName}");

        // 显示被选中的视觉效果
        SetSelected(true);

        // 显示卡槽选择
        ShowSlotSelection();
    }

    // ʾѡ
    void ShowSlotSelection()
    {
        if (ownerCharacter == null) return;

        // ֻѡǰɫĿ
        if (CardSelectionSystem.Instance != null)
        {
            CardSelectionSystem.Instance.ShowSlotSelection(
                cardData,
                ownerCharacter,
                ownerCharacter, // ĿɫԼ
                OnCardSlotSelected
            );
        }
    }

    // ѡص
    void OnCardSlotSelected(CardData card, CharacterData target, int slotIndex)
    {
        Debug.Log($"ѡص: {card.cardName} -> {target.characterName} Ŀ {slotIndex}");

        // ƳѡЧ
        SetSelected(false);

        // ÿƵ
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.AttemptToPlaceCard(card, target, slotIndex);
        }
    }

    // ѡ״̬
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (cardBackground != null)
        {
            cardBackground.color = selected ? selectedColor : cardData.cardColor;
        }
    }

    // ͣЧ
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
    
    public void SetHighlight(bool canBeDiscarded, bool isSelectedForDiscard)
    {
        if (cardBackground == null) return;

        if (isSelectedForDiscard)
        {
            cardBackground.color = selectedForDiscardColor;
        }
        else if (canBeDiscarded)
        {
            cardBackground.color = canBeDiscardedColor;
        }
        else
        {
            // Restore original color, considering if it's selected for play
            cardBackground.color = isSelected ? selectedColor : cardData.cardColor;
        }
    }

    public CardData GetCard()
    {
        return cardData;
    }
}