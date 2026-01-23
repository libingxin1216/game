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

    [Header("UI引用")]
    public Transform playerSideContainer;
    public Transform enemySideContainer;
    public Transform handCardContainer;
    public GameObject characterSlotPrefab;
    public GameObject battleCardPrefab;
    public Button nextTurnButton;
    public Text turnText;

    [Header("卡牌选择系统")]
    public GameObject cardSelectionPanelPrefab;

    [Header("当前选中的角色")]
    public CharacterData currentSelectedCharacter;

    [Header("目标选择状态")]
    public CharacterData currentSelectingCharacter; // 当前正在选择目标的角色
    public bool isSelectingTarget = false; // 是否正在选择目标

    private CardSelectionSystem cardSelectionSystem;
    private bool isInitialized = false;

    void Awake()
    {
        Debug.Log("=== BattleManager Awake ===");

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("BattleManager实例创建");

            // 立即初始化数据，不等待Start
            InitializeWithTestData();
            isInitialized = true;
        }
        else
        {
            Debug.Log("存在重复的BattleManager，销毁");
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

        // 默认选中第一个玩家角色
        if (battleData != null && battleData.playerTeam.Count > 0)
        {
            SelectCharacter(battleData.playerTeam[0]);
        }
        EnsureEventSystem();
    }

    void InitializeCardSelectionSystem()
    {
        // 如果场景中已经有CardSelectionSystem，使用它
        cardSelectionSystem = FindObjectOfType<CardSelectionSystem>();

        if (cardSelectionSystem == null && cardSelectionPanelPrefab != null)
        {
            GameObject selectionPanelObj = Instantiate(cardSelectionPanelPrefab, transform);
            cardSelectionSystem = selectionPanelObj.GetComponent<CardSelectionSystem>();

            if (cardSelectionSystem != null)
            {
                Debug.Log("创建卡牌选择系统");
            }
            else
            {
                Debug.LogError("卡牌选择面板没有CardSelectionSystem组件");
            }
        }
    }

    // 使用测试数据初始化战斗
    void InitializeWithTestData()
    {
        Debug.Log("=== 初始化测试战斗数据 ===");

        // 获取所有角色作为玩家队伍
        List<CharacterData> playerTeam = new List<CharacterData>();
        List<CharacterData> enemyTeam = new List<CharacterData>();

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager未找到！");
            // 创建临时的GameDataManager
            GameObject go = new GameObject("TempGameDataManager");
            go.AddComponent<GameDataManager>();
        }

        var allCharacters = GameDataManager.Instance.GetAllCharacters();
        Debug.Log($"从GameDataManager获取到 {allCharacters.Count} 个角色");

        if (allCharacters.Count == 0)
        {
            Debug.LogError("没有角色数据！创建测试角色...");
            // 创建测试角色
            playerTeam.Add(CreateTestCharacter("测试角色1", Color.red));
            playerTeam.Add(CreateTestCharacter("测试角色2", Color.green));
            playerTeam.Add(CreateTestCharacter("测试角色3", Color.blue));
        }
        else
        {
            // 前3个角色作为玩家队伍
            for (int i = 0; i < Mathf.Min(3, allCharacters.Count); i++)
            {
                playerTeam.Add(allCharacters[i]);
                Debug.Log($"玩家队伍添加: {allCharacters[i].characterName}");
            }
        }

        // 创建敌人队伍
        for (int i = 0; i < 3; i++)
        {
            var enemyCharacter = CreateTestCharacter($"敌人{i + 1}", Color.gray);
            enemyTeam.Add(enemyCharacter);
            Debug.Log($"敌人队伍添加: {enemyCharacter.characterName}");
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

        // 添加一些测试卡牌
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
            Debug.LogError("CharacterSlot预制体未设置！");
            return;
        }

        if (playerSideContainer == null)
        {
            Debug.LogError("玩家侧容器未设置！");
            return;
        }

        if (enemySideContainer == null)
        {
            Debug.LogError("敌人侧容器未设置！");
            return;
        }

        // 清空现有UI
        foreach (Transform child in playerSideContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemySideContainer)
        {
            Destroy(child.gameObject);
        }

        // 创建玩家角色UI
        Debug.Log($"创建 {battleData.playerTeam.Count} 个玩家角色UI");
        for (int i = 0; i < battleData.playerTeam.Count; i++)
        {
            CreateCharacterSlotUI(battleData.playerTeam[i], playerSideContainer, i, true);
        }

        // 创建敌人角色UI
        Debug.Log($"创建 {battleData.enemyTeam.Count} 个敌人角色UI");
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
            Debug.LogError("无法创建角色槽位：预制体为空");
            return;
        }

        if (parent == null)
        {
            Debug.LogError("无法创建角色槽位：父容器为空");
            return;
        }

        GameObject slotObj = Instantiate(characterSlotPrefab, parent);
        slotObj.name = $"{character.characterName}_Slot";

        // 重置位置和旋转
        slotObj.transform.localPosition = Vector3.zero;
        slotObj.transform.localRotation = Quaternion.identity;
        slotObj.transform.localScale = Vector3.one;

        // 获取RectTransform组件
        RectTransform rect = slotObj.GetComponent<RectTransform>();

        // 重置锚点
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
                          (battleData.isPlayerTurn ? "玩家回合" : "敌人回合");
        }

        // 更新下一回合按钮状态
        if (nextTurnButton != null)
        {
            nextTurnButton.interactable = battleData.isPlayerTurn && CheckIfPlayerCanEndTurn();
        }

        // 更新所有角色UI
        UpdateAllCharacterUI();
    }

    void UpdateAllCharacterUI()
    {
        // 更新所有CharacterBattleUI组件
        var allBattleUIs = FindObjectsOfType<CharacterBattleUI>();
        Debug.Log($"找到 {allBattleUIs.Length} 个CharacterBattleUI组件");

        foreach (var battleUI in allBattleUIs)
        {
            battleUI.UpdateUI();
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
            Debug.LogError("战斗卡牌预制体未设置！");
            return;
        }

        // 清空手牌容器
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

        // 显示手牌（水平排列）
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
                Debug.Log($"创建手牌 {i}: {card.cardName}");
            }
        }
    }



    // 放置卡牌到卡槽
    // 修改卡牌放置方法，确保触发检测
    public bool PlaceCardInSlot(CardData card, CharacterData targetCharacter, int slotIndex)
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

        // 立即更新UI显示
        UpdateCharacterSlotUI(targetCharacter);

        // 立即检查是否需要选择目标
        CheckForTargetSelection(targetCharacter);

        // 更新全局UI
        UpdateUI();

        return true;
    }

    // 更新角色的卡槽UI显示
    void UpdateCharacterSlotUI(CharacterData character)
    {
        // 查找角色的UI
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

    // 查找角色UI
    CharacterBattleUI FindCharacterUI(CharacterData character)
    {
        var allCharacters = FindObjectsOfType<CharacterBattleUI>();
        foreach (var charUI in allCharacters)
        {
            if (charUI.CharacterData == character)
            {
                return charUI;
            }
        }
        return null;
    }

    public void OnCharacterClickedAsTarget(CharacterData target)
    {
        if (!isSelectingTarget || currentSelectingCharacter == null || target == null)
        {
            Debug.LogWarning("无法选择目标：不在目标选择模式");
            return;
        }

        // 检查目标是否有效
        if (target.isDead)
        {
            Debug.LogWarning("不能选择已死亡的目标");
            return;
        }

        // 检查目标是否是敌人（玩家只能选择敌人作为目标）
        if (!battleData.enemyTeam.Contains(target))
        {
            Debug.LogWarning("只能选择敌方目标");
            return;
        }

        // 记录目标
        if (battleData.characterSlots.TryGetValue(currentSelectingCharacter, out var slots))
        {
            slots.target = target;

            Debug.Log($"目标选择完成: {currentSelectingCharacter.characterName} → {target.characterName}");

            // 清除高亮
            HighlightSelectingCharacter(false);
            HighlightAvailableTargets(false);

            // 重置状态
            currentSelectingCharacter = null;
            isSelectingTarget = false;

            // 更新目标选择状态的UI
            UpdateTargetSelectionUI();

            // 检查是否可以结束回合
            UpdateUI();

            // 显示成功信息
            ShowTargetSelectionHint($"目标选择完成: {target.characterName}", true);
        }
    }

    void UpdateTargetSelectionUI()
    {
        // 更新所有角色卡槽的目标显示
        foreach (var character in battleData.playerTeam)
        {
            if (battleData.characterSlots.TryGetValue(character, out var slots))
            {
                CharacterBattleUI characterUI = FindCharacterUI(character);
                if (characterUI != null && slots.target != null)
                {
                    // 显示已选择的目标
                    characterUI.SetAsSelectedTarget(true);

                    // 同时更新目标角色的显示
                    CharacterBattleUI targetUI = FindCharacterUI(slots.target);
                    if (targetUI != null)
                    {
                        targetUI.SetAsSelectedTarget(true);
                    }
                }
            }
        }
    }

    public void EndPlayerTurn()
    {
        Debug.Log("结束玩家回合");

        if (!battleData.isPlayerTurn) return;

        // 切换到敌人回合
        battleData.isPlayerTurn = false;
        battleData.currentTurn++;
        UpdateUI();

        Debug.Log("切换到敌人回合");
    }

    // 选择角色（显示手牌）
    public void SelectCharacter(CharacterData character)
    {
        if (character == null || character.isDead) return;

        Debug.Log($"选择角色显示手牌: {character.characterName}");

        // 如果正在选择目标，先取消
        if (isSelectingTarget)
        {
            CancelTargetSelection();
        }

        currentSelectedCharacter = character;
        DisplayHandCards(character);
    }



    // 撤回卡牌方法（可选）
    public void WithdrawCardFromSlot(CharacterData character, int slotIndex, CardData card)
    {
        if (battleData == null || !battleData.characterSlots.ContainsKey(character))
        {
            Debug.LogError("撤回卡牌参数错误");
            return;
        }

        var slots = battleData.characterSlots[character];

        if (slotIndex < 0 || slotIndex >= 3)
        {
            Debug.LogError($"无效的卡槽索引: {slotIndex}");
            return;
        }

        // 从卡槽移除卡牌
        slots.slots[slotIndex] = null;

        // 添加回手牌
        if (character.handCards != null)
        {
            character.handCards.Add(card);
        }

        // 更新UI
        UpdateCharacterSlotUI(character);

        Debug.Log($"从 {character.characterName} 的卡槽 {slotIndex} 撤回卡牌 {card.cardName}");
    }


    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("创建EventSystem");
        }
    }

    // 检查是否需要选择目标
    // 加强检查逻辑
    void CheckForTargetSelection(CharacterData character)
    {
        if (battleData == null || !battleData.characterSlots.ContainsKey(character))
        {
            Debug.LogWarning($"无法检查目标选择: {character?.characterName}");
            return;
        }

        var slots = battleData.characterSlots[character];

        // 检查卡槽是否都满了
        bool allSlotsFilled = true;
        for (int i = 0; i < 3; i++)
        {
            if (slots.slots[i] == null)
            {
                allSlotsFilled = false;
                Debug.Log($"卡槽 {i} 为空");
                break;
            }
        }

        Debug.Log($"{character.characterName} 卡槽已满: {allSlotsFilled}, 当前目标: {slots.target?.characterName ?? "无"}");

        // 如果卡槽已满且没有目标，进入目标选择模式
        if (allSlotsFilled && slots.target == null)
        {
            Debug.Log($"触发目标选择模式: {character.characterName}");
            StartTargetSelection(character);
        }
        else if (!allSlotsFilled && currentSelectingCharacter == character)
        {
            // 如果取消了一张卡牌，退出目标选择模式
            Debug.Log($"取消目标选择模式: {character.characterName}");
            CancelTargetSelection();
        }
    }

    // 开始目标选择
    void StartTargetSelection(CharacterData character)
    {
        // 如果已经在为目标选择，先取消
        if (isSelectingTarget && currentSelectingCharacter != character)
        {
            CancelTargetSelection();
        }

        currentSelectingCharacter = character;
        isSelectingTarget = true;

        Debug.Log($"=== 进入目标选择模式 ===");
        Debug.Log($"为角色选择目标: {character.characterName}");

        // 高亮显示需要选择目标的角色
        HighlightSelectingCharacter(true);

        // 高亮可选的敌方目标
        HighlightAvailableTargets(true);

        // 显示提示信息
        ShowTargetSelectionHint($"请为 [{character.characterName}] 选择敌方目标", false);
    }

    // 取消目标选择
    void CancelTargetSelection()
    {
        if (!isSelectingTarget) return;

        // 取消高亮
        HighlightSelectingCharacter(false);
        HighlightAvailableTargets(false);

        // 重置状态
        currentSelectingCharacter = null;
        isSelectingTarget = false;

        Debug.Log("取消目标选择");
    }

    // 高亮显示正在选择目标的角色
    void HighlightSelectingCharacter(bool highlight)
    {
        if (currentSelectingCharacter == null) return;

        CharacterBattleUI characterUI = FindCharacterUI(currentSelectingCharacter);
        if (characterUI != null)
        {
            // 添加黄色边框或背景
            Image background = characterUI.GetComponent<Image>();
            if (background != null)
            {
                background.color = highlight ? Color.yellow : Color.white;
            }

            Debug.Log($"{currentSelectingCharacter.characterName} 高亮: {highlight}");
        }
    }

    // 高亮可选的敌方目标
    void HighlightAvailableTargets(bool highlight)
    {
        int highlightCount = 0;

        foreach (var enemy in battleData.enemyTeam)
        {
            if (enemy.isDead)
            {
                Debug.Log($"跳过死亡敌人: {enemy.characterName}");
                continue;
            }

            CharacterBattleUI enemyUI = FindCharacterUI(enemy);
            if (enemyUI != null)
            {
                enemyUI.SetTargetSelectable(highlight);
                highlightCount++;

                if (highlight)
                {
                    Debug.Log($"高亮敌人: {enemy.characterName}");
                }
            }
        }

        Debug.Log($"高亮了 {highlightCount} 个敌人");
    }

    // 选择目标（由CharacterBattleUI调用）
    public void SelectTarget(CharacterData target)
    {
        if (!isSelectingTarget || currentSelectingCharacter == null || target == null)
        {
            Debug.LogWarning("无法选择目标：不在目标选择模式");
            return;
        }

        // 检查目标是否有效
        if (target.isDead)
        {
            Debug.LogWarning("不能选择已死亡的目标");
            return;
        }

        // 检查目标是否是敌人
        if (!battleData.enemyTeam.Contains(target))
        {
            Debug.LogWarning("只能选择敌方目标");
            return;
        }

        // 记录目标
        if (battleData.characterSlots.TryGetValue(currentSelectingCharacter, out var slots))
        {
            slots.target = target;

            Debug.Log($"目标选择完成: {currentSelectingCharacter.characterName} → {target.characterName}");

            // 清除高亮
            HighlightSelectingCharacter(false);
            HighlightAvailableTargets(false);

            // 重置状态
            currentSelectingCharacter = null;
            isSelectingTarget = false;

            // 检查是否可以结束回合
            UpdateUI();

            // 显示成功信息
            ShowTargetSelectionHint($"目标选择完成: {target.characterName}", true);
        }
    }

    // 显示目标选择提示
    void ShowTargetSelectionHint(string message, bool isSuccess = false)
    {
        // 这里可以显示UI提示
        Debug.Log($"目标选择提示: {message}");

        // 如果有turnText，可以用它显示提示
        if (turnText != null)
        {
            string originalText = turnText.text;
            turnText.text = message;

            if (isSuccess)
            {
                // 成功消息显示2秒后恢复
                StartCoroutine(RestoreTurnText(originalText, 2f));
            }
        }
    }

    IEnumerator RestoreTurnText(string originalText, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (turnText != null)
        {
            turnText.text = originalText;
        }
    }

    // 检查是否可以结束回合
    bool CheckIfPlayerCanEndTurn()
    {
        if (battleData == null || !battleData.isPlayerTurn) return false;

        // 如果有角色正在选择目标，不能结束回合
        if (isSelectingTarget)
        {
            Debug.Log("有角色正在选择目标，不能结束回合");
            return false;
        }

        // 检查所有玩家角色
        foreach (var character in battleData.playerTeam)
        {
            if (character.isDead) continue;

            if (battleData.characterSlots.TryGetValue(character, out var slots))
            {
                // 检查卡槽是否都满了
                bool allSlotsFilled = true;
                for (int i = 0; i <= 2; i++)
                {
                    if (slots.slots[i] == null)
                    {
                        allSlotsFilled = false;
                        break;
                    }
                }

                // 如果卡槽满了但没选目标，不能结束回合
                if (allSlotsFilled && slots.target == null)
                {
                    Debug.Log($"角色 {character.characterName} 卡槽已满但未选择目标");
                    return false;
                }
            }
        }

        return true;
    }

    public bool IsSelectingTarget()
    {
        return isSelectingTarget;
    }
}