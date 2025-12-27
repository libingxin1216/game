using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public Sprite portrait;
    public bool isLeft; // true左边出现 false右边出现
    [TextArea(3, 5)]
    public string content;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

