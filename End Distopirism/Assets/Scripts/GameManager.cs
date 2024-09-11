using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //CharacterManager Character = GameObject.Find("CharacterManager").GetComponent<CharacterManager>();
    public enum GameState
    {
        start, playerTurn, enemyTurn, win, pause
    }

    public GameState state;
    public bool isLive; //�� ���� ��
    public bool EnemySelect; //�� ���� ����

    void Awake()
    {
        state = GameState.start;    //���� ���� �˸�
        BattleStart();
    }

    void BattleStart()
    {
        //���� ���۽� ĳ���� ���� �ִϸ��̼� �� ȿ���� �ְ� ������ ���� �Ʒ���

        //�÷��̾ ������ �� �ѱ��

        state = GameState.playerTurn;
        isLive = true;
    }

    //���� ��ư
    public void PlayerAttackButton()
    {
                //�÷��̾� ���� �ƴ� �� ����

        if(state != GameState.playerTurn)
        {
            return;
        }
        StartCoroutine(PlayerAttack());
    }

    IEnumerator selectTarget()  //�� ĳ����&Ÿ�� ����
    {
        while(true)
        {
            if (state != GameState.enemyTurn && Input.GetMouseButtonDown(0))
            {
                //GameObject clickObject =
                
            }
        }
    }
    IEnumerator PlayerAttack()  //�÷��̾� ������
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("�÷��̾� ����");
        //���� ��ų, ������ �� �ڵ� �ۼ�

        if(!isLive)
        {
            state = GameState.win;
            EndBattle();
        }
        else//�� ���� ������ ��ȯ
        {
            state = GameState.enemyTurn;
            StartCoroutine(EnemyTurn());
        }
    }

    void EndBattle()    //���� ����
    {
        Debug.Log("���� ����");
    }

    IEnumerator EnemyTurn() //�� ������
    {
        yield return new WaitForSeconds(1f);
        //�� ���� �ڵ�
        Debug.Log("�� ����");

        //�� ���� �������� �÷��̾�� �� �ѱ��
        state = GameState.playerTurn;
    }
}
