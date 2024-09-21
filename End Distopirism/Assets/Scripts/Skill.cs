using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill")]
public class Skill : ScriptableObject
{
    public string skillName;    // 스킬 이름
    public Sprite sprite;       // 스킬 아이콘
    public int maxDmg;          // 스킬의 최대 데미지
    public int minDmg;          // 스킬의 최소 데미지
    public int dmgUp;           // 레벨 상승 시 증가 데미지
    [TextArea(3, 5)]
    public string skillEffect;  // 스킬 효과 설명
    public AnimationClip animationClip;     //해당 스킬 애니메이션

    // 스킬의 정보를 출력하는 메서드
    public void PrintSkillInfo()
    {
        Debug.Log("스킬 이름: " + skillName);
        Debug.Log("최대 데미지: " + maxDmg);
        Debug.Log("최소 데미지: " + minDmg);
        Debug.Log("레벨 상승 시 데미지 증가: " + dmgUp);
        Debug.Log("스킬 효과: " + skillEffect);
    }
}