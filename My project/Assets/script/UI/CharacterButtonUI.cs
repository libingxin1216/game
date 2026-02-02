using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterButtonUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Image background;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text characterClassText;
    [SerializeField] private Button button;
    [SerializeField] private Image selectionBorder;

    private CharacterData characterData;
    private Action<CharacterData> onClickCallback;

    public void Initialize(CharacterData data, Action<CharacterData> callback)
    {
        // 安全检查
        if (data == null)
        {
            Debug.LogError("CharacterButtonUI.Initialize: data为null!");
            return;
        }

        characterData = data;
        onClickCallback = callback;

        Debug.Log($"初始化角色按钮: {data.characterName}");

        // 更新UI - 添加null检查
        if (characterNameText != null)
        {
            characterNameText.text = data.characterName ?? "未命名";
        }
        else
        {
            Debug.LogError("characterNameText为null!");
        }

        if (characterClassText != null)
        {
            characterClassText.text = GetCharacterClassString(data.characterClass);
        }
        else
        {
            Debug.LogError("characterClassText为null!");
        }

        // 设置背景颜色
        if (background != null)
        {
            background.color = data.characterColor;
        }
        else
        {
            Debug.LogError("background为null!");
        }

        // 绑定按钮事件
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError("button为null!");
        }

        SetSelected(false);
    }

    string GetCharacterClassString(CharacterClass charClass)
    {
        switch (charClass)
        {
            case CharacterClass.Beelzebub: return "蝇王·持续伤害";
            case CharacterClass.Mammon: return "贪婪·治疗续航";
            case CharacterClass.Asmodeus: return "魅魔·辅助增益";
            default: return "通用";
        }
    }

    void OnButtonClicked()
    {
        Debug.Log($"点击角色: {characterData?.characterName}");
        onClickCallback?.Invoke(characterData);
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.gameObject.SetActive(selected);
        else
            Debug.LogWarning("selectionBorder为null!");

        // 选中效果
        if (background != null && characterData != null)
        {
            if (selected)
            {
                background.color = new Color(
                    characterData.characterColor.r * 1.2f,
                    characterData.characterColor.g * 1.2f,
                    characterData.characterColor.b * 1.2f
                );
            }
            else
            {
                background.color = characterData.characterColor;
            }
        }
    }

    public CharacterData GetCharacter()
    {
        return characterData;
    }
}