using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public string CharName;
    public int maxHp;   //�ִ� ü��
    public int hp;      //���� ü��

    public int dex;     //����

    public int MaxDmg;  //�ּ� ������
    public int Dmg;     //���� ������
    public int MinDmg;  //�ִ� ������
    public int DmgUp;   //���� ���� �� 1���� ������ ������

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
