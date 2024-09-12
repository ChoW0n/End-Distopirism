using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private static BattleManager BattleManager_instance;
    //외부 접근용 프로퍼티
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
    public bool isLive; //적 생존 시
    public bool EnemySelect; //적 선택 여부
    public bool PlayerSelect; //내 캐릭터 선택 여부
    public int drow = 0; //교착 횟수
    public int PlayerCheck = 0;
    public int EnemyCheck = 0;
    public bool AllTargetSelected = false; //모든 타겟을 설정했는가

    //다수의 적과 플레이어를 선택할 수 있도록 List 사용
    public List<CharacterManager> targetObjects = new List<CharacterManager>();
    public List<CharacterManager> playerObjects = new List<CharacterManager>();

    void Awake()
    {
        state = GameState.start; // 전투 시작 알림
        
    }
    public void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        PlayerCheck = players.Length;
        EnemyCheck = enemys.Length;
        BattleStart();  //게임 시작시 전투 시작
    }
    public void Update()
    {
        if (!AllTargetSelected && state == GameState.playerTurn)
        {
            SelectTarget();
            //Debug.Log("선택 실행 중");
        }
    }

    void BattleStart()
    {
        //전투 시작 시 캐릭터 등장 애니메이션 등 효과를 넣고 싶으면 여기 아래에

        //플레이어나 적에게 턴 넘기기
        state = GameState.playerTurn;
        isLive = true;
    }

    //공격&턴 종료 버튼
    public void PlayerAttackButton()
    {
        //플레이어 턴이 아닐 때 방지
        if (state != GameState.playerTurn || !PlayerSelect || !EnemySelect)
        {
            return;
        }

        StartCoroutine(PlayerAttack());
    }

    void SelectTarget()  //내 캐릭터&타겟 선택
    {
        //적의 턴이 아니고 마우스 클릭을 할 때
        if (state != GameState.enemyTurn && Input.GetMouseButtonDown(0))
        {
            //클릭된 오브젝트 가져오기
            GameObject clickObject = UIManager.Instance.MouseGetObject();

            if (clickObject != null)
            {
                if (clickObject.tag == "Enemy")
                {
                    CharacterManager selectedEnemy = clickObject.GetComponent<CharacterManager>();

                    //적이 이미 선택된 적 리스트에 있는지 확인
                    if (targetObjects.Contains(selectedEnemy))
                    {
                        //동일한 플레이어 클릭 시 선택 취소
                        targetObjects.Remove(selectedEnemy);
                        Debug.Log("적 캐릭터 선택 취소됨");
                    }
                    else
                    {
                        //새로운 적 선택
                        targetObjects.Add(selectedEnemy);
                        Debug.Log("적 캐릭터 선택됨");
                    }
                }
                //플레이어 캐릭터 선택 또는 재선택
                else if (clickObject.tag == "Player")
                {
                    CharacterManager selectedPlayer = clickObject.GetComponent<CharacterManager>();

                    //플레이어가 이미 선택된 플레이어 리스트에 있는지 확인
                    if (playerObjects.Contains(selectedPlayer))
                    {
                        //동일한 플레이어 클릭 시 선택 취소
                        playerObjects.Remove(selectedPlayer);
                        Debug.Log("플레이어 캐릭터 선택 취소됨");
                    }
                    else
                    {
                        //새로운 플레이어 선택
                        playerObjects.Add(selectedPlayer);
                        Debug.Log("플레이어 캐릭터 선택됨");
                    }
                }

                //아군과 적이 모두 선택되었는지 확인
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



    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");

        //리스트의 각 플레이어와 적이 1:1로 매칭되어 공격
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];

            //플레이어와 적의 공격력 및 피해 계산
            playerObject.Dmg = Random.Range(playerObject.MinDmg, playerObject.MaxDmg);
            targetObject.Dmg = Random.Range(targetObject.MinDmg, targetObject.MaxDmg);
            if (targetObject.Dmg > playerObject.Dmg)
            {
                playerObject.hp = playerObject.hp - (targetObject.Dmg - playerObject.dex);
                Debug.Log("합 패배: " + (targetObject.Dmg - playerObject.dex));
                drow = 0;
            }
            else if (targetObject.Dmg < playerObject.Dmg)
            {
                targetObject.hp = targetObject.hp - (playerObject.Dmg - targetObject.dex);
                Debug.Log("합 승리: " + (playerObject.Dmg - targetObject.dex));
                drow = 0;
            }
            else if (targetObject.Dmg == playerObject.Dmg)
            {

                drow = drow + 1;
                Debug.Log("합  교착: " + drow + "합");
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

        //전투 종료 여부 판단
        if (EnemyCheck == 0)
        {
            state = GameState.win;
            Debug.Log("승리");
            EndBattle();
        }
        else if (PlayerCheck == 0)
        {
            state = GameState.lose;
            Debug.Log("패배");
            EndBattle();
        }
        else
        {
            //적 공격 턴으로 전환
            state = GameState.enemyTurn;
            StartCoroutine(EnemyTurn());
        }
    }

    void EndBattle()    //전투 종료
    {
        Debug.Log("전투 종료");
    }

    IEnumerator EnemyTurn() //적 공격턴
    {
        yield return new WaitForSeconds(1f);
        //적 공격 코드
        Debug.Log("적 공격");

        //적 공격 끝났으면 플레이어에게 턴 넘기기
        state = GameState.playerTurn;
        AllTargetSelected = false;
    }
}
