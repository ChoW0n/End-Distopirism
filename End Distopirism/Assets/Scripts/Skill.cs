using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill")]
public class Skill : ScriptableObject
{
    public string skillName;    // 스킬 이름
    public Sprite sprite;       // 전투 UI 스킬 아이콘
    public int minDmg;          // 스킬의 최소 데미지
    public int dmgUp;           // 코인 성공 시 증가 데미지
    [TextArea(3, 5)]
    public string skillEffect;  // 스킬 효과 설명
    public AnimationClip animationClip;     //해당 스킬 애니메이션
    public Sprite successSprite; // 시전 성공 시 사용할 스프라이트

    public Sprite nomalSprite; //카드 상태일때 스프라이트

    [Header("카드 종류 설정")]
    [Tooltip("카드의 종류 (일반/레어/에픽/레전드 등)")]
    public string cardType = "일반";  // 기본값은 "일반"으로 설정

    [Header("덱 빌딩 설정")]
    [Tooltip("덱에 넣을 수 있는 최대 카드 수 (0 = 제한 없음)")]
    public int maxCardCount = 3;  // 덱에 넣을 수 있는 최대 카드 수

    // 스킬의 정보를 출력하는 메서드
    public void PrintSkillInfo()
    {
        Debug.Log("스킬 이름: " + skillName);
        Debug.Log("최소 데미지: " + minDmg);
        Debug.Log("레벨 상승 시 데미지 증가: " + dmgUp);
        Debug.Log("스킬 효과: " + skillEffect);
    }
}