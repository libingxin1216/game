using UnityEngine;
using UnityEngine.UI;

public class DebugScript : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 开始调试 ===");

        // 1. 检查GameDataManager
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager实例为null!");
        }
        else
        {
            Debug.Log("✓ GameDataManager存在");

            // 2. 检查角色数据
            var characters = GameDataManager.Instance.GetAllCharacters();
            Debug.Log($"角色数量: {characters?.Count ?? 0}");

            if (characters != null && characters.Count > 0)
            {
                foreach (var character in characters)
                {
                    Debug.Log($"角色: {character.characterName}, 颜色: {character.characterColor}, 卡牌数: {character.uniqueDeck?.Count ?? 0}");
                }
            }
            else
            {
                Debug.LogError("没有加载到角色数据!");
            }

            // 3. 检查通用卡牌
            var universalCards = GameDataManager.Instance.GetUniversalCards();
            Debug.Log($"通用卡牌数量: {universalCards?.Count ?? 0}");
        }

        // 4. 检查UI组件
        CheckUIComponents();

        Debug.Log("=== 调试结束 ===");
    }

    void CheckUIComponents()
    {
        // 检查Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas存在: {canvas.name}, 渲染模式: {canvas.renderMode}");
        }
        else
        {
            Debug.LogError("Canvas不存在!");
        }

        // 检查CharacterLibraryUI脚本
        CharacterLibraryUI ui = FindObjectOfType<CharacterLibraryUI>();
        if (ui != null)
        {
            Debug.Log("CharacterLibraryUI脚本存在");

           
        }
        else
        {
            Debug.LogError("CharacterLibraryUI脚本不存在!");
        }
    }
}