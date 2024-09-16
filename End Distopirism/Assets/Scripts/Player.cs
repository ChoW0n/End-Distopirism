using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Player
{
    public string CharName;

    // 채력
    public int maxHp;

    [HideInInspector]
    public int hp;

    // 방어 레벨
    private int defLevel;
    public int DefLevel => defLevel;

    // 공격
    public int DmgLevel;

    [HideInInspector]
    public int Dmg;

    [HideInInspector]
    public int MaxDmg;

    [HideInInspector]
    public int MinDmg;

    [HideInInspector]
    public int DmgUp;   // 코인 성공 시 1개당 데미지 증가값

    // 코인
    public int MaxCoin; //최대 코인량
    public int Coin;    //동일 피해 합 시작 시 인식할 코인 합 패배 시 1개씩 차감

    // 정신력
    public float MenTality = 100f;

    // 스킬 리스트 추가
    // Unity 인스펙터에서 스킬 ScriptableObject를 등록할 수 있도록 함
    public List<Skill> skills;


    public void Init()
    {
        hp = maxHp;
    }
}