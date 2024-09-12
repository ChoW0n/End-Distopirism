using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public int draw = 0; //���� Ƚ��
    public int PlayerCheck = 0;
    public int EnemyCheck = 0;
    public bool AllTargetSelected = false; //��� Ÿ���� �����ߴ°�
    public bool Attaking = false;
    public bool Selecting = false;  //���� �����ؾ��ϴ� ������ ��

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
        if (state == GameState.playerTurn && Input.GetMouseButtonDown(0))
        {
            SelectTarget();
            Debug.Log("���� ���� ��");
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
        if (state != GameState.playerTurn || !AllTargetSelected)
        {
            return;
        }
        if (Attaking)
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

                    if (Selecting)
                    {
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
                            Selecting = false;
                        }
                    }    
                }
                //�÷��̾� ĳ���� ���� �Ǵ� �缱��
                else if (clickObject.tag == "Player")
                {
                    if (Selecting)
                    {
                        Debug.Log("���� �������ּ���.");
                        return;
                    }
                    CharacterManager selectedPlayer = clickObject.GetComponent<CharacterManager>();

                    //�÷��̾ �̹� ���õ� �÷��̾� ����Ʈ�� �ִ��� Ȯ��
                    if (playerObjects.Contains(selectedPlayer))
                    {
                        //��Ī�� �� ����
                        int index = playerObjects.IndexOf(selectedPlayer);
                        if (index != -1)
                        {
                            if (index < targetObjects.Count)
                            {
                                targetObjects.RemoveAt(index);
                            }
                            
                        }
                        //������ �÷��̾� Ŭ�� �� ���� ���
                        playerObjects.Remove(selectedPlayer);
                        Debug.Log("�÷��̾� ĳ���� ���� ��ҵ�");
                        
                    }
                    else
                    {
                        //���ο� �÷��̾� ����
                        playerObjects.Add(selectedPlayer);
                        Debug.Log("�÷��̾� ĳ���� ���õ�");
                        Selecting = true;
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


    // ���� ������ ��� ������ ���Ͽ� ���ʽ� �� �г�Ƽ ����

    IEnumerator PlayerAttack()  //�÷��̾� ������
    {
        Attaking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("�÷��̾� ����");

        //����Ʈ�� �� �÷��̾�� ���� 1:1�� ��Ī�Ǿ� ����
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];

            //�÷��̾�� ���� ���ݷ� �� ���� ���
            //���ݷ��� ���� ����
            int PlayerBonusDmg = 0;
            int EnemyBonusDmg = 0;
            
            int playercoinbonus = 0;
            int playersuccessCount = 0;

            int enemycoinbonus = 0;
            int enemysuccessCount = 0;

            if (playerObject.DmgLevel > (targetObject.DefLevel + 4))
            {
                PlayerBonusDmg = ((playerObject.DmgLevel-playerObject.DefLevel)/4)*1;
            }
            if (targetObject.DmgLevel > (playerObject.DefLevel + 4))
            {
                EnemyBonusDmg = ((targetObject.DmgLevel - playerObject.DefLevel) / 4) * 1;
            }
            // ���ŷ¿� ����Ͽ� ���� ��� ����
            float maxMenTality = 100f; // �ִ� ���ŷ�
            float maxProbability = 0.6f; // �ִ� Ȯ�� (60%)

            // ���ŷ¿� ���� Ȯ�� ���
            float currentProbability = Mathf.Max(0f, maxProbability * (playerObject.MenTality / maxMenTality));
            
            for (int j = 0; j < playerObject.Coin; j++)
            {
                // ���� ������: ���� Ȯ���� ���� ���� ���� ����
                if (Random.value < currentProbability)
                {
                    playersuccessCount++;
                }
            }
            playercoinbonus = playersuccessCount * playerObject.DmgUp;
            Debug.Log($"{playerObject.CharName}�� ���� ������ ���� Ƚ��: {playersuccessCount}");
            Debug.Log($"{playerObject.CharName}�� ���� ����: {playerObject.Coin} / {playerObject.MaxCoin}");
            for (int j = 0; j < targetObject.Coin; j++)
            {
                // ���� ������: ���� Ȯ���� ���� ���� ���� ����
                if (Random.value < currentProbability)
                {
                    enemysuccessCount++;
                }
            }
            enemycoinbonus = enemysuccessCount * targetObject.DmgUp;
            Debug.Log($"{targetObject.CharName}�� ���� ������ ���� Ƚ��: {enemysuccessCount}");
            Debug.Log($"{targetObject.CharName}�� ���� ����: {targetObject.Coin} / {targetObject.MaxCoin}");



            //���� ������
            playerObject.Dmg = Random.Range(playerObject.MaxDmg, playerObject.MinDmg)+playercoinbonus+ PlayerBonusDmg;
            targetObject.Dmg = Random.Range(targetObject.MaxDmg, targetObject.MinDmg) + enemycoinbonus + EnemyBonusDmg;
            // ����� �α� �߰�
            Debug.Log($"{playerObject.CharName}�� ���� ������: {playerObject.Dmg} (�⺻ ������: {playerObject.MinDmg} - {playerObject.MaxDmg}, ���� ���ʽ�: {playercoinbonus}, ���� ���ʽ�: {PlayerBonusDmg})");
            Debug.Log($"{targetObject.CharName}�� ���� ������: {targetObject.Dmg} (�⺻ ������: {targetObject.MinDmg} - {targetObject.MaxDmg}, ���� ���ʽ�: {enemycoinbonus}, ���� ���ʽ�: {EnemyBonusDmg})");

            //�� ����
            if (!(playerObject.Coin > 0 || targetObject.Coin > 0))  //�� �� �Ѹ��̶� ������ ���ٸ� �ٷ� �����ֱ�
            {
                if (playerObject.Coin < 0)
                {
                    for (int j = 0; j < targetObject.Coin; j++)
                    {
                        playerObject.hp -= targetObject.Dmg - playerObject.DefLevel;
                        Debug.Log($"{targetObject.CharName}��(��) ���� ����: {targetObject.Dmg})");
                    }
                }
                else if (targetObject.Coin < 0)
                {
                    for (int j = 0; j < playerObject.Coin; j++)
                    {
                        targetObject.hp -= playerObject.Dmg - targetObject.DefLevel;
                        Debug.Log($"{playerObject.CharName}��(��) ���� ����: {playerObject.Dmg})");
                    }
                }
            }
            else if ((playerObject.Coin > 0 || targetObject.Coin > 0))  //�� �� ������ ���� ��
            {
                if (targetObject.Dmg == playerObject.Dmg)   //���ذ� ���ٸ�
                {
                    draw++;
                    Debug.Log($"���� ���� �߻� {draw} ȸ");
                    i--;
                }
           
                if (draw >= 3)  //������ 3ȸ �̻��̶��
                {
                    targetObject.MenTality -= 10;
                    playerObject.MenTality -= 10;
                    Debug.Log($"{targetObject.CharName}�� {playerObject.CharName} �� ���ŷ� ����)");
                    draw = 0;
                }
            
            if (playerObject.Dmg > targetObject.Dmg)    //�÷��̾��� �¸�
            {
                if (targetObject.Coin > 0)
                {
                    targetObject.Coin--;
                    i--;
                }
                else
                {
                    for (int j = 0; j < playerObject.Coin; j++)
                    {
                        targetObject.hp -= playerObject.Dmg - targetObject.DefLevel;
                        Debug.Log($"{playerObject.CharName}��(��) ���� ����: {playerObject.Dmg})");
                    }
                }
            }
            if (targetObject.Dmg > playerObject.Dmg)    //���� �¸�
            {
                if (playerObject.Coin > 0)
                {
                    playerObject.Coin--;
                    i--;
                }
                else
                {
                    for (int j = 0; j < targetObject.Coin; j++)
                    {
                        playerObject.hp -= targetObject.Dmg - playerObject.DefLevel;
                        Debug.Log($"{targetObject.CharName}��(��) ���� ����: {targetObject.Dmg})");
                    }
                }
            }
                
            }
            if (0 > playerObject.hp)
            {
                PlayerCheck--;
            }
            if (0 > targetObject.hp)
            {
                EnemyCheck--;
            }
            Attaking = false;
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
            playerObjects.Clear();
            targetObjects.Clear();
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