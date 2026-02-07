using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEffectExecutor : MonoBehaviour
{
    public static CardEffectExecutor Instance;

    [Header("特效")]
    public GameObject damageEffectPrefab;
    public GameObject healEffectPrefab;
    public GameObject shieldEffectPrefab;
    public GameObject statusEffectPrefab;

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

    // 执行卡牌效果
    public IEnumerator ExecuteCardEffects(CardData card, CharacterData caster, CharacterData target, BattleData battleData)
    {
        Debug.Log($"执行卡牌效果: {card.cardName}, 施法者: {caster.characterName}, 目标: {target?.characterName ?? "无"}");

        // 遍历所有效果
        foreach (var effect in card.effects)
        {
            yield return ExecuteSingleEffect(effect, card, caster, target, battleData);
            yield return new WaitForSeconds(0.3f); // 效果间隔
        }
    }

    // 执行单个效果
    IEnumerator ExecuteSingleEffect(CardEffect effect, CardData card, CharacterData caster, CharacterData target, BattleData battleData)
    {
        Debug.Log($"执行效果: {effect.effectName}, 值: {effect.value}");

        switch (effect.effectName)
        {
            case "造成伤害":
                yield return ExecuteDamageEffect(effect, caster, target, battleData);
                break;

            case "治疗":
                yield return ExecuteHealEffect(effect, caster, target, battleData);
                break;

            case "添加护盾":
                yield return ExecuteShieldEffect(effect, caster, target);
                break;

            case "附加腐烂":
                yield return ExecuteAddStatusEffect(effect, caster, target, StatusType.Rot);
                break;

            case "施加衰弱":
                yield return ExecuteAddStatusEffect(effect, caster, target, StatusType.Weak);
                break;

            case "施加易伤":
                yield return ExecuteAddStatusEffect(effect, caster, target, StatusType.Strong);
                break;

            case "抽牌":
                yield return ExecuteDrawCardEffect(effect, caster);
                break;

            case "引爆腐烂":
                yield return ExecuteTriggerRotEffect(effect, target, battleData);
                break;

            case "随机伤害":
                yield return ExecuteRandomDamageEffect(effect, caster, battleData);
                break;

            default:
                Debug.LogWarning($"未知效果类型: {effect.effectName}");
                break;
        }
    }

    // 执行伤害效果
    IEnumerator ExecuteDamageEffect(CardEffect effect, CharacterData caster, CharacterData target, BattleData battleData)
    {
        if (target == null || target.isDead) yield break;

        // 计算伤害，考虑易伤/衰弱状态
        int damage = effect.value;
        damage += caster.GetDamageModifier(); // 施法者的易伤加成
        damage -= target.GetDamageModifier(); // 目标的衰弱减免

        // 确保伤害不为负
        damage = Mathf.Max(1, damage);

        Debug.Log($"{caster.characterName} 对 {target.characterName} 造成 {damage} 点伤害");

        // 显示伤害特效
        if (damageEffectPrefab != null && target.characterClass != CharacterClass.Universal)
        {
            ShowEffectAtCharacter(damageEffectPrefab, target, $"{damage}");
        }

        // 应用伤害
        target.TakeDamage(damage);

        yield return new WaitForSeconds(0.5f);
    }

    // 执行治疗效果
    IEnumerator ExecuteHealEffect(CardEffect effect, CharacterData caster, CharacterData target, BattleData battleData)
    {
        if (target == null || target.isDead) yield break;

        int healAmount = effect.value;

        // 检查是否有"下次治疗双倍"效果
        if (caster.HasTemporaryEffect("下次治疗双倍"))
        {
            healAmount *= 2;
            caster.RemoveTemporaryEffect("下次治疗双倍");
            Debug.Log("触发治疗双倍效果");
        }

        Debug.Log($"{caster.characterName} 治疗 {target.characterName} {healAmount} 点生命");

        // 显示治疗特效
        if (healEffectPrefab != null)
        {
            ShowEffectAtCharacter(healEffectPrefab, target, $"+{healAmount}");
        }

        // 应用治疗
        target.Heal(healAmount);

        yield return new WaitForSeconds(0.5f);
    }

    // 执行护盾效果
    IEnumerator ExecuteShieldEffect(CardEffect effect, CharacterData caster, CharacterData target)
    {
        if (target == null || target.isDead) yield break;

        int shieldAmount = effect.value;

        Debug.Log($"{caster.characterName} 为 {target.characterName} 添加 {shieldAmount} 点护盾");

        // 显示护盾特效
        if (shieldEffectPrefab != null)
        {
            ShowEffectAtCharacter(shieldEffectPrefab, target, $"+{shieldAmount}护盾");
        }

        // 添加护盾
        target.AddShield(shieldAmount);

        yield return new WaitForSeconds(0.3f);
    }

    // 执行添加状态效果
    IEnumerator ExecuteAddStatusEffect(CardEffect effect, CharacterData caster, CharacterData target, StatusType statusType)
    {
        if (target == null || target.isDead) yield break;

        int stacks = effect.statusStacks;

        Debug.Log($"{caster.characterName} 对 {target.characterName} 施加 {stacks} 层{GetStatusName(statusType)}");

        // 显示状态特效
        if (statusEffectPrefab != null)
        {
            ShowEffectAtCharacter(statusEffectPrefab, target, $"+{stacks}{GetStatusSymbol(statusType)}");
        }

        // 添加状态
        target.AddStatus(statusType, stacks);

        yield return new WaitForSeconds(0.3f);
    }

    // 执行抽牌效果
    IEnumerator ExecuteDrawCardEffect(CardEffect effect, CharacterData caster)
    {
        int drawCount = effect.value;

        Debug.Log($"{caster.characterName} 抽 {drawCount} 张牌");

        // 需要从牌库抽牌的逻辑
        // 暂时只记录日志

        yield return new WaitForSeconds(0.2f);
    }

    // 执行引爆腐烂效果
    IEnumerator ExecuteTriggerRotEffect(CardEffect effect, CharacterData target, BattleData battleData)
    {
        if (target == null || target.isDead) yield break;

        Debug.Log($"引爆 {target.characterName} 的腐烂效果");

        // 触发腐烂状态
        target.TriggerStatusEffect(StatusType.Rot);

        yield return new WaitForSeconds(0.5f);
    }

    // 执行随机伤害效果
    IEnumerator ExecuteRandomDamageEffect(CardEffect effect, CharacterData caster, BattleData battleData)
    {
        // 获取所有存活的敌人
        List<CharacterData> enemies = new List<CharacterData>(battleData.enemyTeam);
        enemies.RemoveAll(e => e.isDead);

        if (enemies.Count == 0) yield break;

        Debug.Log($"{caster.characterName} 施放随机伤害");

        // 随机选择目标3次
        for (int i = 0; i < 3; i++)
        {
            if (enemies.Count == 0) break;

            int randomIndex = Random.Range(0, enemies.Count);
            CharacterData randomTarget = enemies[randomIndex];

            // 执行伤害
            CardEffect damageEffect = new CardEffect
            {
                effectName = "造成伤害",
                value = effect.value
            };

            yield return ExecuteDamageEffect(damageEffect, caster, randomTarget, battleData);
        }
    }

    // 在角色位置显示特效
    void ShowEffectAtCharacter(GameObject effectPrefab, CharacterData character, string text = "")
    {
        CharacterBattleUI characterUI = FindCharacterUI(character);
        if (characterUI != null)
        {
            GameObject effect = Instantiate(effectPrefab, characterUI.transform);
            effect.transform.localPosition = Vector3.zero;

            // 如果有文本组件，则显示文本
            Text effectText = effect.GetComponentInChildren<Text>();
            if (effectText != null && !string.IsNullOrEmpty(text))
            {
                effectText.text = text;
            }

            // 自动销毁
            Destroy(effect, 2f);
        }
    }

    CharacterBattleUI FindCharacterUI(CharacterData character)
    {
        CharacterBattleUI[] allUIs = FindObjectsOfType<CharacterBattleUI>();
        foreach (var ui in allUIs)
        {
            if (ui.characterData == character)
            {
                return ui;
            }
        }
        return null;
    }

    string GetStatusName(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return "腐烂";
            case StatusType.Strong: return "易伤";
            case StatusType.Weak: return "衰弱";
            case StatusType.Shield: return "护盾";
            default: return "状态";
        }
    }

    string GetStatusSymbol(StatusType status)
    {
        switch (status)
        {
            case StatusType.Rot: return "腐";
            case StatusType.Strong: return "易";
            case StatusType.Weak: return "衰";
            case StatusType.Shield: return "盾";
            default: return "状";
        }
    }
}