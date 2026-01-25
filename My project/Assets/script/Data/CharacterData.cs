using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterId;
    public string characterName;
    public string description;
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    public CharacterClass characterClass;
    public Color characterColor = Color.white;

    // 牌库相关
    public List<CardData> uniqueDeck = new List<CardData>();
    public List<CardData> addedCards = new List<CardData>(); // 添加的通用牌
    public List<CardData> battleDeck = new List<CardData>(); // 战斗牌库（特有+通用）
    public List<CardData> handCards = new List<CardData>();  // 手牌 - 这个必须有
    public List<CardData> discardPile = new List<CardData>(); // 弃牌堆

    // 状态效果
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public int shield = 0;

    // 临时效果字典
    public Dictionary<string, object> temporaryEffects = new Dictionary<string, object>();

    // 构造函数
    public CharacterData(string id, string name, string desc, CharacterClass charClass, Color color)
    {
        characterId = id;
        characterName = name;
        description = desc;
        characterClass = charClass;
        characterColor = color;

        // 初始化生命值
        currentHealth = maxHealth;
        isDead = false;
        shield = 0;

        handCards = new List<CardData>();
        uniqueDeck = new List<CardData>();
        addedCards = new List<CardData>();
        battleDeck = new List<CardData>();
        discardPile = new List<CardData>();
        statusEffects = new List<StatusEffect>();
        temporaryEffects = new Dictionary<string, object>();
    }

    // 添加临时效果
    public void AddTemporaryEffect(string effectName, object value = null)
    {
        temporaryEffects[effectName] = value ?? true;
    }

    // 检查是否有临时效果
    public bool HasTemporaryEffect(string effectName)
    {
        return temporaryEffects.ContainsKey(effectName);
    }

    // 移除临时效果
    public void RemoveTemporaryEffect(string effectName)
    {
        temporaryEffects.Remove(effectName);
    }

    // 触发状态效果
    public void TriggerStatusEffect(StatusType statusType)
    {
        StatusEffect status = statusEffects.Find(s => s.type == statusType);
        if (status != null && status.stacks > 0)
        {
            // 触发腐烂效果
            if (statusType == StatusType.Rot)
            {
                // 腐烂：回合开始前造成伤害，无视护盾
                TakeDamage(status.stacks, true);
                Debug.Log($"{characterName} 触发 {status.stacks} 层腐烂，受到 {status.stacks} 点伤害");
            }
        }
    }

    // 处理回合开始时的状态效果
    public void ProcessTurnStartStatus()
    {
        // 触发腐烂效果
        TriggerStatusEffect(StatusType.Rot);

        // 减少状态持续时间
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect status = statusEffects[i];

            if (status.duration > 0)
            {
                status.duration--;
                if (status.duration == 0)
                {
                    // 持续时间结束，移除状态
                    statusEffects.RemoveAt(i);
                    Debug.Log($"{characterName} 的 {GetStatusName(status.type)} 状态结束");
                }
            }
        }

        // 清空临时效果
        temporaryEffects.Clear();
    }

    // 处理回合结束时的状态效果
    public void ProcessTurnEndStatus()
    {
        // 每回合清空护盾
        shield = 0;

        Debug.Log($"{characterName} 回合结束，护盾清空");
    }

    string GetStatusName(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return "腐烂";
            case StatusType.Strong: return "强壮";
            case StatusType.Weak: return "衰弱";
            case StatusType.Shield: return "护盾";
            default: return "未知";
        }
    }

    // 初始化方法
    public void Initialize()
    {
        currentHealth = maxHealth;
        isDead = false;
        shield = 0;
        handCards.Clear();
        statusEffects.Clear();

        // 合并牌库
        battleDeck.Clear();
        battleDeck.AddRange(uniqueDeck);
        battleDeck.AddRange(addedCards);
    }

    // 抽牌方法
    public void DrawCard(int count = 1)
    {
        for (int i = 0; i < count && battleDeck.Count > 0; i++)
        {
            // 简单实现：从牌库第一张抽
            if (battleDeck.Count > 0)
            {
                CardData card = battleDeck[0];
                handCards.Add(card);
                battleDeck.RemoveAt(0);
            }
        }
    }

    // 弃牌方法
    public void DiscardCard(CardData card)
    {
        if (handCards.Contains(card))
        {
            handCards.Remove(card);
            discardPile.Add(card);
        }
    }

    // 受到伤害
    public void TakeDamage(int damage, bool ignoreShield = false)
    {
        if (isDead) return;

        if (!ignoreShield && shield > 0)
        {
            int remainingShield = shield - damage;
            if (remainingShield >= 0)
            {
                shield = remainingShield;
                damage = 0;
            }
            else
            {
                damage = -remainingShield;
                shield = 0;
            }
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
    }

    // 治疗
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // 添加护盾
    public void AddShield(int amount)
    {
        shield += amount;
    }

    // 添加状态
    // 在 CharacterData.cs 中添加这个方法：
    public void AddStatus(StatusType statusType, int stacks = 1, int duration = -1)
    {
        StatusEffect existing = statusEffects.Find(s => s.type == statusType);
        if (existing != null)
        {
            // 叠加层数
            existing.stacks += stacks;
            if (duration > 0 && existing.duration > 0)
            {
                existing.duration = Mathf.Max(existing.duration, duration);
            }
        }
        else
        {
            // 创建新状态
            StatusEffect newStatus = new StatusEffect(statusType, stacks, duration);
            statusEffects.Add(newStatus);
        }
    }

    // 获取攻击加成/减益
    public int GetDamageModifier()
    {
        int modifier = 0;

        foreach (StatusEffect status in statusEffects)
        {
            switch (status.type)
            {
                case StatusType.Strong:
                    modifier += status.stacks;
                    break;
                case StatusType.Weak:
                    modifier -= status.stacks;
                    break;
            }
        }

        return modifier;
    }
}