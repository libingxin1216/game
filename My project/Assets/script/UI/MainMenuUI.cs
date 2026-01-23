using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 添加这一行

public class MainMenuUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Button characterLibraryButton;
    [SerializeField] private Button startBattleButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        // 按钮事件绑定
        if (characterLibraryButton != null)
            characterLibraryButton.onClick.AddListener(OpenCharacterLibrary);

        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(StartBattle);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        Debug.Log("主菜单UI初始化完成");
    }

    void OpenCharacterLibrary()
    {
        Debug.Log("打开角色库");
        GameManager.Instance.LoadCharacterLibrary();
    }

    void StartBattle()
    {
        Debug.Log("开始战斗按钮被点击");

        // 方法1：直接通过GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log("通过GameManager加载战斗场景");
            GameManager.Instance.LoadBattleScene();
        }
        // 方法2：直接加载场景（备用）
        else
        {
            Debug.Log("直接加载战斗场景");
            SceneManager.LoadScene("Battle");
        }
    }

    void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}