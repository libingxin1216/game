using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("战斗数据")]
    public BattleData battleData;

    [Header("UI元素")]
    public Transform playerSideContainer;
    public Transform enemySideContainer;
    public Transform handCardContainer;
    public GameObject characterSlotPrefab;
    public GameObject battleCardPrefab;
    public Button nextTurnButton;
    public Button endTurnButton; // 添加此行
    public Text turnText;
    public CardConsumptionUI cardConsumptionUI;

    [Header("战斗结果UI")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("卡牌选择系统")]
    public GameObject cardSelectionPanelPrefab;

    [Header("当前选中的角色")]
    public CharacterData currentSelectedCharacter;

    [Header("目标选择状态")]
    public CharacterData currentSelectingCharacter; // 当前正在选择目标的角色
    public bool isSelectingTarget = false;

    [Header("卡牌丢弃模式状态")]
    public bool isDiscardMode = false;
    private CardData cardToPlay;
    private CharacterData discardTargetCharacter;
    private int discardSlotIndex;
    private int requiredDiscardCount;
    private CardType requiredCardType;
    private List<CardData> cardsToDiscard = new List<CardData>();

    [Header("背景变暗")]
    [SerializeField] private Image backgroundDimmer;

    public bool IsInDiscardMode => isDiscardMode;

    // 存储每个角色的UI
    private Dictionary<CharacterData, CharacterBattleUI> characterUIs = new Dictionary<CharacterData, CharacterBattleUI>();

    // 存储每个角色的目标
    private Dictionary<CharacterData, CharacterData> characterTargets = new Dictionary<CharacterData, CharacterData>();

    // 公共方法，用于外部检查是否在选择目标
    public bool IsSelectingTarget()
    {
        return isSelectingTarget;
    }

    // 当角色被点击作为目标时调用
    public void OnCharacterClickedAsTarget(CharacterData targetCharacter)
    {
        if (isSelectingTarget)
        {
            SelectTarget(targetCharacter);
        }
    }

    // 选择目标
    public void SelectTarget(CharacterData target)
    {
        if (!isSelectingTarget)
        {
            Debug.LogWarning("不在目标选择模式下，无法选择目标");
            return;
        }

        Debug.Log($"为角色 {currentSelectingCharacter.characterName} 选择了目标: {target.characterName}");

        // 在这里处理卡牌效果
        // ...

        // 重置目标选择状态
        isSelectingTarget = false;
        currentSelectingCharacter = null;

        // 更新UI
        UpdateUI();
    }

    public void OnEnemyTargetSelected(CharacterData enemyCharacter)
    {
        if (isSelectingTarget)
        {
            Debug.Log($"Target selected: {enemyCharacter.characterName}");
            // TODO: Apply card effect to the target

            isSelectingTarget = false;
            currentSelectingCharacter = null;
        }
        else
        { 
            Debug.Log($"Clicked on {enemyCharacter.characterName} but not in target selection mode.");
        }
    } // 是否正在选择目标

    private CardSelectionSystem cardSelectionSystem;
    private bool isInitialized = false;

    public void CheckBattleEnd()
    {
        bool allEnemiesDead = battleData.enemyTeam.TrueForAll(e => e.isDead);
        bool allPlayersDead = battleData.playerTeam.TrueForAll(p => p.isDead);

        if (allEnemiesDead)
        {
            Debug.Log("战斗胜利！");
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }
        else if (allPlayersDead)
        {
            Debug.Log("战斗失败！");
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }
    }

    void Awake()
    {
        Debug.Log("=== BattleManager Awake ===");

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("BattleManager实例创建");

            // 在此初始化数据，而不是等Start
            InitializeWithTestData();
            isInitialized = true;
        }
        else
        {
            Debug.Log("发现重复的BattleManager，销毁");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("=== BattleManager Start ===");

        if (nextTurnButton != null)
        {
            nextTurnButton.onClick.AddListener(EndPlayerTurn);
            Debug.Log("下一回合按钮事件绑定");
        }
        else
        {
            Debug.LogError("下一回合按钮未设置！");
        }

        // 初始化卡牌选择系统
        InitializeCardSelectionSystem();

        // 确保UI已创建
        CreateBattleUI();
        UpdateUI();

        // 默认选中的第一个我方角色
        if (battleData != null && battleData.playerTeam.Count > 0)
        {
            SelectCharacter(battleData.playerTeam[0]);
        }
        EnsureEventSystem();
    }

    public void EndEnemyTurn()
    {
        Debug.Log("=== 敌方回合结束，切换到我方回合 ===");
        battleData.currentTurn++;
        battleData.isPlayerTurn = true;
        UpdateUI();
        StartCoroutine(StartPlayerTurnRoutine());
    }

    void InitializeCardSelectionSystem()
    {
        // 场景中可能已经有CardSelectionSystem在使用
        cardSelectionSystem = FindObjectOfType<CardSelectionSystem>();

        if (cardSelectionSystem == null && cardSelectionPanelPrefab != null)
        {
            GameObject selectionPanelObj = Instantiate(cardSelectionPanelPrefab, transform);
            cardSelectionSystem = selectionPanelObj.GetComponent<CardSelectionSystem>();

            if (cardSelectionSystem != null)
            {
                Debug.Log("成功创建卡牌选择系统");
            }
            else
            {
                Debug.LogError("选择面板预制件没有CardSelectionSystem组件");
            }
        }
    }

    // 使用测试数据初始化战斗
    void InitializeWithTestData()
    {
        Debug.Log("=== 初始化测试战斗数据 ===");

        // 获取所有角色作为我方队伍
        List<CharacterData> playerTeam = new List<CharacterData>();
        List<CharacterData> enemyTeam = new List<CharacterData>();

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager未找到！");
            // 紧急创建临时GameDataManager
            GameObject go = new GameObject("TempGameDataManager");
            go.AddComponent<GameDataManager>();
        }

        var allCharacters = GameDataManager.Instance.GetAllCharacters();
        Debug.Log($"从GameDataManager获取到 {allCharacters.Count} 个角色");

        if (allCharacters.Count == 0)
        {
            Debug.LogError("没有角色数据，创建测试角色...");
            // 创建测试角色
            playerTeam.Add(CreateTestCharacter("测试角色1", Color.red));
            playerTeam.Add(CreateTestCharacter("测试角色2", Color.green));
            playerTeam.Add(CreateTestCharacter("测试角色3", Color.blue));
        }
        else
        {
            // 前3个角色作为我方队伍
            for (int i = 0; i < Mathf.Min(3, allCharacters.Count); i++)
            {
                playerTeam.Add(allCharacters[i]);
                Debug.Log($"我方队伍添加: {allCharacters[i].characterName}");
            }
        }

        // 创建随机敌人
        for (int i = 0; i < 3; i++)
        {
            var enemyCharacter = CreateTestCharacter($"敌人{i + 1}", Color.gray);
            enemyTeam.Add(enemyCharacter);
            Debug.Log($"敌方队伍添加: {enemyCharacter.characterName}");
        }

        battleData = new BattleData();
        battleData.Initialize(playerTeam, enemyTeam);

        // 为每个角色抽初始手牌
        foreach (var character in playerTeam)
        {
            DrawInitialHand(character, 5);
        }

        foreach (var character in enemyTeam)
        {
            DrawInitialHand(character, 5);
        }

        Debug.Log("战斗数据初始化完成");
    }

    CharacterData CreateTestCharacter(string name, Color color)
    {
        CharacterData character = new CharacterData(
            $"Test_{name}",
            name,
            "测试角色",
            CharacterClass.Universal,
            color
        );

        character.Initialize();
        character.currentHealth = 100;

        return character;
    }

    void DrawInitialHand(CharacterData character, int cardCount)
    {
        // 清空手牌
        if (character.handCards == null)
        {
            character.handCards = new List<CardData>();
        }
        else
        {
            character.handCards.Clear();
        }

        // 创建一些测试卡牌
        for (int i = 0; i < cardCount; i++)
        {
            CardData testCard = new CardData(
                $"TestCard_{i}",
                $"测试卡{i + 1}",
                Random.Range(0, 3),
                "测试卡牌描述",
                CardType.Attack,
                CharacterClass.Universal
            );

            // 随机颜色
            testCard.cardColor = new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f)
            );

            character.handCards.Add(testCard);
        }

        Debug.Log($"{character.characterName} 抽了 {character.handCards.Count} 张手牌");
    }

    void CreateBattleUI()
    {
        Debug.Log("=== 创建战斗UI ===");

        if (characterSlotPrefab == null)
        {
            Debug.LogError("CharacterSlot预制件未设置！");
            return;
        }

        if (playerSideContainer == null)
        {
            Debug.LogError("我方容器未设置！");
            return;
        }

        if (enemySideContainer == null)
        {
            Debug.LogError("敌方容器未设置！");
            return;
        }

        // 清理旧的UI
        foreach (Transform child in playerSideContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemySideContainer)
        {
            Destroy(child.gameObject);
        }

        // 创建我方角色UI
        Debug.Log($"创建 {battleData.playerTeam.Count} 个我方角色UI");
        for (int i = 0; i < battleData.playerTeam.Count; i++)
        {
            CreateCharacterSlotUI(battleData.playerTeam[i], playerSideContainer, i, true);
        }

        // 创建敌方角色UI
        Debug.Log($"创建 {battleData.enemyTeam.Count} 个敌方角色UI");
        for (int i = 0; i < battleData.enemyTeam.Count; i++)
        {
            CreateCharacterSlotUI(battleData.enemyTeam[i], enemySideContainer, i, false);
        }

        Debug.Log("战斗UI创建完成");
    }

    void CreateCharacterSlotUI(CharacterData character, Transform parent, int index, bool isPlayer)
    {
        if (characterSlotPrefab == null)
        {
            Debug.LogError("无法创建角色槽位：预制件为空");
            return;
        }

        if (parent == null)
        {
            Debug.LogError("无法创建角色槽位：父容器为空");
            return;
        }

        GameObject slotObj = Instantiate(characterSlotPrefab, parent);
        slotObj.name = $"{character.characterName}_Slot";

        // 设置位置和旋转
        slotObj.transform.localPosition = Vector3.zero;
        slotObj.transform.localRotation = Quaternion.identity;
        slotObj.transform.localScale = Vector3.one;

        // 获取RectTransform组件
        RectTransform rect = slotObj.GetComponent<RectTransform>();

        // 设置锚点
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);

        // 设置位置 - 垂直排列
        float yPosition = -index * 220f;
        rect.anchoredPosition = new Vector2(0, yPosition);

        // 设置大小
        if (isPlayer)
        {
            rect.sizeDelta = new Vector2(180, 250);
        }
        else
        {
            rect.sizeDelta = new Vector2(160, 220);
        }

        // 获取CharacterBattleUI组件并初始化
        CharacterBattleUI battleUI = slotObj.GetComponent<CharacterBattleUI>();
        if (battleUI != null)
        {
            characterUIs[character] = battleUI; // 添加到字典中
            battleUI.Initialize(character, isPlayer);
            Debug.Log($"创建角色UI: {character.characterName} (玩家: {isPlayer}) 位置: {rect.anchoredPosition}");
        }
        else
        {
            Debug.LogWarning($"角色 {character.characterName} 的UI组件未找到");
        }
    }

    void UpdateUI()
    {
        Debug.Log("更新UI");

        // 更新回合信息
        if (turnText != null)
        {
            turnText.text = $"第 {battleData.currentTurn} 回合 - " +
                          (battleData.isPlayerTurn ? "你的回合" : "敌方回合");
        }

        // 更新下一回合按钮状态
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = battleData.isPlayerTurn && CheckIfPlayerCanEndTurn();
        }

        // 更新所有角色UI
        UpdateAllCharacterUI();
        UpdateBackgroundDimmer();
    }

    void UpdateAllCharacterUI()
    {
        foreach (var kvp in characterUIs)
        {
            var character = kvp.Key;
            var ui = kvp.Value;

            ui.UpdateUI();

            // 控制TargetSelector的显隐
            if (ui.targetSelector != null)
            {
                bool isSelected = (character == currentSelectedCharacter);
                ui.targetSelector.gameObject.SetActive(isSelected);
            }
        }
    }

    void DisplayHandCards(CharacterData character)
    {
        if (handCardContainer == null)
        {
            Debug.LogError("手牌容器未设置！");
            return;
        }

        if (battleCardPrefab == null)
        {
            Debug.LogError("战斗卡牌预制件未设置！");
            return;
        }

        // 清理旧手牌
        foreach (Transform child in handCardContainer)
        {
            Destroy(child.gameObject);
        }

        if (character.handCards == null || character.handCards.Count == 0)
        {
            Debug.Log($"{character.characterName} 没有手牌");
            return;
        }

        Debug.Log($"显示 {character.characterName} 的手牌 ({character.handCards.Count}张)");

        // 显示新牌，水平居中
        float spacing = 140f;
        float startX = -spacing * (character.handCards.Count - 1) / 2f;

        for (int i = 0; i < character.handCards.Count; i++)
        {
            CardData card = character.handCards[i];
            GameObject cardObj = Instantiate(battleCardPrefab, handCardContainer);

            // 设置位置
            RectTransform rect = cardObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(startX + (i * spacing), 0);

            BattleCardUI cardUI = cardObj.GetComponent<BattleCardUI>();
            if (cardUI != null)
            {
                cardUI.Initialize(card, character);
                Debug.Log($"创建卡牌 {i}: {card.cardName}");
            }
        }

        UpdateHandCardHighlights();
    }

    public void DiscardCards(List<CardData> cardsToDiscard)
    {
        if (currentSelectedCharacter == null) return;

        foreach (var card in cardsToDiscard)
        {
            currentSelectedCharacter.handCards.Remove(card);
        }

        DisplayHandCards(currentSelectedCharacter);
    }
    public void AttemptToPlaceCard(CardData card, CharacterData targetCharacter, int slotIndex)
    {
        if (isDiscardMode) return; // 如果已在丢弃模式，则忽略新的放置尝试

        var discardRequirement = card.consumptionRequirements.Find(r => r.consumptionType == CardConsumptionType.Discard);

        if (discardRequirement != null && discardRequirement.requiredCount > 0)
        {
            Debug.Log($"卡牌 {card.cardName} 需要弃掉 {discardRequirement.requiredCount} 张类型为 {discardRequirement.requiredCardType} 的牌");
            cardConsumptionUI.Show(card, targetCharacter, slotIndex, discardRequirement.requiredCount, discardRequirement.requiredCardType);
        }
        else
        {
            Debug.Log($"卡牌 {card.cardName} 无需弃牌，直接放置");
            PlaceCardInSlot(card, targetCharacter, slotIndex, new List<CardData>());
        }
    }

    // 进入弃牌模式
    public void EnterDiscardMode(CardData card, CharacterData target, int slotIndex, int count, CardType type)
    {
        isDiscardMode = true;
        cardToPlay = card;
        discardTargetCharacter = target;
        discardSlotIndex = slotIndex;
        requiredDiscardCount = count;
        requiredCardType = type;
        cardsToDiscard.Clear();

        Debug.Log($"进入弃牌模式: 需要弃掉 {count} 张 {type} 类型的牌来打出 {card.cardName}");

        // 更新UI，高亮可弃置的卡牌
        UpdateHandCardHighlights();
        UpdateBackgroundDimmer();
    }

    public void PlayCard(CardData card)
    {
        var discardRequirement = card.consumptionRequirements.Find(r => r.consumptionType == CardConsumptionType.Discard);

        if (discardRequirement != null)
        {
            EnterDiscardMode(card, null, 0, discardRequirement.requiredCount, discardRequirement.requiredCardType);
        }
        else
        {
            //  如果没有消耗需求，直接执行卡牌效果
            //  ExecuteCardEffect(card);
        }
    }

    private void UpdateBackgroundDimmer()
    {
        if (backgroundDimmer != null)
        {
            backgroundDimmer.gameObject.SetActive(isDiscardMode || (cardConsumptionUI != null && cardConsumptionUI.IsConsumptionPanelActive));
        }
    }

    // 当卡牌被选择用于丢弃时调用
    public void OnCardSelectedForDiscard(CardData card)
    {
        if (!isDiscardMode && !cardConsumptionUI.IsConsumptionPanelActive) return;

        cardConsumptionUI.SelectCardForDiscard(card);
        UpdateHandCardHighlights();
    }

    // 退出弃牌模式
    public void ExitDiscardMode(bool cardReturnedToHand = false)
    {
        isDiscardMode = false;
        cardsToDiscard.Clear();
        UpdateHandCardHighlights();
        UpdateBackgroundDimmer();

        if (cardReturnedToHand)
        {
            // 可选：如果取消弃牌，将卡牌放回手牌的视觉效果
            Debug.Log($"取消打出卡牌 {cardToPlay.cardName}，返回手牌");
        }

        cardToPlay = null;
        discardTargetCharacter = null;
    }

    private void UpdateHandCardHighlights()
    {
        var handCardsUI = handCardContainer.GetComponentsInChildren<BattleCardUI>();
        foreach (var cardUI in handCardsUI)
        {
            bool canBeDiscarded = isDiscardMode && cardUI.cardData.cardType == requiredCardType;
            bool isSelectedForDiscard = cardsToDiscard.Contains(cardUI.cardData);
            cardUI.SetHighlight(canBeDiscarded, isSelectedForDiscard);
        }
    }

    // 放置卡牌到槽位
    public bool PlaceCardInSlot(CardData card, CharacterData targetCharacter, int slotIndex, List<CardData> discardedCards)
    {
        if (battleData == null || card == null || targetCharacter == null)
        {
            Debug.LogError("放置卡牌参数错误");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= 3)
        {
            Debug.LogError($"无效的卡槽索引: {slotIndex}");
            return false;
        }

        // 检查目标角色是否有效
        if (!battleData.characterSlots.ContainsKey(targetCharacter))
        {
            Debug.LogError($"目标角色 {targetCharacter.characterName} 没有卡槽数据");
            return false;
        }

        var slots = battleData.characterSlots[targetCharacter];

        // 直接替换卡牌
        slots.slots[slotIndex] = card;

        Debug.Log($"卡牌 {card.cardName} 放置到 {targetCharacter.characterName} 的卡槽 {slotIndex}");
        Debug.Log($"当前卡槽状态: [0]={slots.slots[0]?.cardName}, [1]={slots.slots[1]?.cardName}, [2]={slots.slots[2]?.cardName}");

        // 从手牌中移除被丢弃的卡牌
        foreach (var discardedCard in discardedCards)
        {
            if (currentSelectedCharacter.handCards.Contains(discardedCard))
            {
                currentSelectedCharacter.handCards.Remove(discardedCard);
            }
        }

        // 从手牌中移除卡牌
        if (currentSelectedCharacter != null && currentSelectedCharacter.handCards.Contains(card))
        {
            currentSelectedCharacter.handCards.Remove(card);
            Debug.Log($"从 {currentSelectedCharacter.characterName} 的手牌中移除了 {card.cardName}");
        }

        DisplayHandCards(currentSelectedCharacter); // 重新显示手牌

        // 更新角色UI显示
        UpdateCharacterSlotUI(targetCharacter);

        // 检查是否需要选择目标
        CheckForTargetSelection(targetCharacter);

        // 更新全局UI
        UpdateUI();



        return true;
    }

    // 更新角色的卡槽UI显示
    void UpdateCharacterSlotUI(CharacterData character)
    {
        // 寻找角色的UI
        CharacterBattleUI characterUI = FindCharacterUI(character);
        if (characterUI != null && battleData.characterSlots.TryGetValue(character, out var slots))
        {
            // 更新卡槽显示
            for (int i = 0; i < 3; i++)
            {
                CardSlotUI slotUI = characterUI.GetCardSlot(i);
                if (slotUI != null)
                {
                    if (slots.slots[i] != null)
                    {
                        slotUI.PlaceCard(slots.slots[i]);
                    }
                    else
                    {
                        slotUI.ClearCard();
                    }
                }
            }
        }
    }

    // 寻找角色UI
    CharacterBattleUI FindCharacterUI(CharacterData character)
    {
        var allUIs = FindObjectsOfType<CharacterBattleUI>();
        foreach (var ui in allUIs)
        {
            if (ui.characterData == character)
            {
                return ui;
            }
        }
        return null;
    }

    // 检查是否需要进入目标选择模式
    void CheckForTargetSelection(CharacterData character)
    {
        var slots = battleData.characterSlots[character];
        if (slots.IsReadyToActivate())
        {
            // 检查是否有需要选择目标的卡牌
            foreach (var card in slots.slots)
            {
                if (card != null && card.requiresTarget)
                {
                    EnterTargetSelectionMode(character);
                    return;
                }
            }

            // 如果没有需要选择目标的卡牌，直接激活
            ActivateCharacterSkills(character);
        }
    }

    // 进入目标选择模式
    void EnterTargetSelectionMode(CharacterData character)
    {
        isSelectingTarget = true;
        currentSelectingCharacter = character;
        Debug.Log($"{character.characterName} 进入目标选择模式");

        // TODO: 提示玩家选择目标
        // 例如，高亮可选目标
    }

    // 激活角色技能
    void ActivateCharacterSkills(CharacterData character)
    {
        Debug.Log($"激活 {character.characterName} 的技能");
        var slots = battleData.characterSlots[character];

        // TODO: 执行卡牌效果

        // 清空卡槽
        // slots.Clear();
        UpdateCharacterSlotUI(character);
    }

    // 结束玩家回合
    public void EndPlayerTurn()
    {
        // 清空目标
        characterTargets.Clear();

        if (!battleData.isPlayerTurn)
        {
            Debug.LogWarning("不是你的回合，不能结束回合");
            return;
        }

        Debug.Log("=== 玩家回合结束 ===");

        // 激活所有我方角色的技能
        foreach (var character in battleData.playerTeam)
        {
            if (battleData.characterSlots.ContainsKey(character))
            {
                ActivateCharacterSkills(character);
            }
        }

        // 切换到敌方回合
        battleData.isPlayerTurn = false;
        UpdateUI();

        // 开始敌方回合
        StartCoroutine(EnemyTurnRoutine());
    }

    // 敌方回合逻辑
    IEnumerator EnemyTurnRoutine()
      {
        Debug.Log("=== 敌人回合开始 ===");
        nextTurnButton.interactable = false;

        // 调用BattleAI来执行敌人回合
        if (BattleAI.Instance != null)
        {
            yield return BattleAI.Instance.ExecuteEnemyTurn(battleData);
        }
        else
        {
            Debug.LogError("BattleAI实例未找到！");
            yield return new WaitForSeconds(1f); // 如果AI不存在，至少等待一下
        }

        Debug.Log("=== 敌人回合结束 ===");
        EndEnemyTurn();
    }

    IEnumerator StartPlayerTurnRoutine()
    {
        Debug.Log("开始玩家回合");

        // 玩家回合开始时的逻辑
        foreach (var player in battleData.playerTeam)
        {
            if (!player.isDead)
            {
                player.DrawCard();
            }
        }

        UpdateUI();

        // 启用回合结束按钮
        if (endTurnButton != null)
        {
            endTurnButton.interactable = true;
        }

        yield return null;
    }

    // 设置角色目标
    public void SetCharacterTarget(CharacterData character, CharacterData target)
    {
        if (battleData.playerTeam.Contains(character))
        {
            characterTargets[character] = target;
            Debug.Log($"角色 {character.characterName} 的目标设置为 {target.characterName}");

            // 检查是否所有玩家角色都已设置目标
            CheckAllTargetsSet();
        }
    }

    private void CheckAllTargetsSet()
    {
        foreach (var playerChar in battleData.playerTeam)
        {
            if (!characterTargets.ContainsKey(playerChar) || characterTargets[playerChar] == null)
            {
                // 只要有一个角色没设置目标，就返回
                return;
            }
        }

        // 如果所有角色都设置了目标，则高亮结束回合按钮
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = true;
        }
    }

    // 检查玩家是否可以结束回合
    bool CheckIfPlayerCanEndTurn()
    {
        // 必须所有角色都设置了目标
        foreach (var playerChar in battleData.playerTeam)
        {
            if (!characterTargets.ContainsKey(playerChar) || characterTargets[playerChar] == null)
            {
                return false;
            }
        }
        return true;
    }

    // 选择角色
    public void SelectCharacter(CharacterData character)
    {
        if (character == null)
        {
            Debug.LogWarning("尝试选择一个空角色");
            return;
        }

        // 只能选择我方角色
        if (!battleData.playerTeam.Contains(character))
        {
            Debug.Log($"不能选择敌方角色 {character.characterName}");
            return;
        }

        // 强制退出目标选择模式
        isSelectingTarget = false;

        // 强制退出目标选择模式
        isSelectingTarget = false;

        currentSelectedCharacter = character;
        Debug.Log($"选择了角色: {character.characterName}");

        // 更新手牌显示
        DisplayHandCards(character);

        // 更新UI以高亮显示选中的角色
        UpdateAllCharacterUI();
    }

    // 确保场景中有EventSystem
    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("创建了EventSystem");
        }
    }
}