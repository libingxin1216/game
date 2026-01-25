using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    [Header("状态图标")]
    public Sprite rotIcon;
    public Sprite strongIcon;
    public Sprite weakIcon;
    public Sprite shieldIcon;

    [Header("状态颜色")]
    public Color rotColor = new Color(0.5f, 0.2f, 0.5f); // 紫色
    public Color strongColor = Color.red;
    public Color weakColor = Color.blue;
    public Color shieldColor = Color.cyan;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 获取状态图标
    public Sprite GetStatusIcon(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return rotIcon;
            case StatusType.Strong: return strongIcon;
            case StatusType.Weak: return weakIcon;
            case StatusType.Shield: return shieldIcon;
            default: return null;
        }
    }

    // 获取状态颜色
    public Color GetStatusColor(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return rotColor;
            case StatusType.Strong: return strongColor;
            case StatusType.Weak: return weakColor;
            case StatusType.Shield: return shieldColor;
            default: return Color.white;
        }
    }

    // 获取状态描述
    public string GetStatusDescription(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot:
                return "腐烂：回合开始前造成层数伤害，无视护盾";
            case StatusType.Strong:
                return "强壮：直接伤害增加层数";
            case StatusType.Weak:
                return "衰弱：直接伤害减少层数";
            case StatusType.Shield:
                return "护盾：吸收伤害的临时生命值，每回合清空";
            default:
                return "未知状态";
        }
    }

    // 应用状态效果到角色
    public void ApplyStatusToCharacter(CharacterData character, StatusType status, int stacks, int duration = -1)
    {
        if (character == null || character.isDead) return;

        StatusEffect existingStatus = character.statusEffects.Find(s => s.type == status);

        if (existingStatus != null)
        {
            // 叠加层数
            existingStatus.stacks += stacks;
            if (duration > 0 && existingStatus.duration < duration)
            {
                existingStatus.duration = duration;
            }

            Debug.Log($"{character.characterName} 的 {GetStatusName(status)} 叠加到 {existingStatus.stacks} 层");
        }
        else
        {
            // 添加新状态
            StatusEffect newStatus = new StatusEffect(status, stacks, duration);;

            character.statusEffects.Add(newStatus);
            Debug.Log($"{character.characterName} 获得 {stacks} 层{GetStatusName(status)}");
        }
    }

    string GetStatusName(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return "腐烂";
            case StatusType.Strong: return "强壮";
            case StatusType.Weak: return "衰弱";
            case StatusType.Shield: return "护盾";
            default: return "状态";
        }
    }
}