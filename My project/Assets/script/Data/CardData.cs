using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public string cardId;
    public string cardName;
    public int cost;
    public string description;
    public CardType cardType;
    public CharacterClass characterClass;
    public Color cardColor = Color.white;
    public List<CardEffect> effects = new List<CardEffect>();
    public bool requiresTarget; // 是否需要目标
    public List<CardConsumptionRequirement> consumptionRequirements = new List<CardConsumptionRequirement>(); // 卡牌消耗需求

    public CardData(string id, string name, int cost, string desc, CardType type, CharacterClass charClass)
    {
        this.cardId = id;
        this.cardName = name;
        this.cost = cost;
        this.description = desc;
        this.cardType = type;
        this.characterClass = charClass;
        UpdateRequiresTarget();
    }

    // 根据效果判断是否需要目标
    public void UpdateRequiresTarget()
    {
        requiresTarget = false;
        foreach (var effect in effects)
        {
            if (effect.target != TargetType.Self && effect.target != TargetType.None)
            {
                requiresTarget = true;
                break;
            }
        }
    }
}

[System.Serializable]
public class CardConsumptionRequirement
{
    public CardConsumptionType consumptionType; // 消耗类型，例如丢弃或消耗
    public int requiredCount; // 需要消耗的卡牌数量
    public CardType requiredCardType; // 需要消耗的卡牌类型
}

[System.Serializable]
public class CardEffect
{
    public string effectName;
    public int value;
    public TargetType target;  // TargetType 应该可以配置
    public StatusType statusType;
    public int statusStacks;
    public bool isTemporary;
}

public enum CardConsumptionType
{
    Discard, // 丢弃
    Exhaust // 消耗
}