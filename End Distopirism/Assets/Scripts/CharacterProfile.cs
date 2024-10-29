using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterProfile : MonoBehaviour
{
    [SerializeField]
    private Player player;
    public Player GetPlayer => player;

    [SerializeField]
    private SkillManager skillManager;

    public bool live;

    public int bonusDmg = 0;    //diff 차이에 따른 데미지 증가값

    public int coinBonus = 0; //코인 보너스
    public int successCount = 0;  //성공 횟수

    public bool isSelected = false; // 선택된 상태를 나타내는 변수

    private float initialYRotation;

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

        // 스킬 적용하여 캐릭터의 데미지 값을 설정합니다.
        ApplySkill();

        initialYRotation = transform.rotation.eulerAngles.y;
    }

    public void ShowCharacterInfo()
    {
        UIManager.Instance.ShowCharacterInfo(this);
        
        // 선택 상태에 따라 스킬 카드 처리
        if (isSelected)
        {
            if (skillManager != null)
            {
                Debug.Log($"캐릭터 {player.charName}의 스킬 카드 활성화");
                skillManager.AssignRandomSkillSprites(player.skills);
            }
        }
        else
        {
            HideSkillCards();
        }
    }

    // 스킬 카드를 숨기는 메서드 추가
    public void HideSkillCards()
    {
        if (skillManager != null)
        {
            Debug.Log($"캐릭터 {player.charName}의 스킬 카드 비활성화");
            skillManager.DeactivateCards();
        }
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

    void Update()
    {
        // 캐릭터가 카메라를 계속 바라보게 합니다.
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 direction = mainCamera.transform.position - transform.position;
            direction.x = 0; // x축 회전을 방지하여 y축만 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Vector3 euler = targetRotation.eulerAngles;
            euler.y = initialYRotation; // 초기 Y 회전 값 유지
            transform.rotation = Quaternion.Euler(euler);
        }

        // 태그에 따라 Y 회전값 수정
        if (CompareTag("Player"))
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.y = initialYRotation; // Player일 경우 초기 Y 회전 유지
            transform.rotation = Quaternion.Euler(euler);
        }
        else if (CompareTag("Enemy"))
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.y = -initialYRotation; // Enemy일 경우 Y 회전을 음수로 설정
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
