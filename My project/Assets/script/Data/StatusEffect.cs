using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public StatusType type;
    public string statusName;
    public int stacks;
    public int duration; // -1表示永久，否则每回合减少

    public StatusEffect(StatusType type, int stacks, int duration = -1)
    {
        this.type = type;
        this.stacks = stacks;
        this.duration = duration;

        switch (type)
        {
            case StatusType.Rot:
                statusName = "腐烂";
                break;
            case StatusType.Strong:
                statusName = "强壮";
                break;
            case StatusType.Weak:
                statusName = "衰弱";
                break;
            case StatusType.Shield:
                statusName = "护盾";
                break;
            default:
                statusName = "未知状态";
                break;
        }
    }

    // 回合开始时的效果
    public void ApplyTurnStartEffect(CharacterData character)
    {
        switch (type)
        {
            case StatusType.Rot:
                // 腐烂效果：回合开始前造成伤害，无视护盾
                character.TakeDamage(stacks, true);
                Debug.Log($"{character.characterName} 受到 {stacks} 点腐烂伤害");
                break;

            case StatusType.Strong:
                Debug.Log($"{character.characterName} 获得 {stacks} 层强壮效果");
                break;

            case StatusType.Weak:
                Debug.Log($"{character.characterName} 受到 {stacks} 层衰弱效果");
                break;
        }
    }

    public void ReduceDuration()
    {
        if (duration > 0)
        {
            duration--;
            if (duration == 0)
            {
                stacks = 0;
            }
        }
    }
}