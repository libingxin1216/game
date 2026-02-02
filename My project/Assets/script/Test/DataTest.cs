using UnityEngine;


public class DataTest : MonoBehaviour
{
    void Start()
    {
        if (GameDataManager.Instance != null)
        {
            Debug.Log($"角色数量: {GameDataManager.Instance.allCharacters.Count}");
            Debug.Log($"通用卡牌数量: {GameDataManager.Instance.universalCards.Count}");

            foreach (var character in GameDataManager.Instance.allCharacters)
            {
                Debug.Log($"角色: {character.characterName}, 卡牌数量: {character.uniqueDeck.Count}");
            }
        }
    }
}
