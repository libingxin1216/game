using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameManager : MonoBehaviour
{
    // 单例模式
    public static CardGameManager Instance { get; private set; }

    // UI引用
    public Text playerHealthText;
    public Text enemyHealthText;
    public Text statusText;
    public Button[] playerCardButtons;
    public Button enemyTargetButton;  // 敌人目标按钮
    public Button cancelButton;  // 取消选择按钮

    // 游戏数据
    private int playerHealth = 3;
    private int enemyHealth = 3;
    private List<int> playerCards = new List<int>();
    private List<int> enemyCards = new List<int>();
    private bool isPlayerTurn = true;
    private bool gameOver = false;

    // 目标选择状态
    private bool isSelectingTarget = false;
    private int selectedCardIndex = -1;
    private int selectedCardType = -1;

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
    }

    void Start()
    {
        Debug.Log("游戏初始化");
        InitializeGame();
        UpdateUI();
    }

    void InitializeGame()
    {
        Debug.Log("初始化游戏");
        playerHealth = 3;
        enemyHealth = 3;

        playerCards.Clear();
        enemyCards.Clear();

        playerCards.Add(1); // 扣1血
        playerCards.Add(2); // 扣2血
        playerCards.Add(3); // 加2血

        enemyCards.Add(1);
        enemyCards.Add(2);
        enemyCards.Add(3);

        isPlayerTurn = true;
        gameOver = false;
        isSelectingTarget = false;
        selectedCardIndex = -1;
        selectedCardType = -1;

        // 初始化敌人目标按钮状态
        UpdateEnemyTargetButton();

        statusText.text = "玩家回合 - 请选择一张牌";
        Debug.Log("初始化完成");
    }

    void UpdateUI()
    {
        Debug.Log($"更新UI: 玩家生命={playerHealth}, 敌人生命={enemyHealth}");

        if (playerHealthText != null)
            playerHealthText.text = $"玩家生命值: {playerHealth}";
        else
            Debug.LogError("playerHealthText为空！");

        if (enemyHealthText != null)
            enemyHealthText.text = $"敌方生命值: {enemyHealth}";
        else
            Debug.LogError("enemyHealthText为空！");

        // 更新卡牌按钮
        for (int i = 0; i < playerCardButtons.Length; i++)
        {
            if (playerCardButtons[i] == null)
            {
                Debug.LogError($"playerCardButtons[{i}]为空！");
                continue;
            }

            if (i < playerCards.Count)
            {
                playerCardButtons[i].gameObject.SetActive(true);
                UpdateButtonText(i);

                // 在选择目标状态时，所有卡牌按钮不可交互
                playerCardButtons[i].interactable = isPlayerTurn && !gameOver && !isSelectingTarget;
            }
            else
            {
                playerCardButtons[i].gameObject.SetActive(false);
            }
        }

        // 在UpdateUI方法末尾添加
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(isSelectingTarget);
            cancelButton.interactable = isSelectingTarget;
        }

        // 更新敌人目标按钮
        UpdateEnemyTargetButton();
    }

    // 更新敌人目标按钮状态
    void UpdateEnemyTargetButton()
    {
        if (enemyTargetButton != null)
        {
            // 按钮一直存在，但只有特定条件下可点击
            bool isInteractable = isSelectingTarget && isPlayerTurn && !gameOver;
            enemyTargetButton.interactable = isInteractable;

            // 使用视觉脚本控制外观
            EnemyTargetButtonVisual visual = enemyTargetButton.GetComponent<EnemyTargetButtonVisual>();
            if (visual != null)
            {
                if (isInteractable)
                {
                    visual.SetSelectableState();
                }
                else
                {
                    visual.SetNormalState();
                }
            }
            else
            {
                // 如果没有视觉脚本，使用简单的颜色变化
                Image buttonImage = enemyTargetButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (isInteractable)
                    {
                        buttonImage.color = new Color(1f, 0f, 0f, 0.3f);
                    }
                    else
                    {
                        buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                    }
                }
            }
        }
    }

    void UpdateButtonText(int index)
    {
        if (index >= playerCards.Count) return;

        int cardType = playerCards[index];
        Text buttonText = playerCardButtons[index].GetComponentInChildren<Text>();

        if (buttonText != null)
        {
            switch (cardType)
            {
                case 1: buttonText.text = "扣1血"; break;
                case 2: buttonText.text = "扣2血"; break;
                case 3: buttonText.text = "加2血"; break;
            }
        }
    }

    // 玩家点击卡牌
    public void PlayCard(int cardIndex)
    {
        Debug.Log($"玩家点击卡牌，索引: {cardIndex}");

        if (!isPlayerTurn)
        {
            Debug.Log("不是玩家回合，不能出牌");
            return;
        }

        if (gameOver)
        {
            Debug.Log("游戏已结束，不能出牌");
            return;
        }

        if (cardIndex >= playerCards.Count)
        {
            Debug.Log($"卡牌索引{cardIndex}超出范围，玩家有{playerCards.Count}张牌");
            return;
        }

        Debug.Log($"玩家选择的卡牌类型: {playerCards[cardIndex]}");
        int cardType = playerCards[cardIndex];

        // 根据卡牌类型处理
        if (cardType == 1 || cardType == 2)
        {
            // 扣血卡牌：进入目标选择状态
            StartTargetSelection(cardIndex, cardType);
        }
        else if (cardType == 3)
        {
            // 加血卡牌：直接生效，不需要选择目标
            ExecuteHealCard(cardIndex);
        }

        UpdateUI();
    }

    // 开始目标选择
    void StartTargetSelection(int cardIndex, int cardType)
    {
        Debug.Log($"开始目标选择，卡牌索引: {cardIndex}, 类型: {cardType}");

        // 进入目标选择状态
        isSelectingTarget = true;
        selectedCardIndex = cardIndex;
        selectedCardType = cardType;

        // 更新状态文本
        statusText.text = "请点击敌方确认攻击";

        UpdateUI();
    }

    // 取消目标选择
    void CancelTargetSelection()
    {
        Debug.Log("取消目标选择");

        isSelectingTarget = false;
        selectedCardIndex = -1;
        selectedCardType = -1;

        // 更新状态文本
        statusText.text = "玩家回合 - 请选择一张牌";

        UpdateUI();
    }

    // 玩家点击敌人目标
    public void OnEnemyTargetClicked()
    {
        Debug.Log("玩家点击了敌人目标");

        if (!isSelectingTarget || selectedCardIndex == -1)
        {
            Debug.Log("不在目标选择状态");
            return;
        }

        // 播放点击反馈效果
        if (enemyTargetButton != null)
        {
            EnemyTargetButtonVisual visual = enemyTargetButton.GetComponent<EnemyTargetButtonVisual>();
            if (visual != null)
            {
                visual.PlayClickEffect();
            }
        }

        // 执行攻击
        ExecuteAttack();
    }

    // 执行攻击
    void ExecuteAttack()
    {
        Debug.Log($"执行攻击，卡牌类型: {selectedCardType}");

        // 执行卡牌效果
        ExecuteCardEffect(selectedCardType, true);

        // 移除使用的卡牌
        playerCards.RemoveAt(selectedCardIndex);

        // 重置选择状态
        isSelectingTarget = false;
        selectedCardIndex = -1;
        selectedCardType = -1;

        // 检查游戏是否结束
        CheckGameOver();

        // 如果游戏未结束，切换到敌方回合
        if (!gameOver)
        {
            isPlayerTurn = false;
            statusText.text = "敌方回合";
            Debug.Log("切换到敌方回合");
            StartCoroutine(EnemyTurn());
        }

        UpdateUI();
    }

    // 执行治疗卡牌
    void ExecuteHealCard(int cardIndex)
    {
        Debug.Log("执行治疗卡牌");

        // 执行卡牌效果
        ExecuteCardEffect(3, true);
        playerCards.RemoveAt(cardIndex);

        // 检查游戏是否结束
        CheckGameOver();

        // 如果游戏未结束，切换到敌方回合
        if (!gameOver)
        {
            isPlayerTurn = false;
            statusText.text = "敌方回合";
            Debug.Log("切换到敌方回合");
            StartCoroutine(EnemyTurn());
        }

        UpdateUI();
    }

    // 执行卡牌效果
    void ExecuteCardEffect(int cardType, bool isPlayerUsing)
    {
        Debug.Log($"执行卡牌效果: 类型{cardType}, 玩家使用: {isPlayerUsing}");

        switch (cardType)
        {
            case 1: // 扣1血
                if (isPlayerUsing)
                {
                    enemyHealth = Mathf.Max(0, enemyHealth - 1);
                    Debug.Log($"敌人生命值减少1，当前: {enemyHealth}");
                    statusText.text = "玩家使用：扣除敌方1点生命值";
                }
                else
                {
                    playerHealth = Mathf.Max(0, playerHealth - 1);
                    Debug.Log($"玩家生命值减少1，当前: {playerHealth}");
                    statusText.text = "敌方使用：扣除玩家1点生命值";
                }
                break;

            case 2: // 扣2血
                if (isPlayerUsing)
                {
                    enemyHealth = Mathf.Max(0, enemyHealth - 2);
                    Debug.Log($"敌人生命值减少2，当前: {enemyHealth}");
                    statusText.text = "玩家使用：扣除敌方2点生命值";
                }
                else
                {
                    playerHealth = Mathf.Max(0, playerHealth - 2);
                    Debug.Log($"玩家生命值减少2，当前: {playerHealth}");
                    statusText.text = "敌方使用：扣除玩家2点生命值";
                }
                break;

            case 3: // 加2血
                if (isPlayerUsing)
                {
                    playerHealth += 2;
                    Debug.Log($"玩家生命值增加2，当前: {playerHealth}");
                    statusText.text = "玩家使用：增加自身2点生命值";
                }
                else
                {
                    enemyHealth += 2;
                    Debug.Log($"敌人生命值增加2，当前: {enemyHealth}");
                    statusText.text = "敌方使用：增加自身2点生命值";
                }
                break;
        }
    }

    // 敌方回合
    IEnumerator EnemyTurn()
    {
        Debug.Log("开始敌方回合");

        // 等待1.5秒让玩家看到回合切换
        yield return new WaitForSeconds(1.5f);

        if (enemyCards.Count > 0 && !gameOver)
        {
            int randomIndex = Random.Range(0, enemyCards.Count);
            int cardType = enemyCards[randomIndex];
            Debug.Log($"敌方出牌: 索引{randomIndex}, 类型{cardType}");

            // 直接执行卡牌效果
            ExecuteCardEffect(cardType, false);
            enemyCards.RemoveAt(randomIndex);

            CheckGameOver();

            if (!gameOver)
            {
                isPlayerTurn = true;
                statusText.text = "玩家回合 - 请选择一张牌";
                Debug.Log("切换回玩家回合");
            }
        }
        else
        {
            Debug.Log("敌方没有卡牌了");
        }

        UpdateUI();
    }

    // 检查游戏是否结束
    void CheckGameOver()
    {
        // 检查生命值
        if (playerHealth <= 0 || enemyHealth <= 0)
        {
            gameOver = true;
            if (playerHealth <= 0 && enemyHealth <= 0)
                statusText.text = "平局！双方生命值都为0";
            else if (playerHealth <= 0)
                statusText.text = "游戏结束！敌方胜利";
            else if (enemyHealth <= 0)
                statusText.text = "游戏结束！玩家胜利";
            Debug.Log("游戏结束 - 生命值为0");
            return;
        }

        // 检查卡牌是否用完
        if (playerCards.Count == 0 && enemyCards.Count == 0)
        {
            gameOver = true;
            if (playerHealth > enemyHealth)
                statusText.text = $"游戏结束！玩家胜利 ({playerHealth} vs {enemyHealth})";
            else if (enemyHealth > playerHealth)
                statusText.text = $"游戏结束！敌方胜利 ({playerHealth} vs {enemyHealth})";
            else
                statusText.text = $"平局！双方生命值相同 ({playerHealth} vs {enemyHealth})";
            Debug.Log("游戏结束 - 卡牌用完");
        }
    }

    // 重新开始游戏
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        InitializeGame();
        UpdateUI();
    }

    // 添加取消选择按钮的方法（可选）
    public void CancelSelection()
    {
        if (isSelectingTarget)
        {
            CancelTargetSelection();
        }
    }
}