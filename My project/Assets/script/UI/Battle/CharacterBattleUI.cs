using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 添加这行
using System.Collections;

public class CharacterBattleUI : MonoBehaviour, IPointerClickHandler // 添加接口
{
    [Header("UI组件")]
    [SerializeField] private Image characterAvatar;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Button selectButton;

    [Header("高亮效果")]
    [SerializeField] private Image highlightBorder;
    [SerializeField] private Color validHighlightColor = Color.green;
    [SerializeField] private Color invalidHighlightColor = Color.red;

    [Header("颜色")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color deadColor = Color.gray;

    [Header("目标选择")]
    [SerializeField] private Image targetSelectableBorder; // 可选目标边框
    [SerializeField] private Color targetSelectableColor = Color.red;
    [SerializeField] private Color targetSelectedColor = Color.green;

    [Header("状态显示")]
    public Transform statusEffectsContainer;
    public GameObject statusEffectIconPrefab;

    // 私有字段
    [SerializeField] private CharacterData _characterData;
    private bool isPlayer;

    // 公共属性
    public CharacterData CharacterData
    {
        get { return _characterData; }
        private set { _characterData = value; }
    }

    [Header("卡槽引用")]
    public CardSlotUI[] cardSlots = new CardSlotUI[3];

    // 添加一个字段标识是否是玩家角色
    private bool isPlayerCharacter;

    public void Initialize(CharacterData data, bool isPlayerSide)
    {
        CharacterData = data;
        isPlayerCharacter = isPlayerSide; // 保存这个信息

        Debug.Log($"初始化角色UI: {data.characterName}");

        CharacterData = data;
        isPlayer = isPlayerSide;

        // 设置基本信息
        if (characterNameText != null)
            characterNameText.text = data.characterName;

        // 设置头像颜色
        if (characterAvatar != null)
        {
            characterAvatar.color = data.characterColor;

            // 如果是敌人，变暗一点
            if (!isPlayer)
            {
                characterAvatar.color *= 0.7f;
            }
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();

            if (isPlayerSide)
            {
                // 玩家角色：显示手牌
                selectButton.onClick.AddListener(OnPlayerCharacterClicked);
                Debug.Log($"为玩家角色 {data.characterName} 设置手牌显示事件");
            }
            else
            {
                // 敌人角色：作为目标选择
                selectButton.onClick.AddListener(OnEnemyCharacterClicked);
                Debug.Log($"为敌人角色 {data.characterName} 设置目标选择事件");
            }
        }
        UpdateUI();
        InitializeCardSlots();
    }

    void InitializeCardSlots()
    {
        // 自动查找子对象中的卡槽
        cardSlots = GetComponentsInChildren<CardSlotUI>();

        // 按顺序排序（确保顺序正确）
        System.Array.Sort(cardSlots, (a, b) => a.slotIndex.CompareTo(b.slotIndex));

        Debug.Log($"为 {CharacterData.characterName} 找到 {cardSlots.Length} 个卡槽");
    }

    public void UpdateUI()
    {
        if (CharacterData == null) return;

        // 更新生命值显示
        if (healthSlider != null)
        {
            float healthPercent = (float)CharacterData.currentHealth / CharacterData.maxHealth;
            healthSlider.value = healthPercent;
        }

        if (healthText != null)
        {
            healthText.text = $"{CharacterData.currentHealth}/{CharacterData.maxHealth}";
        }

        // 如果角色死亡，变灰
        if (characterAvatar != null)
        {
            if (CharacterData.isDead)
            {
                characterAvatar.color = deadColor;
                if (selectButton != null)
                    selectButton.interactable = false;
            }
            else
            {
                if (selectButton != null)
                    selectButton.interactable = true;
            }
        }
    }

    // 更新状态显示
    public void UpdateStatusEffects()
    {
        if (CharacterData == null) return;

        // 清空现有状态图标
        foreach (Transform child in statusEffectsContainer)
        {
            Destroy(child.gameObject);
        }

        // 显示所有状态
        foreach (var status in CharacterData.statusEffects)
        {
            if (status.stacks <= 0) continue;

            GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectsContainer);
            Image iconImage = iconObj.GetComponent<Image>();
            Text stackText = iconObj.GetComponentInChildren<Text>();

            if (iconImage != null && StatusEffectManager.Instance != null)
            {
                iconImage.sprite = StatusEffectManager.Instance.GetStatusIcon(status.type);
                iconImage.color = StatusEffectManager.Instance.GetStatusColor(status.type);
            }

            if (stackText != null)
            {
                stackText.text = status.stacks.ToString();
            }

            // 添加悬停提示
            StatusIconTooltip tooltip = iconObj.GetComponent<StatusIconTooltip>();
            if (tooltip == null)
            {
                tooltip = iconObj.AddComponent<StatusIconTooltip>();
            }
            tooltip.statusType = status.type;
        }
    }

    void OnPlayerCharacterClicked()
    {
        if (CharacterData == null || CharacterData.isDead) return;

        Debug.Log($"点击选择角色: {CharacterData.characterName}");

        // 通知BattleManager选中这个角色
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SelectCharacter(CharacterData);
        }
    }

    // 敌人角色被点击（作为目标）
    void OnEnemyCharacterClicked()
    {
        if (CharacterData == null || CharacterData.isDead) return;

        Debug.Log($"点击敌人作为目标: {CharacterData.characterName}");

        // 检查是否在目标选择模式下
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.isSelectingTarget)
            {
                // 在目标选择模式下，点击敌人选择为目标
                BattleManager.Instance.SelectTarget(CharacterData);
            }
            else
            {
                // 不在目标选择模式下，显示提示
                Debug.Log($"点击了敌人，但当前不在目标选择模式");
            }
        }
    }

    // 设置可被选为目标的状态
    public void SetTargetSelectable(bool selectable)
    {
        Debug.Log($"设置 {CharacterData.characterName} 可选状态: {selectable}");

        if (targetSelectableBorder == null)
        {
            Debug.LogWarning($"角色 {CharacterData.characterName} 没有目标边框");
            return;
        }

        targetSelectableBorder.gameObject.SetActive(selectable);

        if (selectable)
        {
            targetSelectableBorder.color = targetSelectableColor;

            // 添加闪烁效果
            StartCoroutine(BlinkTargetBorder());
            Debug.Log($"开始闪烁: {CharacterData.characterName}");
        }
        else
        {
            StopAllCoroutines();
            Debug.Log($"停止闪烁: {CharacterData.characterName}");
        }
    }

    // 闪烁效果
    IEnumerator BlinkTargetBorder()
    {
        while (targetSelectableBorder.gameObject.activeSelf)
        {
            float alpha = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.5f;
            Color color = targetSelectableBorder.color;
            color.a = alpha;
            targetSelectableBorder.color = color;

            yield return null;
        }
    }

    // 设置已被选中为目标的样式
    public void SetAsSelectedTarget(bool selected)
    {
        if (targetSelectableBorder == null) return;

        targetSelectableBorder.gameObject.SetActive(selected);

        if (selected)
        {
            targetSelectableBorder.color = targetSelectedColor;
            StopAllCoroutines(); // 停止闪烁
        }
    }

    // 实现IPointerClickHandler接口
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (CharacterData == null || CharacterData.isDead) return;

        Debug.Log($"点击角色: {CharacterData.characterName}");

        // 如果正在选择目标，处理目标选择
        if (BattleManager.Instance != null && BattleManager.Instance.IsSelectingTarget())
        {
            BattleManager.Instance.OnCharacterClickedAsTarget(CharacterData);
        }
        else
        {
            // 正常点击选择角色（显示手牌）
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.SelectCharacter(CharacterData);
            }
        }
    }

    // 添加这个方法
    public CardSlotUI GetCardSlot(int index)
    {
        if (index >= 0 && index < cardSlots.Length)
        {
            return cardSlots[index];
        }
        return null;
    }

    public void SetHighlight(bool highlight, bool isValid)
    {
        if (highlightBorder == null) return;

        highlightBorder.gameObject.SetActive(highlight);

        if (highlight)
        {
            highlightBorder.color = isValid ? validHighlightColor : invalidHighlightColor;
        }
    }


}