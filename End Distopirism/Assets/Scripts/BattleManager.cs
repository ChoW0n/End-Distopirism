using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private static BattleManager BattleManager_instance;
    //�ܺ� ���ٿ� ������Ƽ
    public static BattleManager Instance
    {
        get
        {
            if (BattleManager_instance == null)
            {
                BattleManager_instance = FindObjectOfType<BattleManager>();
            }
            return BattleManager_instance;
        }
    }

    public enum GameState
    {
        start, 
        playerTurn, 
        enemyTurn, 
        win, 
        lose, 
        pause
    }

    public GameState state;
    public bool isLive; //�� ���� ��
    public bool EnemySelect; //�� ���� ����
    public bool PlayerSelect; //�� ĳ���� ���� ����
    public int drow = 0; //���� Ƚ��
    public int PlayerCheck = 0;
    public int EnemyCheck = 0;
    public bool AllTargetSelected = false; //��� Ÿ���� �����ߴ°�

    //�ټ��� ���� �÷��̾ ������ �� �ֵ��� List ���
    public List<CharacterManager> targetObjects = new List<CharacterManager>();
    public List<CharacterManager> playerObjects = new List<CharacterManager>();

    void Awake()
    {
        state = GameState.start; // ���� ���� �˸�
        
    }
    public void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        PlayerCheck = players.Length;
        EnemyCheck = enemys.Length;
        BattleStart();  //���� ���۽� ���� ����
    }
    public void Update()
    {
        if (!AllTargetSelected && state == GameState.playerTurn)
        {
            SelectTarget();
            //Debug.Log("���� ���� ��");
        }
    }

    void BattleStart()
    {
        //���� ���� �� ĳ���� ���� �ִϸ��̼� �� ȿ���� �ְ� ������ ���� �Ʒ���

        //�÷��̾ ������ �� �ѱ��
        state = GameState.playerTurn;
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

    void SelectTarget()  //�� ĳ����&Ÿ�� ����
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
                if (playerObjects.Count == PlayerCheck && targetObjects.Count == EnemyCheck)
                {
                    AllTargetSelected = true;
                }
                else
                {
                    AllTargetSelected = false;
                }
                
            }
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
            if (0 > playerObject.hp)
            {
                PlayerCheck--;
            }
            if (0 > targetObject.hp)
            {
                EnemyCheck--;
            }
        }

        //���� ���� ���� �Ǵ�
        if (EnemyCheck == 0)
        {
            state = GameState.win;
            Debug.Log("�¸�");
            EndBattle();
        }
        else if (PlayerCheck == 0)
        {
            state = GameState.lose;
            Debug.Log("�й�");
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
        AllTargetSelected = false;
    }
}
