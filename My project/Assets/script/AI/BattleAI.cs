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

    // 执行敌人回合
    public IEnumerator ExecuteEnemyTurn(BattleData battleData)
    {
        Debug.Log("=== 敌人AI回合开始 ===");

        yield return new WaitForSeconds(1f);

        // 为每个敌人角色执行行动
        foreach (var enemy in battleData.enemyTeam)
        {
            if (enemy.isDead) continue;

            yield return ExecuteEnemyActions(enemy, battleData);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("=== 敌人AI回合结束 ===");
    }

    // 执行单个敌人的行动
    IEnumerator ExecuteEnemyActions(CharacterData enemy, BattleData battleData)
    {
        Debug.Log($"敌人AI: {enemy.characterName} 开始行动");

        // 1. 为敌人放置卡牌
        yield return PlaceEnemyCards(enemy, battleData);

        // 2. 选择目标
        yield return SelectEnemyTarget(enemy, battleData);

        // 3. 执行卡牌效果
        yield return ExecuteEnemyCardEffects(enemy, battleData);

        Debug.Log($"敌人AI: {enemy.characterName} 行动完成");
    }

    // 为敌人放置卡牌
    IEnumerator PlaceEnemyCards(CharacterData enemy, BattleData battleData)
    {
        if (!battleData.characterSlots.TryGetValue(enemy, out var slots)) yield break;

        // 清空之前的卡牌
        slots.ClearSlots();

        // 从手牌中选择卡牌放置（简单AI：随机选择）
        for (int i = 0; i < 3; i++)
        {
            if (enemy.handCards.Count > 0)
            {
                // 随机选择一张卡牌
                int randomIndex = Random.Range(0, enemy.handCards.Count);
                CardData selectedCard = enemy.handCards[randomIndex];

                // 放置到卡槽
                slots.slots[i] = selectedCard;

                // 从手牌移除
                enemy.handCards.RemoveAt(randomIndex);

                Debug.Log($"敌人AI: {enemy.characterName} 放置卡牌 {selectedCard.cardName} 到卡槽 {i}");

                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    // 选择敌人目标
    IEnumerator SelectEnemyTarget(CharacterData enemy, BattleData battleData)
    {
        if (!battleData.characterSlots.TryGetValue(enemy, out var slots)) yield break;

        // 选择目标（简单AI：随机选择存活玩家）
        List<CharacterData> alivePlayers = new List<CharacterData>();
        foreach (var player in battleData.playerTeam)
        {
            if (!player.isDead)
            {
                alivePlayers.Add(player);
            }
        }

        if (alivePlayers.Count > 0)
        {
            int randomIndex = Random.Range(0, alivePlayers.Count);
            slots.target = alivePlayers[randomIndex];

            Debug.Log($"敌人AI: {enemy.characterName} 选择目标 {slots.target.characterName}");
        }

        yield return new WaitForSeconds(0.3f);
    }

    // 执行敌人卡牌效果
    IEnumerator ExecuteEnemyCardEffects(CharacterData enemy, BattleData battleData)
    {
        if (!battleData.characterSlots.TryGetValue(enemy, out var slots)) yield break;
        if (slots.target == null) yield break;

        Debug.Log($"敌人AI: {enemy.characterName} 执行卡牌效果，目标: {slots.target.characterName}");

        // 执行每个卡槽的卡牌效果
        for (int i = 0; i < 3; i++)
        {
            if (slots.slots[i] != null)
            {
                Debug.Log($"敌人AI: 执行卡牌 {slots.slots[i].cardName}");

                // 使用效果执行器
                if (CardEffectExecutor.Instance != null)
                {
                    yield return CardEffectExecutor.Instance.ExecuteCardEffects(
                        slots.slots[i], enemy, slots.target, battleData
                    );
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // 智能AI版本（可选）
    IEnumerator ExecuteSmartEnemyActions(CharacterData enemy, BattleData battleData)
    {
        // 1. 分析局势
        CharacterData weakestPlayer = GetWeakestPlayer(battleData);
        CharacterData strongestPlayer = GetStrongestPlayer(battleData);

        // 2. 根据角色类型选择策略
        switch (enemy.characterClass)
        {
            case CharacterClass.Beelzebub:
                // 别西卜：优先给低血量目标上腐烂
                yield return ExecuteBeelzebubStrategy(enemy, weakestPlayer, battleData);
                break;

            case CharacterClass.Mammon:
                // 玛门：治疗自己，攻击敌人
                yield return ExecuteMammonStrategy(enemy, strongestPlayer, battleData);
                break;

            case CharacterClass.Asmodeus:
                // 阿斯蒙蒂斯：给敌人上衰弱，给友军加强壮
                yield return ExecuteAsmodeusStrategy(enemy, battleData);
                break;

            default:
                // 默认策略
                yield return ExecuteDefaultStrategy(enemy, battleData);
                break;
        }
    }

    CharacterData GetWeakestPlayer(BattleData battleData)
    {
        CharacterData weakest = null;
        float lowestHealthPercent = 1f;

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

    CharacterData GetStrongestPlayer(BattleData battleData)
    {
        CharacterData strongest = null;
        float highestDamagePotential = 0f;

        foreach (var player in battleData.playerTeam)
        {
            if (player.isDead) continue;

            // 简单评估：生命值越高越强
            float strength = player.currentHealth;
            if (strength > highestDamagePotential)
            {
                highestDamagePotential = strength;
                strongest = player;
            }
        }

        return strongest;
    }

    // 各个角色的策略
    IEnumerator ExecuteBeelzebubStrategy(CharacterData enemy, CharacterData target, BattleData battleData)
    {
        // 别西卜策略：给目标上腐烂，使用AOE技能
        Debug.Log($"别西卜AI: 优先攻击最弱目标 {target?.characterName}");

        // 这里可以添加具体的卡牌选择逻辑
        yield break;
    }

    IEnumerator ExecuteMammonStrategy(CharacterData enemy, CharacterData target, BattleData battleData)
    {
        // 玛门策略：先治疗自己，再攻击
        Debug.Log($"玛门AI: 治疗自己，攻击最强目标 {target?.characterName}");

        // 检查是否需要治疗
        float healthPercent = (float)enemy.currentHealth / enemy.maxHealth;
        if (healthPercent < 0.5f)
        {
            // 优先使用治疗卡牌
            Debug.Log("玛门AI: 生命值低，优先治疗");
        }

        yield break;
    }

    IEnumerator ExecuteAsmodeusStrategy(CharacterData enemy, BattleData battleData)
    {
        // 阿斯蒙蒂斯策略：给敌人上衰弱，给友军加强壮
        Debug.Log("阿斯蒙蒂斯AI: 使用增益/减益技能");
        yield break;
    }

    IEnumerator ExecuteDefaultStrategy(CharacterData enemy, BattleData battleData)
    {
        // 默认策略：随机选择
        Debug.Log("通用AI: 使用随机策略");
        yield break;
    }
}