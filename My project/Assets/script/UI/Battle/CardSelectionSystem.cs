using UnityEngine;
using UnityEngine.UI;
using System;

public class CardSelectionSystem : MonoBehaviour
{
    public static CardSelectionSystem Instance { get; private set; }

    [Header("UI组件")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Text selectionTitle;
    [SerializeField] private Button[] slotButtons = new Button[3];
    [SerializeField] private Text[] slotButtonTexts = new Text[3];
    [SerializeField] private Button cancelButton;

    [Header("当前选择状态")]
    private CardData selectedCard;
    private CharacterData cardOwner;
    private CharacterData targetCharacter; // 卡牌要放置的角色
    private Action<CardData, CharacterData, int> onSlotSelectedCallback;

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

        InitializeUI();
    }

    void InitializeUI()
    {
        // 初始化按钮事件
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i; // 闭包需要局部变量
            slotButtons[i].onClick.AddListener(() => OnSlotSelected(slotIndex));

            if (slotButtonTexts[i] != null)
            {
                slotButtonTexts[i].text = $"卡槽 {i + 1}";
            }
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelSelection);
        }

        // 默认隐藏选择面板
        HideSelectionPanel();
    }

    // 显示卡槽选择面板
    public void ShowSlotSelection(CardData card, CharacterData owner, CharacterData targetChar,
                                 Action<CardData, CharacterData, int> callback)
    {
        if (card == null || owner == null || targetChar == null)
        {
            Debug.LogError("显示选择面板参数错误");
            return;
        }

        selectedCard = card;
        cardOwner = owner;
        targetCharacter = targetChar;
        onSlotSelectedCallback = callback;

        // 更新UI
        if (selectionTitle != null)
        {
            selectionTitle.text = $"选择放置位置 ({targetChar.characterName})";
        }

        // 更新每个卡槽按钮的状态
        UpdateSlotButtons();

        // 显示面板
        ShowSelectionPanel();

        Debug.Log($"显示卡槽选择: {card.cardName} -> {targetChar.characterName}");
    }

    // 更新卡槽按钮
    void UpdateSlotButtons()
    {
        if (targetCharacter == null) return;

        // 检查角色是否有卡槽数据
        if (BattleManager.Instance != null &&
            BattleManager.Instance.battleData.characterSlots.TryGetValue(targetCharacter, out var slots))
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                if (slotButtons[i] == null) continue;

                bool hasCard = slots.slots[i] != null;
                string cardInfo = hasCard ? $" ({slots.slots[i].cardName})" : "";

                if (slotButtonTexts[i] != null)
                {
                    slotButtonTexts[i].text = $"卡槽 {i + 1}{cardInfo}";
                }

                // 可以设置不同的颜色
                Image buttonImage = slotButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = hasCard ? new Color(1f, 0.8f, 0.8f) : new Color(0.8f, 1f, 0.8f);
                }
            }
        }
    }

    // 卡槽被选择
    void OnSlotSelected(int slotIndex)
    {
        Debug.Log($"选择卡槽 {slotIndex}");

        if (selectedCard == null || cardOwner == null || targetCharacter == null)
        {
            Debug.LogError("选择卡槽时数据不完整");
            HideSelectionPanel();
            return;
        }

        // 执行回调
        onSlotSelectedCallback?.Invoke(selectedCard, targetCharacter, slotIndex);

        // 隐藏面板
        HideSelectionPanel();

        // 清空选择
        ClearSelection();
    }

    // 取消选择
    void CancelSelection()
    {
        Debug.Log("取消卡槽选择");

        HideSelectionPanel();
        ClearSelection();
    }

    // 显示选择面板
    void ShowSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);

            // 将面板放在屏幕中央
            RectTransform rect = selectionPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
        }
    }

    // 隐藏选择面板
    void HideSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    // 清空选择数据
    void ClearSelection()
    {
        selectedCard = null;
        cardOwner = null;
        targetCharacter = null;
        onSlotSelectedCallback = null;
    }

    // 检查是否正在选择
    public bool IsSelecting()
    {
        return selectionPanel != null && selectionPanel.activeSelf;
    }
}