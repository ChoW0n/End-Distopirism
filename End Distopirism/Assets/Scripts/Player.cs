using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Player
{
    public string charName;

    // 채력
    public int maxHp;

    //행동력
    public int charSkillCost;

    public int hp;

    // 방어 레벨
    public int defLevel;
    public int DefLevel => defLevel;

    // 공격
    public int dmgLevel;

    [HideInInspector]
    public int dmg;

    [HideInInspector]
    public int maxDmg;

    [HideInInspector]
    public int minDmg;

    [HideInInspector]
    public int dmgUp;   // 코인 성공 시 1개당 데미지 증가값

    // 코인
    public int maxCoin; //최대 코인량
    public int coin;    //동일 피해 합 시작 시 인식할 코인 합 패배 시 1개씩 차감

    // 정신력
    public float menTality = 100f;

    // 스킬 리스트 추가
    // Unity 인스펙터에서 스킬 ScriptableObject를 등록할 수 있도록 함
    public List<Skill> skills;

    public bool isFlipped { get; set; }

    public Sprite charSprite; // 캐릭터 스프라이트

    // 상태 효과 리스트 추가
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    // 다음 턴 코인 수정치 추가
    public int nextTurnCoinModifier = 0;

    public bool isBleedingEffect = false;    // 출혈
    public bool isConfusionEffect = false;   // 혼란
    public bool isPoisonEffect = false;      // 독
    public bool isDefenseDownEffect = false; // 방어력감소

    // 각 상태이상의 남은 턴 수
    public int bleedingTurns = 0;
    public int confusionTurns = 0;
    public int poisonTurns = 0;
    public int defenseDownTurns = 0;

    public void Init()
    {
        hp = maxHp;
    }

    // 상태 효과 관련 메서드들
    public void AddStatusEffect(string effectName)
    {
        // 기존 효과 찾기
        var existingEffect = statusEffects.Find(e => e.effectName == effectName);
        
        if (existingEffect != null)
        {
            // 기존 효과가 있다면 지속시간 증가
            existingEffect.duration += GetNewEffectDuration(effectName);
            Debug.Log($"{effectName} 효과가 중첩되어 지속시간이 {existingEffect.duration}턴이 되었습니다.");
        }
        else
        {
            switch (effectName)
            {
                case "출혈":
                    isBleedingEffect = true;
                    bleedingTurns = 3;
                    break;
                case "혼란":
                    isConfusionEffect = true;
                    confusionTurns = 1;
                    break;
                case "독":
                    isPoisonEffect = true;
                    poisonTurns = 3;
                    break;
                case "방어력감소":
                    isDefenseDownEffect = true;
                    defenseDownTurns = 1;
                    break;
                case "출혈2턴":
                    isBleedingEffect = true;
                    bleedingTurns = 2;
                    break;
            }
        }
    }

    private int GetNewEffectDuration(string effectName)
    {
        switch (effectName)
        {
            case "출혈": return 3;
            case "혼란": return 1;
            case "독": return 3;
            case "방어력감소": return 1;
            case "출혈2턴": return 2;
            default: return 1;
        }
    }

    public void UpdateStatusEffects()
    {
        // 턴 감소 및 상태이상 해제
        if (bleedingTurns > 0) bleedingTurns--;
        if (confusionTurns > 0) confusionTurns--;
        if (poisonTurns > 0) poisonTurns--;
        if (defenseDownTurns > 0) defenseDownTurns--;

        // 턴이 0이 되면 상태이상 해제
        if (bleedingTurns <= 0) isBleedingEffect = false;
        if (confusionTurns <= 0) isConfusionEffect = false;
        if (poisonTurns <= 0) isPoisonEffect = false;
        if (defenseDownTurns <= 0) isDefenseDownEffect = false;
    }


    public float GetTotalMentalityModifier()
    {
        float totalModifier = 0;
        foreach (var effect in statusEffects)
        {
            totalModifier += effect.mentalityModifier;
        }
        return totalModifier;
    }
}
