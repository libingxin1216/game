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
        Debug.Log("初始化游戏数据...");
        ClearAllData();
        CreateUniversalCards();
        CreateCharacterCards();
        CreateCharacters();
        Debug.Log($"初始化完成: {allCharacters.Count}个角色, {universalCards.Count}张通用卡牌");
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
        universalCards.Add(new CardData("U001", "打击", 0, "对选定敌方造成2点伤害", CardType.Attack, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Attack) });

        universalCards.Add(new CardData("U002", "防御", 0, "使自身获得3点护盾", CardType.Defense, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Defense) });

        universalCards.Add(new CardData("U003", "储备粮", 0, "被作为费用消耗时抽一张牌", CardType.Special, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Special) });

        universalCards.Add(new CardData("U004", "蓄力", 1, "下一次攻击的直接伤害X2", CardType.Buff, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Buff) });

        universalCards.Add(new CardData("U005", "着重防御", 2, "使自身获得10点护盾", CardType.Defense, CharacterClass.Universal)
        { cardColor = GetCardTypeColor(CardType.Defense) });
    }

    void CreateCharacterCards()
    {
        // 别西卜卡组
        beelzebubCards.Add(new CardData("B001", "气传播", 0, "对选定敌方造成2点伤害两次", CardType.Attack, CharacterClass.Beelzebub)
        { cardColor = new Color(0.7f, 0.3f, 0.8f) });
        beelzebubCards.Add(new CardData("B002", "溃烂", 1, "对选定敌方造成3点伤害，并给予一层【腐烂】", CardType.Status, CharacterClass.Beelzebub)
        { cardColor = new Color(0.6f, 0.2f, 0.7f) });
        beelzebubCards.Add(new CardData("B003", "溶解", 1, "对选定敌方给予两层【腐烂】", CardType.Status, CharacterClass.Beelzebub)
        { cardColor = new Color(0.5f, 0.1f, 0.6f) });
        beelzebubCards.Add(new CardData("B004", "腐败蔓延", 2, "对全体敌方造成6点伤害，并给予两层【腐烂】", CardType.Status, CharacterClass.Beelzebub)
        { cardColor = new Color(0.8f, 0.4f, 0.9f) });
        beelzebubCards.Add(new CardData("B005", "腐败爆发", 2, "结算一次全体敌方的【腐烂】", CardType.Special, CharacterClass.Beelzebub)
        { cardColor = new Color(0.9f, 0.5f, 1.0f) });

        // 玛门卡组
        mammonCards.Add(new CardData("M001", "细水长流", 0, "回复自身2点生命", CardType.Heal, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 0.9f, 0.3f) });
        mammonCards.Add(new CardData("M002", "算计", 0, "下次回复自身生命时X2", CardType.Buff, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 0.8f, 0.2f) });
        mammonCards.Add(new CardData("M003", "生命交易", 1, "对自身造成3点伤害，回复生命值最低的友方3点生命", CardType.Heal, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 0.7f, 0.1f) });
        mammonCards.Add(new CardData("M004", "吸血鬼", 1, "对选定敌方造成3点伤害，回复自身3点生命", CardType.Attack, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 1.0f, 0.4f) });
        mammonCards.Add(new CardData("M005", "财源滚滚", 2, "回复全体友方5点生命", CardType.Heal, CharacterClass.Mammon)
        { cardColor = new Color(1.0f, 1.0f, 0.5f) });

        // 阿斯蒙蒂斯卡组
        asmodeusCards.Add(new CardData("A001", "瞄准好", 0, "对指定敌方给予一层【衰弱】", CardType.Status, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.6f, 0.8f) });
        asmodeusCards.Add(new CardData("A002", "吐气如兰", 1, "使随机友方获得一层【强壮】", CardType.Buff, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.5f, 0.7f) });
        asmodeusCards.Add(new CardData("A003", "援护", 1, "使随机友方获得5点护盾", CardType.Defense, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.4f, 0.6f) });
        asmodeusCards.Add(new CardData("A004", "无情抽打", 2, "对全体敌方随机造成4点伤害3次", CardType.Attack, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.7f, 0.9f) });
        asmodeusCards.Add(new CardData("A005", "贬损", 2, "对全体敌方给予一层【衰弱】", CardType.Status, CharacterClass.Asmodeus)
        { cardColor = new Color(1.0f, 0.8f, 1.0f) });
    }

    void CreateCharacters()
    {
        Debug.Log("创建角色...");

        // 别西卜 - 紫色
        CharacterData beelzebub = new CharacterData("C001", "别西卜",
            "蝇王着重于为敌人挂上持续伤害状态，一旦启动完成，毒性将会极其强烈",
            CharacterClass.Beelzebub, new Color(0.5f, 0.2f, 0.5f));

        // 为角色添加特有卡牌
        beelzebub.uniqueDeck.AddRange(beelzebubCards);

        // 初始化角色
        beelzebub.Initialize();
        allCharacters.Add(beelzebub);

        // 玛门 - 金色
        CharacterData mammon = new CharacterData("C002", "玛门",
            "贪婪环主是续航性角色，在其他方面较为贫弱，但专精于治疗",
            CharacterClass.Mammon, new Color(0.9f, 0.7f, 0.1f));

        mammon.uniqueDeck.AddRange(mammonCards);
        mammon.Initialize();
        allCharacters.Add(mammon);

        // 阿斯蒙蒂斯 - 粉色
        CharacterData asmodeus = new CharacterData("C003", "阿斯蒙蒂斯",
            "大魅魔擅长在后方进行辅助，为友军提供增益并削弱敌方，必要时也能给出部分输出",
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