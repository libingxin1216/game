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

    public CardData(string id, string name, int cost, string desc, CardType type, CharacterClass charClass)
    {
        this.cardId = id;
        this.cardName = name;
        this.cost = cost;
        this.description = desc;
        this.cardType = type;
        this.characterClass = charClass;
    }
}

[System.Serializable]
public class CardEffect
{
    public string effectName;
    public int value;
    public TargetType target;  // TargetType 应该可以访问
    public StatusType statusType;
    public int statusStacks;
    public bool isTemporary;
}