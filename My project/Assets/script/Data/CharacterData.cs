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

    // �ƿ����
    public List<CardData> uniqueDeck = new List<CardData>();
    public List<CardData> addedCards = new List<CardData>(); // ���ӵ�ͨ����
    public List<CardData> battleDeck = new List<CardData>(); // ս���ƿ⣨����+ͨ�ã�
    public List<CardData> handCards = new List<CardData>();  // ���� - ���������
    public List<CardData> discardPile = new List<CardData>(); // ���ƶ�

    // ״̬Ч��
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public int shield = 0;

    // ��ʱЧ���ֵ�
    public Dictionary<string, object> temporaryEffects = new Dictionary<string, object>();

    // ���캯��
    public CharacterData(string id, string name, string desc, CharacterClass charClass, Color color)
    {
        characterId = id;
        characterName = name;
        description = desc;
        characterClass = charClass;
        characterColor = color;

        // ��ʼ������ֵ
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

    // ������ʱЧ��
    public void AddTemporaryEffect(string effectName, object value = null)
    {
        temporaryEffects[effectName] = value ?? true;
    }

    // ����Ƿ�����ʱЧ��
    public bool HasTemporaryEffect(string effectName)
    {
        return temporaryEffects.ContainsKey(effectName);
    }

    // �Ƴ���ʱЧ��
    public void RemoveTemporaryEffect(string effectName)
    {
        temporaryEffects.Remove(effectName);
    }

    // ����״̬Ч��
    public void TriggerStatusEffect(StatusType statusType)
    {
        StatusEffect status = statusEffects.Find(s => s.type == statusType);
        if (status != null && status.stacks > 0)
        {
            // ��������Ч��
            if (statusType == StatusType.Rot)
            {
                // ���ã��غϿ�ʼǰ����˺������ӻ���
                TakeDamage(status.stacks, true);
                Debug.Log($"{characterName} ���� {status.stacks} �㸯�ã��ܵ� {status.stacks} ���˺�");
            }
        }
    }

    // �����غϿ�ʼʱ��״̬Ч��
    public void ProcessTurnStartStatus()
    {
        // ��������Ч��
        TriggerStatusEffect(StatusType.Rot);

        // ����״̬����ʱ��
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect status = statusEffects[i];

            if (status.duration > 0)
            {
                status.duration--;
                if (status.duration == 0)
                {
                    // ����ʱ��������Ƴ�״̬
                    statusEffects.RemoveAt(i);
                    Debug.Log($"{characterName} �� {GetStatusName(status.type)} ״̬����");
                }
            }
        }

        // �����ʱЧ��
        temporaryEffects.Clear();
    }

    // �����غϽ���ʱ��״̬Ч��
    public void ProcessTurnEndStatus()
    {
        // ÿ�غ���ջ���
        shield = 0;

        Debug.Log($"{characterName} �غϽ������������");
    }

    string GetStatusName(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return "����";
            case StatusType.Strong: return "ǿ׳";
            case StatusType.Weak: return "˥��";
            case StatusType.Shield: return "����";
            default: return "δ֪";
        }
    }

    // ��ʼ������
    public void Initialize()
    {
        currentHealth = maxHealth;
        isDead = false;
        shield = 0;
        handCards.Clear();
        statusEffects.Clear();

        // �ϲ��ƿ�
        battleDeck.Clear();
        battleDeck.AddRange(uniqueDeck);
        battleDeck.AddRange(addedCards);
    }

    // ���Ʒ���
    public void DrawCard(int count = 1)
    {
        for (int i = 0; i < count && battleDeck.Count > 0; i++)
        {
            // ��ʵ�֣����ƿ��һ�ų�
            if (battleDeck.Count > 0)
            {
                CardData card = battleDeck[0];
                handCards.Add(card);
                battleDeck.RemoveAt(0);
            }
        }
    }

    // ���Ʒ���
    public void DiscardCard(CardData card)
    {
        if (handCards.Contains(card))
        {
            handCards.Remove(card);
            discardPile.Add(card);
        }
    }

    // �ܵ��˺�
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
            BattleManager.Instance.CheckBattleEnd();
        }
    }

    // ����
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // ���ӻ���
    public void AddShield(int amount)
    {
        shield += amount;
    }

    // ����״̬
    // �� CharacterData.cs ���������������
    public void AddStatus(StatusType statusType, int stacks = 1, int duration = -1)
    {
        StatusEffect existing = statusEffects.Find(s => s.type == statusType);
        if (existing != null)
        {
            // ���Ӳ���
            existing.stacks += stacks;
            if (duration > 0 && existing.duration > 0)
            {
                existing.duration = Mathf.Max(existing.duration, duration);
            }
        }
        else
        {
            // ������״̬
            StatusEffect newStatus = new StatusEffect(statusType, stacks, duration);
            statusEffects.Add(newStatus);
        }
    }

    // ��ȡ�����ӳ�/����
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