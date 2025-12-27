using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public DialogueData dialogue;

    public Image leftImg;
    public Image rightImg;

    public TMP_Text nameText;
    public TMP_Text contentText;

    int index = 0;

    void Start()
    {
        if (PlayerPrefs.HasKey("DialogueIndex"))
            Load();
        else
            ShowLine();
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    void ShowLine()
    {
        if (index >= dialogue.lines.Length)
        {
            Debug.Log("剧情结束");
            return;
        }

        var line = dialogue.lines[index];

        nameText.text = line.characterName;
        contentText.text = line.content;

        if (line.isLeft)
        {
            leftImg.gameObject.SetActive(true);
            rightImg.gameObject.SetActive(false);
            leftImg.sprite = line.portrait;
        }
        else
        {
            rightImg.gameObject.SetActive(true);
            leftImg.gameObject.SetActive(false);
            rightImg.sprite = line.portrait;
        }
    }

    public void NextLine()
    {
        index++;
        ShowLine();
    }

    // --- 存档读取 ---
    public void Save()
    {
        PlayerPrefs.SetInt("DialogueIndex", index);
        PlayerPrefs.Save();
        Debug.Log("存档成功，当前进度：" + index);
    }


    public void Load()
    {
        if (PlayerPrefs.HasKey("DialogueIndex"))
        {
            index = PlayerPrefs.GetInt("DialogueIndex");
            ShowLine();
            Debug.Log("读取存档成功，跳到进度：" + index);
        }
        else
        {
            Debug.Log("没有存档");
        }
    }

}
