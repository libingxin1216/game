using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("场景名称")]
    public string mainMenuScene = "MainMenu";
    public string characterLibraryScene = "CharacterLibrary";
    public string battleScene = "Battle";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 确保GameDataManager存在
            EnsureGameDataManager();

            Debug.Log("GameManager初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void EnsureGameDataManager()
    {
        // 如果GameDataManager不存在，创建它
        if (FindObjectOfType<GameDataManager>() == null)
        {
            GameObject go = new GameObject("GameDataManager");
            go.AddComponent<GameDataManager>();
            DontDestroyOnLoad(go);
            Debug.Log("创建GameDataManager");
        }
    }

    public void LoadMainMenu()
    {
        Debug.Log($"加载主菜单场景: {mainMenuScene}");
        SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadCharacterLibrary()
    {
        Debug.Log($"加载角色库场景: {characterLibraryScene}");
        SceneManager.LoadScene(characterLibraryScene);
    }

    public void LoadBattleScene()
    {
        Debug.Log($"加载战斗场景: {battleScene}");
        SceneManager.LoadScene(battleScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}