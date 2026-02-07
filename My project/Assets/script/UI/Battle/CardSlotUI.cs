using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour, IDropHandler
{
    [Header("��������")]
    public int slotIndex;
    public CharacterData ownerCharacter;

    [Header("UI���")]
    public Image slotBackground;
    public Image cardDisplay;
    public Text slotInfoText; // ��ʾ������Ϣ

    [Header("��ɫ")]
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

    // ���ÿ���
    public void PlaceCard(CardData card)
    {
        currentCard = card;

        // ������ʾ
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

        Debug.Log($"���� {card.cardName} ���õ����� {slotIndex}");
    }

    // �������
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

    // ��ȡ��ǰ����
    public CardData GetCard()
    {
        return currentCard;
    }

    // ����Ƿ��п���
    public bool HasCard()
    {
        return currentCard != null;
    }

    // ������ʾ
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        CardButtonUI cardButton = droppedObject.GetComponent<CardButtonUI>();

        if (cardButton != null)
        {
            CardData cardData = cardButton.GetCardData();
            if (cardData != null)
            {
                // 将逻辑统一交给BattleManager处理
                BattleManager.Instance.AttemptToPlaceCard(cardData, ownerCharacter, slotIndex);
            }
        }
    }

    void UpdateDisplay()
    {
        if (slotBackground != null)
        {
            slotBackground.color = HasCard() ? hasCardColor : emptyColor;
        }
    }

    // ����������ں����Ľ������ܣ�
    public void OnClick()
    {
        // Ԥ����������۽�������
        Debug.Log($"������� {slotIndex}, ��ǰ����: {currentCard?.cardName ?? "��"}");
    }

    // �Ƴ����ط����ĵ��ã���ע�͵�����BattleManagerʵ�����ټӣ�
    /*
    void WithdrawCard()
    {
        if (BattleManager.Instance != null)
        {
            // �ȴ�BattleManagerʵ���������
            BattleManager.Instance.WithdrawCardFromSlot(ownerCharacter, slotIndex, currentCard);
            ClearCard();
        }
    }
    */
}