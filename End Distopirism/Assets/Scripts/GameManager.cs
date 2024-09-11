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

    public int drow = 0;    //�� ���� Ƚ��

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

    //����&�� ���� ��ư
    public void PlayerAttackButton()
    {

        //�÷��̾� ���� �ƴ� �� ����

        if (state != GameState.playerTurn)
        {
            return;
        }
        if (!PlayerSelect)
        {
            return;
        }
        if (!EnemySelect)
        {
            return ;
        }

        StartCoroutine(PlayerAttack());
        
    }

    IEnumerator SelectTarget()  //�� ĳ����&Ÿ�� ����
    {
        while (true)
        {
            //���� ���� �ƴϰ� ���콺 Ŭ���� �� ��
            if (state != GameState.enemyTurn && Input.GetMouseButtonDown(0))
            {
                //Ŭ���� ������Ʈ ��������
                GameObject clickObject = UIManager.Instance.MouseGetObject();

                if (clickObject != null)
                {
                    //�� ĳ���� ���� �Ǵ� �缱��(�÷��̾ ���� ���� ��)
                    if (PlayerSelect && clickObject.tag == "Enemy")
                    {
                        CharacterManager selectedEnemy = clickObject.GetComponent<CharacterManager>();

                        //�� ĳ���Ͱ� �̹� ���õ� ���� �������� Ȯ��
                        if (targetObject == selectedEnemy)
                        {
                            //������ �� Ŭ�� �� ���� ���
                            targetObject = null;
                            EnemySelect = false;
                            Debug.Log("�� ĳ���� ���� ��ҵ�");
                        }
                        else
                        {
                            //���ο� �� ����
                            targetObject = selectedEnemy;
                            EnemySelect = true;
                            Debug.Log("�� ĳ���� ���õ�");
                        }
                    }
                    //�÷��̾� ĳ���� ���� �Ǵ� �缱��
                    else if (!PlayerSelect && clickObject.tag == "Player")
                    {
                        playerObject = clickObject.GetComponent<CharacterManager>();
                        PlayerSelect = true;
                        Debug.Log("�÷��̾� ĳ���� ���õ�");
                    }
                    //�÷��̾� ĳ���� �缱��
                    else if (PlayerSelect && clickObject.tag == "Player")
                    {
                        CharacterManager selectedPlayer = clickObject.GetComponent<CharacterManager>();

                        //�÷��̾ �̹� ���õ� �÷��̾�� �������� Ȯ��
                        if (playerObject == selectedPlayer)
                        {
                            //������ �÷��̾� Ŭ�� �� ���� ���
                            playerObject = null;
                            PlayerSelect = false;
                            Debug.Log("�÷��̾� ĳ���� ���� ��ҵ�");
                        }
                        else
                        {
                            //���ο� �÷��̾� ����
                            playerObject = selectedPlayer;
                            EnemySelect = false;    //�� ������ ���
                            Debug.Log("�÷��̾� ĳ���� �缱�õ�");
                        }
                    }
                }
                
            }
            yield return null;
        }

    }
    IEnumerator PlayerAttack()  //�÷��̾� ������
    {
        yield return new WaitForSeconds(1f);
        
        Debug.Log("�÷��̾� ����");
        //���� ��ų, ������ �� �ڵ� �ۼ�
        playerObject.Dmg = Random.Range(playerObject.MinDmg, playerObject.MaxDmg);
        targetObject.Dmg = Random.Range(targetObject.MinDmg, targetObject.MaxDmg);
        if (targetObject.Dmg > playerObject.Dmg)
        {
            playerObject.hp = playerObject.hp - (targetObject.Dmg - playerObject.dex);
            Debug.Log("�� �й�: " + (targetObject.Dmg - playerObject.dex));
            drow = 0;
        }
        else if (targetObject.Dmg < playerObject.Dmg)
        {
            targetObject.hp = targetObject.hp - (playerObject.Dmg - targetObject.dex);
            Debug.Log("�� �¸�: " + (playerObject.Dmg - targetObject.dex));
            drow = 0;
        }
        else if (targetObject.Dmg == playerObject.Dmg)
        {
            
            drow = drow + 1;
            Debug.Log("��  ����: " + drow + "��");
            StopCoroutine(PlayerAttack());
            StartCoroutine(PlayerAttack());
            
        }
        if (!isLive)
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
