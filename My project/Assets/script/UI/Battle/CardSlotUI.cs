using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    [Header("基础设置")]
    public int slotIndex;
    public CharacterData ownerCharacter;

    [Header("UI组件")]
    public Image slotBackground;
    public Image cardDisplay;
    public Text slotInfoText; // 显示卡牌信息

    [Header("颜色")]
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color hasCardColor = Color.white;

    private CardData currentCard;

    void Start()
    {
        UpdateDisplay();
    }

    public void Initialize(CharacterData owner, int index)
    {
        ownerCharacter = owner;
        slotIndex = index;

        name = $"CardSlot_{index}_{owner.characterName}";

        UpdateDisplay();
    }

    // 放置卡牌
    public void PlaceCard(CardData card)
    {
        currentCard = card;

        // 更新显示
        if (cardDisplay != null)
        {
            cardDisplay.color = card.cardColor;
            cardDisplay.gameObject.SetActive(true);
        }

        if (slotInfoText != null)
        {
            slotInfoText.text = card.cardName;
            slotInfoText.gameObject.SetActive(true);
        }

        UpdateDisplay();

        Debug.Log($"卡牌 {card.cardName} 放置到卡槽 {slotIndex}");
    }

    // 清除卡牌
    public void ClearCard()
    {
        currentCard = null;

        if (cardDisplay != null)
        {
            cardDisplay.gameObject.SetActive(false);
        }

        if (slotInfoText != null)
        {
            slotInfoText.gameObject.SetActive(false);
        }

        UpdateDisplay();
    }

    // 获取当前卡牌
    public CardData GetCard()
    {
        return currentCard;
    }

    // 检查是否有卡牌
    public bool HasCard()
    {
        return currentCard != null;
    }

    // 更新显示
    void UpdateDisplay()
    {
        if (slotBackground != null)
        {
            slotBackground.color = HasCard() ? hasCardColor : emptyColor;
        }
    }

    // 鼠标点击（用于后续的交换功能）
    public void OnClick()
    {
        // 预留：点击卡槽交换卡牌
        Debug.Log($"点击卡槽 {slotIndex}, 当前卡牌: {currentCard?.cardName ?? "空"}");
    }

    // 移除撤回方法的调用（先注释掉，等BattleManager实现了再加）
    /*
    void WithdrawCard()
    {
        if (BattleManager.Instance != null)
        {
            // 等待BattleManager实现这个方法
            BattleManager.Instance.WithdrawCardFromSlot(ownerCharacter, slotIndex, currentCard);
            ClearCard();
        }
    }
    */
}