using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleData
{
    // 战斗双方
    public List<CharacterData> playerTeam = new List<CharacterData>();
    public List<CharacterData> enemyTeam = new List<CharacterData>();

    // 当前回合信息
    public bool isPlayerTurn = true;
    public int currentTurn = 1;

    // 当前操作的角色索引
    public int currentPlayerCharacterIndex = 0;
    public int currentTargetCharacterIndex = -1;

    // 卡槽数据
    public class CharacterCardSlots
    {
        public CharacterData character;
        public CardData[] slots = new CardData[3]; // 3个卡槽
        public CharacterData target; // 目标角色

        public bool HasCardInSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < slots.Length && slots[slotIndex] != null;
        }

        public int GetFirstEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) return i;
            }
            return -1;
        }

        public bool IsAllSlotsOccupied()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) return false;
            }
            return true;
        }

        public void ClearSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = null;
            }
            target = null;
        }
    }

    public Dictionary<CharacterData, CharacterCardSlots> characterSlots =
        new Dictionary<CharacterData, CharacterCardSlots>();

    // 初始化战斗
    public void Initialize(List<CharacterData> playerTeam, List<CharacterData> enemyTeam)
    {
        this.playerTeam = playerTeam;
        this.enemyTeam = enemyTeam;

        isPlayerTurn = true;
        currentTurn = 1;

        // 为每个角色初始化卡槽
        characterSlots.Clear();
        foreach (var character in playerTeam)
        {
            characterSlots[character] = new CharacterCardSlots { character = character };
        }
        foreach (var character in enemyTeam)
        {
            characterSlots[character] = new CharacterCardSlots { character = character };
        }
    }

    // 检查战斗是否结束
    public bool IsBattleOver()
    {
        bool allPlayersDead = true;
        bool allEnemiesDead = true;

        foreach (var character in playerTeam)
        {
            if (!character.isDead) allPlayersDead = false;
        }

        foreach (var character in enemyTeam)
        {
            if (!character.isDead) allEnemiesDead = false;
        }

        return allPlayersDead || allEnemiesDead;
    }

    // 获取胜利方
    public string GetWinner()
    {
        bool allPlayersDead = true;
        bool allEnemiesDead = true;

        foreach (var character in playerTeam)
        {
            if (!character.isDead) allPlayersDead = false;
        }

        foreach (var character in enemyTeam)
        {
            if (!character.isDead) allEnemiesDead = false;
        }

        if (allPlayersDead) return "Enemy";
        if (allEnemiesDead) return "Player";
        return "None";
    }
}