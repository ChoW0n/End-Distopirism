using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill")]
public class Skill : ScriptableObject
{
    public string skillName;    // 스킬 이름
    public Sprite Sprite;       // 스킬 아이콘
    public int MaxDmg;          // 스킬의 최대 데미지
    public int MinDmg;          // 스킬의 최소 데미지
    public int DmgUp;           // 레벨 상승 시 증가 데미지
    [TextArea(3, 5)]
    public string skillEffect;  // 스킬 효과 설명
    public AnimationClip animationClip;     //해당 스킬 애니메이션

    // 스킬의 정보를 출력하는 메서드
    public void PrintSkillInfo()
    {
        Debug.Log("스킬 이름: " + skillName);
        Debug.Log("최대 데미지: " + MaxDmg);
        Debug.Log("최소 데미지: " + MinDmg);
        Debug.Log("레벨 상승 시 데미지 증가: " + DmgUp);
        Debug.Log("스킬 효과: " + skillEffect);
    }
}