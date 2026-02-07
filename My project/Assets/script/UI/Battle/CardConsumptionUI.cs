using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CardConsumptionUI : MonoBehaviour
{
    public static CardConsumptionUI Instance { get; private set; }

    [Header("UI元素")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Text promptText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onConfirmAction;
    private Action onCancelAction;

    // 卡牌消耗逻辑状态
    private CardData currentCardToPlace;
    private CharacterData targetCharacter;
    private int targetSlotIndex;
    private List<CardData> cardsToDiscard = new List<CardData>();

    public bool IsConsumptionPanelActive => confirmationPanel.activeSelf;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        confirmationPanel.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    public void Show(CardData card, CharacterData target, int slotIndex, int requiredCount, CardType requiredType)
    {
        currentCardToPlace = card;
        targetCharacter = target;
        targetSlotIndex = slotIndex;
        cardsToDiscard.Clear();

        string message = $"请选择 {requiredCount} 张卡牌用于消耗";
        promptText.text = message;

        onConfirmAction = ProcessConsumption;
        onCancelAction = CancelConsumption;

        confirmationPanel.SetActive(true);
        BattleManager.Instance.EnterDiscardMode(card, target, slotIndex, requiredCount, requiredType);
    }

    public void SelectCardForDiscard(CardData card)
    {
        if (cardsToDiscard.Contains(card))
        {
            cardsToDiscard.Remove(card);
        }
        else
        {
            cardsToDiscard.Add(card);
        }
        // 更新UI提示，显示已选择的卡牌数量
        int requiredDiscardCount = GetRequiredDiscardCount();
        promptText.text = $"要打出 {currentCardToPlace.cardName}, 您需要弃掉 {requiredDiscardCount} 张牌。已选择 {cardsToDiscard.Count}/{requiredDiscardCount} 张。";
    }

    private void ProcessConsumption()
    {
        int requiredDiscardCount = GetRequiredDiscardCount();

        if (cardsToDiscard.Count == requiredDiscardCount)
        {            
            BattleManager.Instance.PlaceCardInSlot(currentCardToPlace, targetCharacter, targetSlotIndex, new List<CardData>(cardsToDiscard));
            ResetConsumptionState();
            BattleManager.Instance.ExitDiscardMode();
        }
        else
        {
            Debug.Log("选择的弃牌数量不正确。");
            // 可以在这里向玩家显示错误消息
        }
    }

    private void OldProcessConsumption()
    {
        int requiredDiscardCount = GetRequiredDiscardCount();

        if (cardsToDiscard.Count == requiredDiscardCount)
        {
            BattleManager.Instance.PlaceCardInSlot(currentCardToPlace, targetCharacter, targetSlotIndex, cardsToDiscard);
            ResetConsumptionState();
        }
        else
        {
            Debug.Log("选择的弃牌数量不正确。");
            // 可以在这里向玩家显示错误消息
        }
    }

    private void CancelConsumption()
    {
        Debug.Log("取消卡牌消耗");
        BattleManager.Instance.ExitDiscardMode(true); // true表示卡牌被退回
        ResetConsumptionState();
    }

    private void ResetConsumptionState()
    {
        confirmationPanel.SetActive(false);
        currentCardToPlace = null;
        targetCharacter = null;
        targetSlotIndex = -1;
        cardsToDiscard.Clear();
    }

    private int GetRequiredDiscardCount()
    {
        if (currentCardToPlace == null) return 0;

        var discardRequirement = currentCardToPlace.consumptionRequirements.Find(req => req.consumptionType == CardConsumptionType.Discard);
        if (discardRequirement != null)
        {
            return discardRequirement.requiredCount;
        }
        return 0;
    }

    public void ShowConfirmation(string message, Action onConfirm, Action onCancel)
    {
        promptText.text = message;
        onConfirmAction = onConfirm;
        onCancelAction = onCancel;
        confirmationPanel.SetActive(true);
    }

    private void OnConfirm()
    {
        onConfirmAction?.Invoke();
    }

    private void OnCancel()
    {
        onCancelAction?.Invoke();
    }
}