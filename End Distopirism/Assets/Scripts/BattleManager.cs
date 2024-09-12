using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public int draw = 0; //교착 횟수
    public int PlayerCheck = 0;
    public int EnemyCheck = 0;
    public bool AllTargetSelected = false; //모든 타겟을 설정했는가
    public bool Attaking = false;
    public bool Selecting = false;  //적을 선택해야하는 상태일 때

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
        if (state == GameState.playerTurn && Input.GetMouseButtonDown(0))
        {
            SelectTarget();
            Debug.Log("선택 실행 중");
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

                    if (Selecting)
                    {
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
                            Selecting = false;
                        }
                    }    
                }
                //플레이어 캐릭터 선택 또는 재선택
                else if (clickObject.tag == "Player")
                {
                    if (Selecting)
                    {
                        Debug.Log("적을 선택해주세요.");
                        return;
                    }
                    CharacterManager selectedPlayer = clickObject.GetComponent<CharacterManager>();

                    //플레이어가 이미 선택된 플레이어 리스트에 있는지 확인
                    if (playerObjects.Contains(selectedPlayer))
                    {
                        //매칭된 적 삭제
                        int index = playerObjects.IndexOf(selectedPlayer);
                        if (index != -1)
                        {
                            if (index < targetObjects.Count)
                            {
                                targetObjects.RemoveAt(index);
                            }
                            
                        }
                        //동일한 플레이어 클릭 시 선택 취소
                        playerObjects.Remove(selectedPlayer);
                        Debug.Log("플레이어 캐릭터 선택 취소됨");
                        
                    }
                    else
                    {
                        //새로운 플레이어 선택
                        playerObjects.Add(selectedPlayer);
                        Debug.Log("플레이어 캐릭터 선택됨");
                        Selecting = true;
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


    // 공격 레벨과 방어 레벨을 비교하여 보너스 및 패널티 적용

    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        Attaking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");

        //리스트의 각 플레이어와 적이 1:1로 매칭되어 공격
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];

            //플레이어와 적의 공격력 및 피해 계산
            //공격레벨 방어레벨 대조
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
            // 정신력에 비례하여 코인 결과 조정
            float maxMenTality = 100f; // 최대 정신력
            float maxProbability = 0.6f; // 최대 확률 (60%)

            // 정신력에 따른 확률 계산
            float currentProbability = Mathf.Max(0f, maxProbability * (playerObject.MenTality / maxMenTality));
            
            for (int j = 0; j < playerObject.Coin; j++)
            {
                // 코인 던지기: 현재 확률에 따라 성공 여부 결정
                if (Random.value < currentProbability)
                {
                    playersuccessCount++;
                }
            }
            playercoinbonus = playersuccessCount * playerObject.DmgUp;
            Debug.Log($"{playerObject.CharName}의 코인 던지기 성공 횟수: {playersuccessCount}");
            Debug.Log($"{playerObject.CharName}의 남은 코인: {playerObject.Coin} / {playerObject.MaxCoin}");
            for (int j = 0; j < targetObject.Coin; j++)
            {
                // 코인 던지기: 현재 확률에 따라 성공 여부 결정
                if (Random.value < currentProbability)
                {
                    enemysuccessCount++;
                }
            }
            enemycoinbonus = enemysuccessCount * targetObject.DmgUp;
            Debug.Log($"{targetObject.CharName}의 코인 던지기 성공 횟수: {enemysuccessCount}");
            Debug.Log($"{targetObject.CharName}의 남은 코인: {targetObject.Coin} / {targetObject.MaxCoin}");



            //최종 데미지
            playerObject.Dmg = Random.Range(playerObject.MaxDmg, playerObject.MinDmg)+playercoinbonus+ PlayerBonusDmg;
            targetObject.Dmg = Random.Range(targetObject.MaxDmg, targetObject.MinDmg) + enemycoinbonus + EnemyBonusDmg;
            // 디버그 로그 추가
            Debug.Log($"{playerObject.CharName}의 최종 데미지: {playerObject.Dmg} (기본 데미지: {playerObject.MinDmg} - {playerObject.MaxDmg}, 코인 보너스: {playercoinbonus}, 공격 보너스: {PlayerBonusDmg})");
            Debug.Log($"{targetObject.CharName}의 최종 데미지: {targetObject.Dmg} (기본 데미지: {targetObject.MinDmg} - {targetObject.MaxDmg}, 코인 보너스: {enemycoinbonus}, 공격 보너스: {EnemyBonusDmg})");

            //합 진행
            if (!(playerObject.Coin > 0 || targetObject.Coin > 0))  //둘 중 한명이라도 코인이 없다면 바로 피해주기
            {
                if (playerObject.Coin < 0)
                {
                    for (int j = 0; j < targetObject.Coin; j++)
                    {
                        playerObject.hp -= targetObject.Dmg - playerObject.DefLevel;
                        Debug.Log($"{targetObject.CharName}이(가) 가한 피해: {targetObject.Dmg})");
                    }
                }
                else if (targetObject.Coin < 0)
                {
                    for (int j = 0; j < playerObject.Coin; j++)
                    {
                        targetObject.hp -= playerObject.Dmg - targetObject.DefLevel;
                        Debug.Log($"{playerObject.CharName}이(가) 가한 피해: {playerObject.Dmg})");
                    }
                }
            }
            else if ((playerObject.Coin > 0 || targetObject.Coin > 0))  //둘 다 코인이 있을 때
            {
                if (targetObject.Dmg == playerObject.Dmg)   //피해가 같다면
                {
                    draw++;
                    Debug.Log($"교착 상태 발생 {draw} 회");
                    i--;
                }
           
                if (draw >= 3)  //교착이 3회 이상이라면
                {
                    targetObject.MenTality -= 10;
                    playerObject.MenTality -= 10;
                    Debug.Log($"{targetObject.CharName}과 {playerObject.CharName} 의 정신력 감소)");
                    draw = 0;
                }
            
            if (playerObject.Dmg > targetObject.Dmg)    //플레이어의 승리
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
                        Debug.Log($"{playerObject.CharName}이(가) 가한 피해: {playerObject.Dmg})");
                    }
                }
            }
            if (targetObject.Dmg > playerObject.Dmg)    //적의 승리
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
                        Debug.Log($"{targetObject.CharName}이(가) 가한 피해: {targetObject.Dmg})");
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
            playerObjects.Clear();
            targetObjects.Clear();
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