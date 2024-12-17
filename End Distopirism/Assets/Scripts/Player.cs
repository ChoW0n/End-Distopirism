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

    // 다음 턴 코인 수정치 추가
    public int nextTurnCoinModifier = 0;

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

    // 덱 관련 메서드들
    public bool CanAddCardToDeck(Skill skill)
    {
        int totalCardsInDeck = skills.Count;
        if (totalCardsInDeck >= maxDeckSize)
        {
            Debug.LogWarning($"덱이 가득 찼습니다. (최대 {maxDeckSize}장)");
            return false;
        }

        if (skill.maxCardCount > 0)
        {
            int currentCount = skills.Count(s => s == skill);
            if (currentCount >= skill.maxCardCount)
            {
                Debug.LogWarning($"{skill.skillName}는 최대 {skill.maxCardCount}장까지만 추가할 수 있습니다.");
                return false;
            }
        }

        if (skill.cardType.ToLower() != "일반")
        {
            if (!currentCardTypes.Contains(skill.cardType))
            {
                var uniqueTypes = skills
                    .Where(s => s.cardType.ToLower() != "일반")
                    .Select(s => s.cardType)
                    .Distinct()
                    .Count();

                if (uniqueTypes >= maxUniqueCardTypes)
                {
                    Debug.LogWarning($"서로 다른 종류의 카드는 최대 {maxUniqueCardTypes}종류까지만 추가할 수 있습니다. (일반 카드 제외)");
                    return false;
                }
            }
        }

        return true;
    }

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

    public bool RemoveCardFromDeck(Skill skill)
    {
        if (skills.Remove(skill))
        {
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
