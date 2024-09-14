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
    public float MenTality = 100f; //���ŷ�

    public int bonusdmg = 0;    //diff ���̿� ���� ������ ������

    public int coinbonus = 0; //���� ���ʽ�
    public int successCount = 0;  //���� Ƚ��

    public bool Live;

    // ��ų ����Ʈ �߰�
    public List<Skill> skills;  // Unity �ν����Ϳ��� ��ų ScriptableObject�� ����� �� �ֵ��� ��

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
        if (skills != null && skills.Count > 0)
        {
            // ��ų�� �����Ͽ� ĳ������ ������ ���� �����ϴ� ����
            Skill skill = skills[0]; // ���÷� ù ��° ��ų�� ����
            MaxDmg = skill.MaxDmg;
            MinDmg = skill.MinDmg;
            DmgUp = skill.DmgUp;
            // �ʿ��� ��� �߰��� ó���� �ڵ�
            Debug.Log("��ų�� ĳ���Ϳ� ����Ǿ����ϴ�: " + skill.skillName);
        }
    }
}
