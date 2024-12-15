using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;  // LINQ 사용을 위해 추가

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

    [Header("덱 제한 설정")]
    public int maxDeckSize = 10;  // 전체 덱 크기 제한
    public int maxUniqueCardTypes = 3;  // 서로 다른 종류의 카드 제한 (일반 카드 제외)
    
    // 현재 덱에 있는 카드 종류들을 추적
    [HideInInspector]
    public HashSet<string> currentCardTypes = new HashSet<string>();

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

    // 덱에 카드를 추가할 수 있는지 확인하는 메서드
    public bool CanAddCardToDeck(Skill skill)
    {
        // 1. 전체 덱 크기 제한 체크 (최우선)
        int totalCardsInDeck = skills.Count;
        if (totalCardsInDeck >= maxDeckSize)
        {
            Debug.LogWarning($"덱이 가득 찼습니다. (최대 {maxDeckSize}장)");
            return false;
        }

        // 2. 개별 카드의 최대 개수 제한 체크
        if (skill.maxCardCount > 0)
        {
            int currentCount = skills.Count(s => s == skill);
            if (currentCount >= skill.maxCardCount)
            {
                Debug.LogWarning($"{skill.skillName}는 최대 {skill.maxCardCount}장까지만 추가할 수 있습니다.");
                return false;
            }
        }

        // 3. 서로 다른 종류의 카드 제한 체크 (일반 카드 제외)
        if (skill.cardType.ToLower() != "일반")
        {
            // 새로운 종류의 카드인 경우에만 체크
            if (!currentCardTypes.Contains(skill.cardType))
            {
                // 현재 덱에 있는 서로 다른 종류의 카드 수 계산 (일반 제외)
                var uniqueTypes = skills
                    .Where(s => s.cardType.ToLower() != "일반")
                    .Select(s => s.cardType)
                    .Distinct()
                    .Count();

                // 새로운 종류를 추가했을 때 제한을 넘는지 체크
                if (uniqueTypes >= maxUniqueCardTypes)
                {
                    Debug.LogWarning($"서로 다른 종류의 카드는 최대 {maxUniqueCardTypes}종류까지만 추가할 수 있습니다. (일반 카드 제외)");
                    return false;
                }
            }
        }

        return true;
    }

    // 덱에 카드를 추가하는 메서드
    public bool AddCardToDeck(Skill skill)
    {
        if (!CanAddCardToDeck(skill))
            return false;

        skills.Add(skill);
        if (skill.cardType.ToLower() != "일반")
        {
            currentCardTypes.Add(skill.cardType);
        }
        return true;
    }

    // 덱에서 카드를 제거하는 메서드
    public bool RemoveCardFromDeck(Skill skill)
    {
        if (skills.Remove(skill))
        {
            // 해당 종류의 카드가 더 이상 없다면 currentCardTypes에서 제거
            if (skill.cardType.ToLower() != "일반" && 
                !skills.Exists(s => s.cardType == skill.cardType))
            {
                currentCardTypes.Remove(skill.cardType);
            }
            return true;
        }
        return false;
    }
}
