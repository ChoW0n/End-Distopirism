using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill")]
public class Skill : ScriptableObject
{
    public string skillName;    // 스킬 이름
    public int MaxDmg;         // 스킬의 최대 데미지
    public int MinDmg;         // 스킬의 최소 데미지
    public int DmgUp;           //코인 성공 시 상승 피해량

    // 스킬의 정보를 출력하는 메서드
    public void PrintSkillInfo()
    {
        Debug.Log("스킬 이름: " + skillName);
        Debug.Log("최대 데미지: " + MaxDmg);
        Debug.Log("최소 데미지: " + MinDmg);
        Debug.Log("코인 성공 시 데미지 상승: " + DmgUp);
    }
}
