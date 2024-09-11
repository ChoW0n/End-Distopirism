using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager GameManager_instance;
    //�ܺ� ���ٿ� ������Ƽ
    public static GameManager Instance
    {
        get
        {
            if (GameManager_instance == null)
            {
                GameManager_instance = FindObjectOfType<GameManager>();
            }
            return GameManager_instance;
        }
    }
    //CharacterManager Character = GameObject.Find("CharacterManager").GetComponent<CharacterManager>();
    public enum GameState
    {
        start, playerTurn, enemyTurn, win, pause
    }

    public GameState state;
    public bool isLive; //�� ���� ��
    public bool EnemySelect; //�� ���� ����
    public bool PlayerSelect; //�� ĳ���� ���� ����

    public CharacterManager targetObject;
    public CharacterManager playerObject;

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
        StartCoroutine(SelectTarget());
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

    IEnumerator SelectTarget()  //�� ĳ����&Ÿ�� ����
    {
        while(true)
        {
            if (state != GameState.enemyTurn && Input.GetMouseButtonDown(0))
            {
                //Ŭ���� ������Ʈ ��������
                GameObject clickObject = UIManager.Instance.MouseGetObject();
                if (clickObject != null)
                {
                    if (PlayerSelect && clickObject.tag == "Enemy")
                    {
                        targetObject = clickObject.GetComponent<CharacterManager>();
                        EnemySelect = true;
                        Debug.Log("�� ĳ���� ���õ�");

                        //�� ĳ���� ���� �Ϸ� ���� ����
                        break;
                    }
                    //�÷��̾� ĳ���� ���� (�÷��̾ ���õ��� �ʾ��� ��)
                    else if (!PlayerSelect && clickObject.tag == "Player")
                    {
                        playerObject = clickObject.GetComponent<CharacterManager>();
                        PlayerSelect = true;
                        Debug.Log("�÷��̾� ĳ���� ���õ�");
                    }
                    else if (PlayerSelect && clickObject.tag == "Player")
                    {
                        playerObject = clickObject.GetComponent<CharacterManager>();
                        EnemySelect = false;
                        Debug.Log("�÷��̾� ĳ���� �缱�õ�");
                    }
                }
            }

            yield return null;

;
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
