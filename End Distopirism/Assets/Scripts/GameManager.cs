using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State
    {
        start, playerTurn, enemyTurn, win
    }

    public State state;
    public bool isLive; //�� ���� ��

    void Awake()
    {
        state = State.start;    //���� ���� �˸�
        BattleStart();
    }

    void BattleStart()
    {
        //���� ���۽� ĳ���� ���� �ִϸ��̼� �� ȿ���� �ְ� ������ ���� �Ʒ���

        //�÷��̾ ������ �� �ѱ��

        state = State.playerTurn;
        isLive = true;
    }

    //���� ��ư
    public void PlayerAttackButton()
    {
                //�÷��̾� ���� �ƴ� �� ����
        if(state != State.playerTurn)
        {
            return;
        }
        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("�÷��̾� ����");
        //���� ��ų, ������ �� �ڵ� �ۼ�

        if(!isLive)
        {
            state = State.win;
            EndBattle();
        }
        else//�� ���� ��
        {
            state = State.enemyTurn;
            StartCoroutine(EnemyTurn());
        }
    }

    void EndBattle()
    {
        Debug.Log("���� ����");
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        //�� ���� �ڵ�
        Debug.Log("�� ����");

        //�� ���� �������� �÷��̾�� �� �ѱ��
        state = State.playerTurn;
    }
}
