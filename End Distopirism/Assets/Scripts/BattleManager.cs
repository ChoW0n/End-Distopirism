using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    

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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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


    
    void CoinRoll(CharacterManager Object, ref int succesCount)// ���ŷ¿� ����Ͽ� ���� ��� ����
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            float maxMenTality = 100f; // �ִ� ���ŷ�
            float maxProbability = 0.6f; // �ִ� Ȯ�� (60%)

            // ���ŷ¿� ���� Ȯ�� ���
            float currentProbability = Mathf.Max(0f, maxProbability * (Object.MenTality / maxMenTality));

            for (int j = 0; j < Object.Coin - 1; j++)
            {
                // ���� ������: ���� Ȯ���� ���� ���� ���� ����
                if (Random.value < currentProbability)
                {
                    Object.successCount++;
                }
            }
            Object.coinbonus = succesCount * Object.DmgUp;
            Debug.Log($"{Object.CharName}�� ���� ������ ���� Ƚ��: {succesCount} / {Object.Coin} ");
            Debug.Log($"{Object.CharName}�� ���� ����: {Object.Coin} / {Object.MaxCoin}");
        }
    }

    void DiffCheck()// ���� ������ ��� ������ ���Ͽ� ���ʽ� �� �г�Ƽ ����
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];


            if (playerObject.DmgLevel > (targetObject.DefLevel + 4))
            {
                playerObject.bonusdmg = ((playerObject.DmgLevel - playerObject.DefLevel) / 4) * 1;
            }
            if (targetObject.DmgLevel > (playerObject.DefLevel + 4))
            {
                targetObject.bonusdmg = ((targetObject.DmgLevel - playerObject.DefLevel) / 4) * 1;
            }
        }
    }
    IEnumerator PlayerAttack()  //�÷��̾� ������
    {
        Attaking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("�÷��̾� ����");
        //���ݷ��� ���� ���� for �ۿ� �ִ� ������ 1ȸ�� üũ�ϱ� ����. �̹� �̸� for ����
        DiffCheck();
        //����Ʈ�� �� �÷��̾�� ���� 1:1�� ��Ī�Ǿ� ����
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];

            //�÷��̾�� ���� ���ݷ� �� ���� ���


            //���� ����
            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"�÷��̾�: {playerObject.CharName}, ��: {targetObject.CharName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);


            //���� ������
            CalculateDamage(playerObject, targetObject);


            //�� ����
            //�� �� �Ѹ��̶� ������ ���ٸ� �ٷ� ���ظ� ��
            if (!(playerObject.Coin > 0 || targetObject.Coin > 0))
            {
                ApplyDamageNoCoins(playerObject, targetObject);
            }
            else
            {
                //���� ���� ó�� ȣ��
                if (targetObject.Dmg == playerObject.Dmg)
                {
                    HandleDraw(ref i, playerObject, targetObject);
                }
                else
                {
                    //���� ó��
                    HandleBattleResult(playerObject, targetObject, ref i);
                }
            }

            //ĳ���͵� ü�� Ȯ�� �� ��� ó��
            CheckHealth(playerObject, targetObject);
        }

        

        //���� �ν� ����
        Attaking = false;
    }

    //������ ���� �Լ�
    void CalculateDamage(CharacterManager playerObject, CharacterManager targetObject)
    {
        playerObject.Dmg = Random.Range(playerObject.MaxDmg, playerObject.MinDmg) + playercoinbonus + playerbonusdmg;
        targetObject.Dmg = Random.Range(targetObject.MaxDmg, targetObject.MinDmg) + enemycoinbonus + enemybonusdmg;
        Debug.Log($"{playerObject.CharName}�� ���� ������: {playerObject.Dmg} (�⺻ ������: {playerObject.MinDmg} - {playerObject.MaxDmg}, ���� ���ʽ�: {playercoinbonus}, ���� ���ʽ�: {playerbonusdmg})");
        Debug.Log($"{targetObject.CharName}�� ���� ������: {targetObject.Dmg} (�⺻ ������: {targetObject.MinDmg} - {targetObject.MaxDmg}, ���� ���ʽ�: {enemycoinbonus}, ���� ���ʽ�: {enemybonusdmg})");

        //�۾� ������

        //�÷��̾�� ���� ��ġ
        Vector2 playerPosition = playerObject.transform.position;
        Vector2 targetPosition = targetObject.transform.position;

        //�÷��̾�� ���� �Ӹ� ���� ������ ǥ��
        UIManager.Instance.ShowDamageText(playerObject.Dmg, targetPosition + Vector2.up * 1f);
        UIManager.Instance.ShowDamageText(targetObject.Dmg, playerPosition + Vector2.up * 1f);
    }

    //���ε��� �������� �ʴٸ�
    void ApplyDamageNoCoins(CharacterManager playerObject, CharacterManager targetObject)
    {
        if (playerObject.Coin == 0)
        {
            ApplyRemainingDamage(targetObject, playerObject);
        }
        else
        {
            ApplyRemainingDamage(playerObject, targetObject);
        }
    }

    //�����ִ� ���� ��ŭ Ÿ��
    void ApplyRemainingDamage(CharacterManager attacker, CharacterManager victim)
    {
        for (int j = 0; j < attacker.Coin; j++)
        {
            int successCount = 0;
            int coinBonus = 0;
            if (j > 0)
            {
                CoinRoll(attacker, ref successCount);
                attacker.Dmg = Random.Range(attacker.MaxDmg, attacker.MinDmg) + coinBonus + attacker.bonusdmg;
            }
            victim.hp -= attacker.Dmg - victim.DefLevel;
            victim.MenTality -= 2;  //�й� �� ���ŷ� -2
            if (attacker.MenTality < 100)
            {
                attacker.MenTality += 1;    //�¸� �� ���ŷ� +1
            }
            Debug.Log($"{attacker.CharName}��(��) ���� ����: {attacker.Dmg}");
        }
        
    }

    //���� ���� �Լ�
    void HandleDraw(ref int i, CharacterManager playerObject, CharacterManager targetObject)
    {
        draw++;
        Debug.Log($"���� ���� �߻� {draw} ȸ");
        
        if (draw < 3)
        {
            i--;
        }
        if (draw >= 3)
        {
            playerObject.MenTality -= 10;
            targetObject.MenTality -= 10;
            Debug.Log($"{playerObject.CharName}�� {targetObject.CharName} �� ���ŷ� ����");
            draw = 0;
        }
    }
    
    //����
    void HandleBattleResult(CharacterManager playerObject, CharacterManager targetObject, ref int i)
    {
        CharacterManager winner;
        CharacterManager loser;
        if(playerObject.Dmg > targetObject.Dmg)
        {
            winner = playerObject;
            loser = targetObject;
        }
        else
        {
            winner = targetObject;
            loser = playerObject;
        }

        if (loser.Coin > 0)
        {
            loser.Coin--;
            i--;    //�ٽ� �ο��
        }
        else
        {
            ApplyRemainingDamage(winner, loser);
        }
    }

    //��Ʋ ������ �� �ν��� �Ʊ�&������ �� ������ ü���� 0���ϰ� �ƴٸ� �����ϱ�.
    void CheckHealth(CharacterManager playerObject, CharacterManager targetObject)
    {
        if (playerObject.hp <= 0)
        {
            PlayerCheck--;
        }
        if (targetObject.hp <= 0)
        {
            EnemyCheck--;
        }
    }

    void CheckBattleEnd()
    {
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
