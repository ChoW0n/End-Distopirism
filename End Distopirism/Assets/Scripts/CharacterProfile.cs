using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterProfile : MonoBehaviour
{
    [SerializeField]
    private Player player;
    public Player GetPlayer => player;

    public bool live;

    public int bonusDmg = 0;    //diff 차이에 따른 데미지 증가값

    public int coinBonus = 0; //코인 보너스
    public int successCount = 0;  //성공 횟수

    public Vector3 originalPosition; // 원래 위치를 저장할 변수 추가

    void Start()
    {
        player.coin = player.maxCoin;
        if (live == false)
        {
            if (player.hp > 0)
            {
                live = true;
            }
        }

        // 현재 위치를 원래 위치로 저장
        originalPosition = transform.position;

        // 스킬을 적용하여 캐릭터의 데미지 값을 설정합니다.
        ApplySkill();
    }

    public void ShowCharacterInfo()
    {
        UIManager.Instance.ShowCharacterInfo(this);
    }

    // 스킬을 캐릭터에 적용하는 메서드
    void ApplySkill()
    {
        if (player.skills != null && player.skills.Count > 0)
        {
            // 스킬을 적용하여 캐릭터의 데미지 값을 설정하는 예시
            Skill skill = player.skills[0]; // 예시로 첫 번째 스킬만 적용
            player.maxDmg = skill.maxDmg;
            player.minDmg = skill.minDmg; 
            player.dmgUp = skill.dmgUp; 
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
