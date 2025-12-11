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

    // 游戏数据
    private int playerHealth = 3;
    private int enemyHealth = 3;
    private List<int> playerCards = new List<int>();
    private List<int> enemyCards = new List<int>();
    private bool isPlayerTurn = true;
    private bool gameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("CardGameManager实例创建");
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
        // 初始化生命值
        playerHealth = 3;
        enemyHealth = 3;

        // 初始化卡牌（每张卡用数字代表：1=扣1血，2=扣2血，3=加2血）
        playerCards.Clear();
        enemyCards.Clear();

        // 双方各有三张相同的牌
        playerCards.Add(1); // 扣1血
        playerCards.Add(2); // 扣2血
        playerCards.Add(3); // 加2血

        enemyCards.Add(1);
        enemyCards.Add(2);
        enemyCards.Add(3);

        // 设置回合
        isPlayerTurn = true;
        gameOver = false;

        // 更新状态文本
        statusText.text = "玩家回合 - 请选择一张牌";
        Debug.Log("初始化完成，玩家回合开始");
    }

    void UpdateUI()
    {
        // 更新生命值显示
        playerHealthText.text = $"玩家生命值: {playerHealth}";
        enemyHealthText.text = $"敌方生命值: {enemyHealth}";
        Debug.Log($"更新UI: 玩家生命{playerHealth}, 敌方生命{enemyHealth}");

        // 更新卡牌按钮状态
        for (int i = 0; i < playerCardButtons.Length; i++)
        {
            if (i < playerCards.Count)
            {
                playerCardButtons[i].gameObject.SetActive(true);
                UpdateButtonText(i);
            }
            else
            {
                playerCardButtons[i].gameObject.SetActive(false);
            }

            // 只有在玩家回合且游戏未结束时才能点击
            playerCardButtons[i].interactable = isPlayerTurn && !gameOver;
        }
    }

    void UpdateButtonText(int index)
    {
        if (index >= playerCards.Count) return;

        int cardType = playerCards[index];
        string buttonText = "";

        switch (cardType)
        {
            case 1:
                buttonText = "扣1血";
                break;
            case 2:
                buttonText = "扣2血";
                break;
            case 3:
                buttonText = "加2血";
                break;
        }

        playerCardButtons[index].GetComponentInChildren<Text>().text = buttonText;
    }

    // 玩家出牌
    public void PlayCard(int cardIndex)
    {
        Debug.Log($"玩家试图出牌，索引: {cardIndex}, 当前回合: {(isPlayerTurn ? "玩家" : "敌方")}");
        if (!isPlayerTurn || gameOver || cardIndex >= playerCards.Count)
        {
            Debug.Log($"出牌条件不满足: isPlayerTurn={isPlayerTurn}, gameOver={gameOver}, cardIndex={cardIndex}, playerCards.Count={playerCards.Count}");
            return;
        }

        int cardType = playerCards[cardIndex];
        Debug.Log($"玩家出牌: {cardType}");
        ExecuteCardEffect(cardType, true);
        playerCards.RemoveAt(cardIndex);

        // 检查是否游戏结束
        CheckGameOver();

        // 切换到敌方回合
        if (!gameOver)
        {
            isPlayerTurn = false;
            statusText.text = "敌方回合";
            Debug.Log("切换到敌方回合");
            StartCoroutine(EnemyTurn());
        }
        else
        {
            Debug.Log("游戏结束，不再切换到敌方回合");
        }

        UpdateUI();
    }

    // 执行卡牌效果
    void ExecuteCardEffect(int cardType, bool isPlayerUsing)
    {
        Debug.Log($"执行卡牌效果: 类型{cardType}, 是玩家使用?{isPlayerUsing}");
        switch (cardType)
        {
            case 1: // 扣1血
                if (isPlayerUsing)
                {
                    enemyHealth = Mathf.Max(0, enemyHealth - 1);
                    statusText.text = "玩家使用：扣除敌方1点生命值";
                }
                else
                {
                    playerHealth = Mathf.Max(0, playerHealth - 1);
                    statusText.text = "敌方使用：扣除玩家1点生命值";
                }
                break;

            case 2: // 扣2血
                if (isPlayerUsing)
                {
                    enemyHealth = Mathf.Max(0, enemyHealth - 2);
                    statusText.text = "玩家使用：扣除敌方2点生命值";
                }
                else
                {
                    playerHealth = Mathf.Max(0, playerHealth - 2);
                    statusText.text = "敌方使用：扣除玩家2点生命值";
                }
                break;

            case 3: // 加2血
                if (isPlayerUsing)
                {
                    playerHealth += 2;
                    statusText.text = "玩家使用：增加自身2点生命值";
                }
                else
                {
                    enemyHealth += 2;
                    statusText.text = "敌方使用：增加自身2点生命值";
                }
                break;
        }
        Debug.Log($"效果执行后: 玩家生命{playerHealth}, 敌方生命{enemyHealth}");
    }

    // 敌方回合
    IEnumerator EnemyTurn()
    {
        Debug.Log("敌方回合开始");
        // 等待1秒，模拟思考时间
        yield return new WaitForSeconds(1.5f);

        if (enemyCards.Count > 0 && !gameOver)
        {
            // 随机选择一张牌
            int randomIndex = Random.Range(0, enemyCards.Count);
            int cardType = enemyCards[randomIndex];
            Debug.Log($"敌方出牌: 索引{randomIndex}, 类型{cardType}");

            ExecuteCardEffect(cardType, false);
            enemyCards.RemoveAt(randomIndex);

            // 检查是否游戏结束
            CheckGameOver();

            // 切换回玩家回合
            if (!gameOver)
            {
                isPlayerTurn = true;
                statusText.text = "玩家回合 - 请选择一张牌";
                Debug.Log("切换回玩家回合");
            }
            else
            {
                Debug.Log("游戏结束，不再切换回玩家回合");
            }
        }
        else
        {
            Debug.Log($"敌方无牌可出或游戏结束: enemyCards.Count={enemyCards.Count}, gameOver={gameOver}");
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
            {
                statusText.text = "平局！双方生命值都为0";
            }
            else if (playerHealth <= 0)
            {
                statusText.text = "游戏结束！敌方胜利";
            }
            else if (enemyHealth <= 0)
            {
                statusText.text = "游戏结束！玩家胜利";
            }
            Debug.Log("游戏结束，生命值为0");
            return;
        }

        // 检查卡牌是否用完
        if (playerCards.Count == 0 && enemyCards.Count == 0)
        {
            gameOver = true;
            if (playerHealth > enemyHealth)
            {
                statusText.text = $"游戏结束！玩家胜利 ({playerHealth} vs {enemyHealth})";
            }
            else if (enemyHealth > playerHealth)
            {
                statusText.text = $"游戏结束！敌方胜利 ({playerHealth} vs {enemyHealth})";
            }
            else
            {
                statusText.text = $"平局！双方生命值相同 ({playerHealth} vs {enemyHealth})";
            }
            Debug.Log("游戏结束，卡牌用完");
        }
    }

    // 重新开始游戏
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        InitializeGame();
        UpdateUI();
    }
}