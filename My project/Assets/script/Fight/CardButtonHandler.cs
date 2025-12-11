using UnityEngine;
using UnityEngine.UI;

public class CardButtonHandler : MonoBehaviour
{
    public int cardIndex = 0;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCardClicked);
        }
        else
        {
            Debug.LogError("按钮组件不存在！");
        }
    }

    public void OnCardClicked()
    {
        Debug.Log($"卡牌按钮被点击，索引: {cardIndex}");
        if (CardGameManager.Instance != null)
        {
            CardGameManager.Instance.PlayCard(cardIndex);
        }
        else
        {
            Debug.LogError("CardGameManager的Instance为空！");
        }
    }
}