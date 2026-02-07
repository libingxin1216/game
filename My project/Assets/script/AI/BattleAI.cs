using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAI : MonoBehaviour
{
    public static BattleAI Instance { get; private set; }

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

    // 执行敌方回合总流程
    public IEnumerator ExecuteEnemyTurn(BattleData battleData)
    {
        Debug.Log("=== 敌人AI回合开始 ===");
        if (BattleManager.Instance != null && BattleManager.Instance.endTurnButton != null)
        {
            BattleManager.Instance.endTurnButton.interactable = false; // AI行动时，禁止玩家操作
        }

        // 依次为每个存活的敌人执行行动
        foreach (var enemy in battleData.enemyTeam)
        {
            if (enemy.isDead) continue;
            yield return StartCoroutine(ExecuteSingleEnemyTurn(enemy, battleData));
        }

        Debug.Log("=== 敌人AI回合结束 ===");
        yield return new WaitForSeconds(1f); // 回合结束前的短暂暂停

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EndEnemyTurn(); // 切换回玩家回合
        }
    }

    // 执行单个敌人的回合，模拟玩家操作
    private IEnumerator ExecuteSingleEnemyTurn(CharacterData enemy, BattleData battleData)
    {
        Debug.Log($"AI: {enemy.characterName} 开始行动...");

        // 1. 模拟思考时间
        float thinkingTime = Random.Range(1.5f, 2.5f);
        Debug.Log($"AI: {enemy.characterName} 思考中... ({thinkingTime:F1}s)");
        yield return new WaitForSeconds(thinkingTime);

        // 2. 决策：选择要出的牌和目标
        CardData cardToPlay = DecideCardToPlay(enemy);
        CharacterData target = DecideTarget(enemy, battleData);

        // 3. 执行行动
        if (cardToPlay != null && target != null)
        {
            Debug.Log($"AI: {enemy.characterName} 决定对 {target.characterName} 使用 {cardToPlay.cardName}.");
            yield return StartCoroutine(PlayCardSequence(enemy, cardToPlay, target, battleData));
        }
        else
        {
            Debug.Log($"AI: {enemy.characterName} 没有可执行的行动，跳过回合。");
            yield return new WaitForSeconds(2f); // 无操作也要有停顿
        }
    }

    // AI决策：决定要出的牌
    private CardData DecideCardToPlay(CharacterData enemy)
    {
        if (enemy.handCards.Count == 0) return null;

        // 简单策略：优先出攻击牌
        foreach (var card in enemy.handCards)
        {
            if (card.cardType == CardType.Attack)
            {
                return card;
            }
        }
        // 没有攻击牌，就出第一张
        return enemy.handCards[0];
    }

    // AI决策：决定目标
    private CharacterData DecideTarget(CharacterData enemy, BattleData battleData)
    {
        // 简单策略：攻击血量百分比最低的玩家
        return GetWeakestPlayer(battleData);
    }

    // 模拟玩家玩一张牌的完整流程
    private IEnumerator PlayCardSequence(CharacterData enemy, CardData card, CharacterData target, BattleData battleData)
    {
        if (!battleData.characterSlots.TryGetValue(enemy, out var slots)) yield break;

        // 流程1: 清空旧卡槽
        slots.ClearSlots();

        // 流程2: 放置卡牌
        Debug.Log($"AI: {enemy.characterName} 正在放置卡牌 {card.cardName}...");
        slots.slots[0] = card; // 简单起见，总是放在第一个槽位
        enemy.handCards.Remove(card);
        // 此处可以触发卡牌放置的UI动画
        yield return new WaitForSeconds(1.5f);

        // 流程3: 选择目标
        Debug.Log($"AI: {enemy.characterName} 正在选择目标 {target.characterName}...");
        slots.target = target;
        // 此处可以触发目标高亮的UI动画
        yield return new WaitForSeconds(1.5f);

        // 流程4: 执行卡牌效果
        Debug.Log($"AI: {enemy.characterName} 对 {target.characterName} 发动 {card.cardName} 的效果!");
        if (CardEffectExecutor.Instance != null)
        {
            yield return CardEffectExecutor.Instance.ExecuteCardEffects(card, enemy, target, battleData);
        }
        yield return new WaitForSeconds(2.0f); // 等待卡牌效果动画和结算

        // 流程5: 清理
        slots.ClearSlots();
        Debug.Log($"AI: {enemy.characterName} 的行动完成。");
    }

    // 获取血量百分比最低的玩家
    private CharacterData GetWeakestPlayer(BattleData battleData)
    {
        CharacterData weakest = null;
        float lowestHealthPercent = float.MaxValue;

        foreach (var player in battleData.playerTeam)
        {
            if (player.isDead) continue;

            float healthPercent = (float)player.currentHealth / player.maxHealth;
            if (healthPercent < lowestHealthPercent)
            {
                lowestHealthPercent = healthPercent;
                weakest = player;
            }
        }
        return weakest;
    }
}