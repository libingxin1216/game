using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 引入事件系统
using System.Collections;

public class CharacterBattleUI : MonoBehaviour, IPointerClickHandler // 实现接口
{
    [Header("UI元素")]
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
    public CharacterData characterData
    {
        get { return _characterData; }
        private set { _characterData = value; }
    }

    // 方便外部访问
    public CharacterData GetCharacterData()
    {
        return characterData;
    }

    [Header("卡牌槽")]
    public CardSlotUI[] cardSlots = new CardSlotUI[3];

    [Header("目标选择器")]
    public TargetSelector targetSelector;

    // 一个字段标识是否为玩家角色
    private bool isPlayerCharacter;

    public void Initialize(CharacterData data, bool isPlayerSide)
    {
        characterData = data;
        isPlayerCharacter = isPlayerSide; // 设置阵营信息

        Debug.Log($"初始化角色UI: {data.characterName}");

        characterData = data;
        isPlayer = isPlayerSide;

        // 设置名称
        if (characterNameText != null)
            characterNameText.text = data.characterName;

        // 设置头像颜色
        if (characterAvatar != null)
        {
            characterAvatar.color = data.characterColor;

            // 如果是敌人，暗一点
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
                // 我方角色，点击后显示手牌
                selectButton.onClick.AddListener(OnPlayerCharacterClicked);
                Debug.Log($"为我方角色 {data.characterName} 绑定手牌显示事件");
            }
            else
            {
                // 敌方角色，点击后作为目标选择
                selectButton.onClick.AddListener(OnEnemyCharacterClicked);
                Debug.Log($"为敌方角色 {data.characterName} 绑定目标选择事件");
            }
        }
        UpdateUI();
        InitializeCardSlots();
        InitializeTargetSelector();
    }

    void InitializeCardSlots()
    {
        // 自动查找子对象中的卡槽
        cardSlots = GetComponentsInChildren<CardSlotUI>();

        // 按顺序排序以确保顺序正确
        System.Array.Sort(cardSlots, (a, b) => a.slotIndex.CompareTo(b.slotIndex));

        Debug.Log($"为 {characterData.characterName} 找到 {cardSlots.Length} 个卡槽");
    }

    public void UpdateUI()
    {
        if (characterData == null) return;

        // 更新生命值显示
        if (healthSlider != null)
        {
            float healthPercent = (float)characterData.currentHealth / characterData.maxHealth;
            healthSlider.value = healthPercent;
        }

        if (healthText != null)
        {
            healthText.text = $"{characterData.currentHealth}/{characterData.maxHealth}";
        }

        // 更新角色存活状态
        if (characterAvatar != null)
        {
            if (characterData.isDead)
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
        if (characterData == null) return;

        // 清理旧的状态图标
        foreach (Transform child in statusEffectsContainer)
        {
            Destroy(child.gameObject);
        }

        // 显示当前状态
        foreach (var status in characterData.statusEffects)
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

            // 鼠标悬停提示
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
        if (characterData == null || characterData.isDead) return;

        Debug.Log($"点击选择角色: {characterData.characterName}");

        // 通知BattleManager选择了哪个角色
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SelectCharacter(characterData);
        }
    }

    // 敌方角色被点击（作为目标）
    void OnEnemyCharacterClicked()
    {
        if (characterData == null || characterData.isDead) return;

        Debug.Log($"点击敌方作为目标: {characterData.characterName}");

        // 检查是否在目标选择模式下
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.IsSelectingTarget())
            {
                // 在目标选择模式下，将此选择为目标
                BattleManager.Instance.SelectTarget(characterData);
            }
            else
            {
                // 非目标选择模式下，显示提示
                Debug.Log($"点击了敌人，但当前非目标选择模式");
            }
        }
    }

    // 设置可被选为目标的状态
    public void SetTargetSelectable(bool selectable)
    {
        Debug.Log($"设置 {characterData.characterName} 的可选目标状态: {selectable}");

        if (targetSelectableBorder == null)
        {
            Debug.LogWarning($"角色 {characterData.characterName} 没有目标边框");
            return;
        }

        targetSelectableBorder.gameObject.SetActive(selectable);

        if (selectable)
        {
            targetSelectableBorder.color = targetSelectableColor;

            // 开启闪烁效果
            StartCoroutine(BlinkTargetBorder());
            Debug.Log($"开始闪烁: {characterData.characterName}");
        }
        else
        {
            StopAllCoroutines();
            Debug.Log($"停止闪烁: {characterData.characterName}");
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

    // 设置已被选为目标的样式
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

    void InitializeTargetSelector()
    {
        if (targetSelector != null)
        {
            targetSelector.owner = this;
        }
    }

    // 实现IPointerClickHandler接口
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (characterData == null || characterData.isDead) return;

        Debug.Log($"点击了角色: {characterData.characterName}");

        // 如果正在选择目标，则响应目标选择
        if (BattleManager.Instance != null && BattleManager.Instance.IsSelectingTarget())
        {
            BattleManager.Instance.OnCharacterClickedAsTarget(characterData);
        }
        else
        {
            // 否则，就是选择角色以显示手牌
            if (isPlayerCharacter && BattleManager.Instance != null)
            {
                BattleManager.Instance.SelectCharacter(characterData);
            }
        }
    }

    // 获取卡牌槽
    public CardSlotUI GetCardSlot(int index)
    {
        if (index >= 0 && index < cardSlots.Length)
        {
            return cardSlots[index];
        }
        return null;
    }
}