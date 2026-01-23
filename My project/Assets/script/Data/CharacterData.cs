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
    public void AddStatus(StatusEffect status)
    {
        statusEffects.Add(status);
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





