using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public List<CharacterData> allCharacters = new List<CharacterData>();
    public List<CardData> universalCards = new List<CardData>();
    public List<CardData> beelzebubCards = new List<CardData>();
    public List<CardData> mammonCards = new List<CardData>();
    public List<CardData> asmodeusCards = new List<CardData>();

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!isInitialized)
            {
                InitializeAllData();
                isInitialized = true;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAllData()
    {
        Debug.Log("��ʼ����Ϸ����...");
        ClearAllData();
        CreateUniversalCards();
        CreateCharacterCards();
        CreateCharacters();
        Debug.Log($"��ʼ�����: {allCharacters.Count}����ɫ, {universalCards.Count}��ͨ�ÿ���");
    }

    void ClearAllData()
    {
        allCharacters.Clear();
        universalCards.Clear();
        beelzebubCards.Clear();
        mammonCards.Clear();
        asmodeusCards.Clear();
    }

    void CreateUniversalCards()
    {
        universalCards.Add(new CardData("U001", "���", 0, "��ѡ���з����2���˺�", CardType.Attack, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Attack) });

        universalCards.Add(new CardData("U002", "����", 0, "ʹ�������3�㻤��", CardType.Defense, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Defense) });

        universalCards.Add(new CardData("U003", "������", 0, "����Ϊ��������ʱ��һ����", CardType.Special, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Special) });

        universalCards.Add(new CardData("U004", "����", 1, "��һ�ι�����ֱ���˺�X2", CardType.Buff, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Buff) });

        universalCards.Add(new CardData("U005", "���ط���", 2, "ʹ�������10�㻤��", CardType.Defense, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Defense) });
        universalCards.Add(new CardData("U006", "牺牲", 1, "弃掉一张攻击牌，对所有敌人造成5点伤害", CardType.Attack, CharacterClass.Universal)
        {
            cardColor = GetCardTypeColor(CardType.Attack),
            consumptionRequirements = new List<CardConsumptionRequirement>
            {
                new CardConsumptionRequirement
                {
                    consumptionType = CardConsumptionType.Discard,
                    requiredCount = 1,
                    requiredCardType = CardType.Attack
                }
            }
        });
    }

    void CreateCharacterCards()
    {
        // ����������
        beelzebubCards.Add(new CardData("B001", "������", 2, "��ѡ���з����2���˺�����", CardType.Attack, CharacterClass.Beelzebub)
        { cardColor = new Color(0.7f, 0.3f, 0.8f) });
        beelzebubCards.Add(new CardData("B002", "����", 1, "��ѡ���з����3���˺���������һ�㡾���á�", CardType.Status, CharacterClass.Beelzebub)
        { cardColor = new Color(0.6f, 0.2f, 0.7f) });
        beelzebubCards.Add(new CardData("B003", "�ܽ�", 1, "��ѡ���з��������㡾���á�", CardType.Status, CharacterClass.Beelzebub)
        { cardColor = new Color(0.5f, 0.1f, 0.6f) });
        beelzebubCards.Add(new CardData("B004", "��������", 2, "��ȫ��з����6���˺������������㡾���á�", CardType.Status, CharacterClass.Beelzebub)
        { cardColor = new Color(0.8f, 0.4f, 0.9f) });
        beelzebubCards.Add(new CardData("B005", "���ܱ���", 2, "����һ��ȫ��з��ġ����á�", CardType.Special, CharacterClass.Beelzebub)
        { cardColor = new Color(0.9f, 0.5f, 1.0f) });

        // ���ſ���
        mammonCards.Add(new CardData("M001", "ϸˮ����", 0, "�ظ�����2������", CardType.Heal, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 0.9f, 0.3f) });
        mammonCards.Add(new CardData("M002", "���", 0, "�´λظ���������ʱX2", CardType.Buff, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 0.8f, 0.2f) });
        mammonCards.Add(new CardData("M003", "��������", 1, "���������3���˺����ظ�����ֵ��͵��ѷ�3������", CardType.Heal, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 0.7f, 0.1f) });
        mammonCards.Add(new CardData("M004", "��Ѫ��", 1, "��ѡ���з����3���˺����ظ�����3������", CardType.Attack, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 1.0f, 0.4f) });
        mammonCards.Add(new CardData("M005", "��Դ����", 2, "�ظ�ȫ���ѷ�5������", CardType.Heal, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 1.0f, 0.5f) });

        // ��˹�ɵ�˹����
        asmodeusCards.Add(new CardData("A001", "��׼��", 0, "��ָ���з�����һ�㡾˥����", CardType.Status, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.6f, 0.8f) });
        asmodeusCards.Add(new CardData("A002", "��������", 1, "ʹ����ѷ����һ�㡾ǿ׳��", CardType.Buff, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.5f, 0.7f) });
        asmodeusCards.Add(new CardData("A003", "Ԯ��", 1, "ʹ����ѷ����5�㻤��", CardType.Defense, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.4f, 0.6f) });
        asmodeusCards.Add(new CardData("A004", "������", 2, "��ȫ��з�������4���˺�3��", CardType.Attack, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.7f, 0.9f) });
        asmodeusCards.Add(new CardData("A005", "����", 2, "��ȫ��з�����һ�㡾˥����", CardType.Status, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.8f, 1.0f) });
    }

    void CreateCharacters()
    {
        Debug.Log("������ɫ...");

        // ������ - ��ɫ
        CharacterData beelzebub = new CharacterData("C001", "������",
            "Ӭ��������Ϊ���˹��ϳ����˺�״̬��һ��������ɣ����Խ��Ἣ��ǿ��",
            CharacterClass.Beelzebub, new Color(0.5f, 0.2f, 0.5f));

        // Ϊ��ɫ�������п���
        beelzebub.uniqueDeck.AddRange(beelzebubCards);

        // ��ʼ����ɫ
        beelzebub.Initialize();
        allCharacters.Add(beelzebub);

        // ���� - ��ɫ
        CharacterData mammon = new CharacterData("C002", "����",
            "̰�������������Խ�ɫ�������������Ϊƶ������ר��������",
            CharacterClass.Mammon, new Color(0.9f, 0.7f, 0.1f));

        mammon.uniqueDeck.AddRange(mammonCards);
        mammon.Initialize();
        allCharacters.Add(mammon);

        // ��˹�ɵ�˹ - ��ɫ
        CharacterData asmodeus = new CharacterData("C003", "��˹�ɵ�˹",
            "����ħ�ó��ں󷽽��и�����Ϊ�Ѿ��ṩ���沢�����з�����ҪʱҲ�ܸ����������",
            CharacterClass.Asmodeus, new Color(0.8f, 0.2f, 0.3f));

        asmodeus.uniqueDeck.AddRange(asmodeusCards);
        asmodeus.Initialize();
        allCharacters.Add(asmodeus);
    }

    Color GetCardTypeColor(CardType type)
    {
        switch (type)
        {
            case CardType.Attack: return new Color(1f, 0.4f, 0.4f);
            case CardType.Defense: return new Color(0.4f, 0.6f, 1f);
            case CardType.Heal: return new Color(0.4f, 1f, 0.4f);
            case CardType.Status: return new Color(1f, 0.7f, 0.4f);
            case CardType.Buff: return new Color(1f, 1f, 0.4f);
            case CardType.Special: return new Color(0.7f, 0.4f, 1f);
            default: return Color.gray;
        }
    }

    public List<CardData> GetUniversalCards() => universalCards;
    public List<CardData> GetCardsByCharacterClass(CharacterClass characterClass)
    {
        switch (characterClass)
        {
            case CharacterClass.Beelzebub: return beelzebubCards;
            case CharacterClass.Mammon: return mammonCards;
            case CharacterClass.Asmodeus: return asmodeusCards;
            default: return universalCards;
        }
    }
    public List<CharacterData> GetAllCharacters() => allCharacters;
}