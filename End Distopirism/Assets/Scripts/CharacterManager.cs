using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public string CharName;
    public int maxHp;   //최대 체력
    public int hp;      //현재 체력

    public int dex;     //방어력

    public int MaxDmg;  //최소 데미지
    public int Dmg;     //최종 데미지
    public int MinDmg;  //최대 데미지
    public int DmgUp;   //코인 성공 시 1개당 데미지 증가값

    public bool Live;

    void Start()
    {
        if (Live == false)
        {
            if (hp > 0)
            {
                Live = true;
            }
        }
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
}
