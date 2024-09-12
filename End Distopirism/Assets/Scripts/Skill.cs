using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill")]
public class Skill : ScriptableObject
{
    public string skillName;    // ��ų �̸�
    public int MaxDmg;         // ��ų�� �ִ� ������
    public int MinDmg;         // ��ų�� �ּ� ������
    public int DmgUp;           //���� ���� �� ��� ���ط�

    // ��ų�� ������ ����ϴ� �޼���
    public void PrintSkillInfo()
    {
        Debug.Log("��ų �̸�: " + skillName);
        Debug.Log("�ִ� ������: " + MaxDmg);
        Debug.Log("�ּ� ������: " + MinDmg);
        Debug.Log("���� ���� �� ������ ���: " + DmgUp);
    }
}
