using UnityEngine;
using UnityEngine.UI;

public class CardButtonHandler : MonoBehaviour
{
    public CardData cardData;
    public CharacterData ownerCharacter; // 添加拥有者角色
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCardClicked);
        }
        else
        {
            Debug.LogError("��ť��������ڣ�");
        }
    }

    public void OnCardClicked()
    {
        if (cardData == null) return;

        Debug.Log($"点击了卡牌按钮: {cardData.cardName}");

        ShowSlotSelection();
    }

    void ShowSlotSelection()
    {
        if (ownerCharacter == null) return;

        if (CardSelectionSystem.Instance != null)
        {
            CardSelectionSystem.Instance.ShowSlotSelection(
                cardData,
                ownerCharacter,
                ownerCharacter, 
                OnCardSlotSelected
            );
        }
    }

    void OnCardSlotSelected(CardData card, CharacterData target, int slotIndex)
    {
        Debug.Log($"选择了卡槽: {card.cardName} -> {target.characterName} 的卡槽 {slotIndex}");

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.AttemptToPlaceCard(card, target, slotIndex);
        }
    }
}