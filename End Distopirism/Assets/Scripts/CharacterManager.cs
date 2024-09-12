using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public string CharName;
    public int maxHp;   // 최대 체력
    public int hp;      // 현재 체력

    public int DefLevel;     // 방어 레벨

    public int DmgLevel;  // 최소 데미지
    public int Dmg;     // 최종 데미지
    public int MaxDmg;
    public int MinDmg;  // 최대 데미지
    public int DmgUp;   // 코인 성공 시 1개당 데미지 증가값
    public int MaxCoin; //최대 코인량
    public int Coin; //동일 피해 합 시작 시 인식할 코인 합 패배 시 1개씩 차감
    public int MenTality = 100; //정신력

    public bool Live;

    // 등록된 스킬 프리팹
    public Skill skill;  // Unity 인스펙터에서 스킬 ScriptableObject를 등록할 수 있도록 함

    void Start()
    {
        Coin = MaxCoin;
        if (Live == false)
        {
            if (hp > 0)
            {
                Live = true;
            }
        }

        // 스킬을 적용하여 캐릭터의 데미지 값을 설정합니다.
        ApplySkill();
    }

    // Update is called once per frame
    void Update()
    {
        if (Live == true)
        {
            if (0 >= hp)
            {
                Live = false;
                gameObject.SetActive(false);
            }
        }
    }

    // 스킬을 캐릭터에 적용하는 메서드
    void ApplySkill()
    {
        if (skill != null)
        {
            MaxDmg = skill.MaxDmg;
            MinDmg = skill.MinDmg;
            DmgUp = skill.DmgUp;
            // 필요한 경우 추가로 처리할 코드
            Debug.Log("스킬이 캐릭터에 적용되었습니다: " + skill.skillName);
        }
    }
}
