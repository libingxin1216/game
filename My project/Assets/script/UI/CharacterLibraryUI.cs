using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLibraryUI : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Button backButton;
    [SerializeField] private Text titleText;

    [Header("角色区域")]
    [SerializeField] private Transform characterListContainer;
    [SerializeField] private GameObject characterButtonPrefab;

    [Header("卡牌区域")]
    [SerializeField] private Transform universalCardContainer;
    [SerializeField] private Transform roleCardContainer;
    [SerializeField] private GameObject cardButtonPrefab;

    [Header("卡牌详情面板")]
    [SerializeField] private GameObject cardDetailPanel;
    [SerializeField] private Text cardDetailName;
    [SerializeField] private Text cardDetailCost;
    [SerializeField] private Text cardDetailDescription;
    [SerializeField] private Button closeDetailButton;

    [Header("标签页")]
    [SerializeField] private Button universalTabButton;
    [SerializeField] private Button roleTabButton;
    [SerializeField] private GameObject universalTabContent;
    [SerializeField] private GameObject roleTabContent;

    private CharacterData currentSelectedCharacter;

    void Start()
    {
        Debug.Log("CharacterLibraryUI Start");

        // 延迟初始化，确保其他组件已加载
        StartCoroutine(DelayedInitialize());
    }

    IEnumerator DelayedInitialize()
    {
        // 等待一帧，确保所有组件加载完成
        yield return null;

        // 初始化UI
        InitializeUI();

        // 绑定按钮事件
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToMainMenu);
        else
            Debug.LogError("backButton为null!");

        if (closeDetailButton != null)
            closeDetailButton.onClick.AddListener(HideCardDetail);

        if (universalTabButton != null)
            universalTabButton.onClick.AddListener(ShowUniversalCards);

        if (roleTabButton != null)
            roleTabButton.onClick.AddListener(ShowRoleCards);

        // 默认显示通用卡牌
        ShowUniversalCards();

        Debug.Log("CharacterLibraryUI初始化完成");
    }

    void InitializeUI()
    {
        Debug.Log("开始初始化UI...");

        // 清空容器
        ClearContainer(characterListContainer);
        ClearContainer(universalCardContainer);
        ClearContainer(roleCardContainer);

        // 加载所有角色
        LoadAllCharacters();

        // 加载通用卡牌
        LoadUniversalCards();

        Debug.Log("UI初始化完成");
    }

    void ClearContainer(Transform container)
    {
        if (container == null)
        {
            Debug.LogError("尝试清空的容器为null!");
            return;
        }

        int childCount = container.childCount;
        Debug.Log($"清空容器: {container.name}, 子对象数: {childCount}");

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    void LoadAllCharacters()
    {
        Debug.Log("开始加载角色...");

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager未初始化！");
            return;
        }

        List<CharacterData> characters = GameDataManager.Instance.GetAllCharacters();
        Debug.Log($"获取到角色数量: {characters?.Count ?? 0}");

        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning("没有找到角色数据！");

            // 测试：手动创建一些测试数据
            CreateTestCharacters();
            return;
        }

        if (characterListContainer == null)
        {
            Debug.LogError("characterListContainer为空！");
            return;
        }

        if (characterButtonPrefab == null)
        {
            Debug.LogError("characterButtonPrefab为空！");
            return;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            if (character == null)
            {
                Debug.LogError($"第{i}个角色数据为null!");
                continue;
            }

            Debug.Log($"创建角色按钮: {character.characterName}");

            // 创建角色按钮
            GameObject characterButtonObj = Instantiate(characterButtonPrefab, characterListContainer);
            if (characterButtonObj == null)
            {
                Debug.LogError("实例化角色按钮失败!");
                continue;
            }

            CharacterButtonUI characterButton = characterButtonObj.GetComponent<CharacterButtonUI>();
            if (characterButton == null)
            {
                Debug.LogError($"角色按钮没有CharacterButtonUI脚本!");
                continue;
            }

            characterButton.Initialize(character, OnCharacterSelected);

            // 默认选择第一个角色
            if (i == 0 && currentSelectedCharacter == null)
            {
                currentSelectedCharacter = character;
                characterButton.SetSelected(true);
                LoadRoleCards(character);
            }
        }
    }

    void CreateTestCharacters()
    {
        Debug.Log("创建测试角色数据...");

        // 创建测试角色
        CharacterData testChar1 = new CharacterData("TEST1", "测试角色1", "测试描述", CharacterClass.Beelzebub, Color.red);
        CharacterData testChar2 = new CharacterData("TEST2", "测试角色2", "测试描述", CharacterClass.Mammon, Color.yellow);
        CharacterData testChar3 = new CharacterData("TEST3", "测试角色3", "测试描述", CharacterClass.Asmodeus, Color.blue);

        List<CharacterData> testChars = new List<CharacterData> { testChar1, testChar2, testChar3 };

        if (characterListContainer == null || characterButtonPrefab == null)
        {
            Debug.LogError("无法创建测试角色：容器或预制体为空");
            return;
        }

        for (int i = 0; i < testChars.Count; i++)
        {
            CharacterData character = testChars[i];
            GameObject characterButtonObj = Instantiate(characterButtonPrefab, characterListContainer);
            CharacterButtonUI characterButton = characterButtonObj.GetComponent<CharacterButtonUI>();

            if (characterButton != null)
            {
                characterButton.Initialize(character, OnCharacterSelected);

                if (i == 0)
                {
                    currentSelectedCharacter = character;
                    characterButton.SetSelected(true);
                }
            }
        }

        Debug.Log("测试角色创建完成");
    }

    void LoadUniversalCards()
    {
        Debug.Log("开始加载通用卡牌...");

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager为空");
            return;
        }

        List<CardData> universalCards = GameDataManager.Instance.GetUniversalCards();
        Debug.Log($"获取到通用卡牌数量: {universalCards?.Count ?? 0}");

        if (universalCards == null || universalCards.Count == 0)
        {
            Debug.LogWarning("没有找到通用卡牌！");
            return;
        }

        if (universalCardContainer == null)
        {
            Debug.LogError("universalCardContainer为空！");
            return;
        }

        if (cardButtonPrefab == null)
        {
            Debug.LogError("cardButtonPrefab为空！");
            return;
        }

        foreach (CardData card in universalCards)
        {
            if (card == null)
            {
                Debug.LogError("卡牌数据为null!");
                continue;
            }

            Debug.Log($"创建通用卡牌按钮: {card.cardName}");

            GameObject cardButtonObj = Instantiate(cardButtonPrefab, universalCardContainer);
            if (cardButtonObj == null)
            {
                Debug.LogError("实例化卡牌按钮失败!");
                continue;
            }

            CardButtonUI cardButton = cardButtonObj.GetComponent<CardButtonUI>();
            if (cardButton != null)
            {
                cardButton.Initialize(card, OnCardClicked);
            }
            else
            {
                Debug.LogError($"卡牌按钮没有CardButtonUI脚本!");
            }
        }
    }

    void LoadRoleCards(CharacterData character)
    {
        Debug.Log($"开始加载角色卡牌: {character?.characterName}");

        if (roleCardContainer == null)
        {
            Debug.LogError("roleCardContainer为空！");
            return;
        }

        // 清空角色卡牌容器
        ClearContainer(roleCardContainer);

        if (character == null || character.uniqueDeck == null)
        {
            Debug.LogWarning("角色或角色卡牌列表为空");
            return;
        }

        Debug.Log($"角色 {character.characterName} 有 {character.uniqueDeck.Count} 张卡牌");

        if (cardButtonPrefab == null)
        {
            Debug.LogError("cardButtonPrefab为空！");
            return;
        }

        foreach (CardData card in character.uniqueDeck)
        {
            if (card == null)
            {
                Debug.LogWarning("角色卡牌数据为null!");
                continue;
            }

            Debug.Log($"创建角色卡牌按钮: {card.cardName}");

            GameObject cardButtonObj = Instantiate(cardButtonPrefab, roleCardContainer);
            CardButtonUI cardButton = cardButtonObj.GetComponent<CardButtonUI>();

            if (cardButton != null)
            {
                cardButton.Initialize(card, OnCardClicked);
            }
            else
            {
                Debug.LogError($"角色卡牌按钮没有CardButtonUI脚本!");
            }
        }
    }

    void OnCharacterSelected(CharacterData character)
    {
        Debug.Log($"选择角色: {character?.characterName}");

        if (character == null) return;

        // 更新当前选中的角色
        currentSelectedCharacter = character;

        // 更新角色按钮的选中状态
        if (characterListContainer != null)
        {
            foreach (Transform child in characterListContainer)
            {
                CharacterButtonUI button = child.GetComponent<CharacterButtonUI>();
                if (button != null)
                {
                    button.SetSelected(button.GetCharacter()?.characterId == character.characterId);
                }
            }
        }

        // 加载该角色的卡牌
        LoadRoleCards(character);

        // 如果当前在角色卡牌标签页，更新标题
        if (roleTabContent != null && roleTabContent.activeSelf && titleText != null)
        {
            titleText.text = $"{character.characterName} - 特有卡牌";
        }
    }

    void OnCardClicked(CardData card)
    {
        Debug.Log($"点击卡牌: {card?.cardName}");

        if (card == null) return;

        ShowCardDetail(card);
    }

    void ShowUniversalCards()
    {
        Debug.Log("显示通用卡牌");

        if (universalTabContent != null) universalTabContent.SetActive(true);
        if (roleTabContent != null) roleTabContent.SetActive(false);

        // 更新按钮状态
        if (universalTabButton != null) universalTabButton.interactable = false;
        if (roleTabButton != null) roleTabButton.interactable = true;

        if (titleText != null) titleText.text = "通用卡牌库";
    }

    void ShowRoleCards()
    {
        Debug.Log("显示角色卡牌");

        if (universalTabContent != null) universalTabContent.SetActive(false);
        if (roleTabContent != null) roleTabContent.SetActive(true);

        // 更新按钮状态
        if (universalTabButton != null) universalTabButton.interactable = true;
        if (roleTabButton != null) roleTabButton.interactable = false;

        if (currentSelectedCharacter != null && titleText != null)
        {
            titleText.text = $"{currentSelectedCharacter.characterName} - 特有卡牌";
        }
        else if (titleText != null)
        {
            titleText.text = "角色卡牌库";
        }
    }

    void ShowCardDetail(CardData card)
    {
        Debug.Log($"显示卡牌详情: {card.cardName}");

        if (card == null || cardDetailPanel == null) return;

        // 更新UI信息
        if (cardDetailName != null) cardDetailName.text = card.cardName;
        if (cardDetailCost != null) cardDetailCost.text = $"消耗: {card.cost}张卡牌";
        if (cardDetailDescription != null) cardDetailDescription.text = card.description;

        // 显示面板
        cardDetailPanel.SetActive(true);
    }

    void HideCardDetail()
    {
        Debug.Log("隐藏卡牌详情");

        if (cardDetailPanel != null)
            cardDetailPanel.SetActive(false);
    }

    void ReturnToMainMenu()
    {
        Debug.Log("返回主菜单");

        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
    }
}