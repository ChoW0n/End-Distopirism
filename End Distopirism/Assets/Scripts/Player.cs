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
            // 새로운 효과 추가
            switch (effectName)
            {
                case "출혈":
                    statusEffects.Add(new StatusEffect("출혈", 3, 0, 0.01f));
                    break;
                case "혼란":
                    statusEffects.Add(new StatusEffect("혼란", 1, -20f, 0));
                    break;
                case "독":
                    statusEffects.Add(new StatusEffect("독", 3, 0, 0, 0.05f, -0.10f));
                    break;
                case "방어력감소":
                    statusEffects.Add(new StatusEffect("방어력감소", 1, 0, 0, 0, 0, 0.5f));
                    break;
                case "출혈2턴":
                    statusEffects.Add(new StatusEffect("출혈", 2, 0, 0.01f));
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
        statusEffects.RemoveAll(effect => effect.duration <= 0);
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
