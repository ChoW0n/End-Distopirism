using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public string CharName;
    public int maxHp;   // �ִ� ü��
    public int hp;      // ���� ü��

    public int DefLevel;     // ��� ����

    public int DmgLevel;  // �ּ� ������
    public int Dmg;     // ���� ������
    public int MaxDmg;
    public int MinDmg;  // �ִ� ������
    public int DmgUp;   // ���� ���� �� 1���� ������ ������
    public int MaxCoin; //�ִ� ���η�
    public int Coin; //���� ���� �� ���� �� �ν��� ���� �� �й� �� 1���� ����
    public int MenTality = 100; //���ŷ�

    public bool Live;

    // ��ϵ� ��ų ������
    public Skill skill;  // Unity �ν����Ϳ��� ��ų ScriptableObject�� ����� �� �ֵ��� ��

    void Start()
    {
        Coin = MaxCoin;
        if (Live == false)
        {
            if (hp > 0)
            {
                Live = true;
            }
        }

        // ��ų�� �����Ͽ� ĳ������ ������ ���� �����մϴ�.
        ApplySkill();
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

    // ��ų�� ĳ���Ϳ� �����ϴ� �޼���
    void ApplySkill()
    {
        if (skill != null)
        {
            MaxDmg = skill.MaxDmg;
            MinDmg = skill.MinDmg;
            DmgUp = skill.DmgUp;
            // �ʿ��� ��� �߰��� ó���� �ڵ�
            Debug.Log("��ų�� ĳ���Ϳ� ����Ǿ����ϴ�: " + skill.skillName);
        }
    }
}
