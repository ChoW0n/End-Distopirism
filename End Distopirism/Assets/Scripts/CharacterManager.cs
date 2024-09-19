using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    public Player GetPlayer => player;

    public bool Live;

    public int bonusdmg = 0;    //diff 차이에 따른 데미지 증가값

    public int coinbonus = 0; //코인 보너스
    public int successCount = 0;  //성공 횟수

    public int totalDamageDealt = 0; // 이번 턴에 입힌 총 데미지

    void Start()
    {
        player.Coin = player.MaxCoin;
        if (Live == false)
        {
            if (player.hp > 0)
            {
                Live = true;
            }
        }

        // 스킬을 적용하여 캐릭터의 데미지 값을 설정합니다.
        ApplySkill();
    }

    // 스킬을 캐릭터에 적용하는 메서드
    void ApplySkill()
    {
        if (player.skills != null && player.skills.Count > 0)
        {
            // 스킬을 적용하여 캐릭터의 데미지 값을 설정하는 예시
            Skill skill = player.skills[0]; // 예시로 첫 번째 스킬만 적용
            player.MaxDmg = skill.MaxDmg;
            player.MinDmg = skill.MinDmg;
            player.DmgUp = skill.DmgUp;
            // 필요한 경우 추가로 처리할 코드
            Debug.Log("스킬이 캐릭터에 적용되었습니다: " + skill.skillName);
        }
    }

    public void OnDeath()
    {
        gameObject.SetActive(false);
        // todo: 이팩트 재생
    }
}