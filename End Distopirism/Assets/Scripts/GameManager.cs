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

    public enum GameState
    {
        start, playerTurn, enemyTurn, win, pause
    }

    public GameState state;
    public bool isLive; //�� ���� ��
    public bool EnemySelect; //�� ���� ����
    public bool PlayerSelect; //�� ĳ���� ���� ����
    public int drow = 0; //���� Ƚ��

    //�ټ��� ���� �÷��̾ ������ �� �ֵ��� List ���
    public List<CharacterManager> targetObjects = new List<CharacterManager>();
    public List<CharacterManager> playerObjects = new List<CharacterManager>();

    void Awake()
    {
        state = GameState.start; // ���� ���� �˸�
        BattleStart();
    }

    void BattleStart()
    {
        //���� ���� �� ĳ���� ���� �ִϸ��̼� �� ȿ���� �ְ� ������ ���� �Ʒ���

        //�÷��̾ ������ �� �ѱ��
        state = GameState.playerTurn;
        StartCoroutine(SelectTarget());
        isLive = true;
    }

    //����&�� ���� ��ư
    public void PlayerAttackButton()
    {
        //�÷��̾� ���� �ƴ� �� ����
        if (state != GameState.playerTurn || !PlayerSelect || !EnemySelect)
        {
            return;
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
                    if (clickObject.tag == "Enemy")
                    {
                        CharacterManager selectedEnemy = clickObject.GetComponent<CharacterManager>();

                        //���� �̹� ���õ� �� ����Ʈ�� �ִ��� Ȯ��
                        if (targetObjects.Contains(selectedEnemy))
                        {
                            //������ �÷��̾� Ŭ�� �� ���� ���
                            targetObjects.Remove(selectedEnemy);
                            Debug.Log("�� ĳ���� ���� ��ҵ�");
                        }
                        else
                        {
                            //���ο� �� ����
                            targetObjects.Add(selectedEnemy);
                            Debug.Log("�� ĳ���� ���õ�");
                        }
                    }
                    //�÷��̾� ĳ���� ���� �Ǵ� �缱��
                    else if (clickObject.tag == "Player")
                    {
                        CharacterManager selectedPlayer = clickObject.GetComponent<CharacterManager>();

                        //�÷��̾ �̹� ���õ� �÷��̾� ����Ʈ�� �ִ��� Ȯ��
                        if (playerObjects.Contains(selectedPlayer))
                        {
                            //������ �÷��̾� Ŭ�� �� ���� ���
                            playerObjects.Remove(selectedPlayer);
                            Debug.Log("�÷��̾� ĳ���� ���� ��ҵ�");
                        }
                        else
                        {
                            //���ο� �÷��̾� ����
                            playerObjects.Add(selectedPlayer);
                            Debug.Log("�÷��̾� ĳ���� ���õ�");
                        }
                    }

                    //�Ʊ��� ���� ��� ���õǾ����� Ȯ��
                    PlayerSelect = playerObjects.Count > 0;
                    EnemySelect = targetObjects.Count > 0;
                }
            }
            yield return null;
        }
    }


    IEnumerator PlayerAttack()  //�÷��̾� ������
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("�÷��̾� ����");

        //����Ʈ�� �� �÷��̾�� ���� 1:1�� ��Ī�Ǿ� ����
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];

            //�÷��̾�� ���� ���ݷ� �� ���� ���
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
        }

            //���� ���� ���� �Ǵ�
            if (!isLive)
        {
            state = GameState.win;
            EndBattle();
        }
        else
        {
            //�� ���� ������ ��ȯ
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
