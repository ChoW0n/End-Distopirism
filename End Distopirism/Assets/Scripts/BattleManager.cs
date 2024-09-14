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


    
    void CoinRoll(CharacterManager Object, ref int succesCount)// 정신력에 비례하여 코인 결과 조정
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            float maxMenTality = 100f; // 최대 정신력
            float maxProbability = 0.6f; // 최대 확률 (60%)

            // 정신력에 따른 확률 계산
            float currentProbability = Mathf.Max(0f, maxProbability * (Object.MenTality / maxMenTality));

            for (int j = 0; j < Object.Coin - 1; j++)
            {
                // 코인 던지기: 현재 확률에 따라 성공 여부 결정
                if (Random.value < currentProbability)
                {
                    Object.successCount++;
                }
            }
            Object.coinbonus = succesCount * Object.DmgUp;
            Debug.Log($"{Object.CharName}의 코인 던지기 성공 횟수: {succesCount} / {Object.Coin} ");
            Debug.Log($"{Object.CharName}의 남은 코인: {Object.Coin} / {Object.MaxCoin}");
        }
    }

    void DiffCheck()// 공격 레벨과 방어 레벨을 비교하여 보너스 및 패널티 적용
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
    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        Attaking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");
        //공격레벨 방어레벨 대조 for 밖에 있는 이유는 1회만 체크하기 위해. 이미 미리 for 돌림
        DiffCheck();
        //리스트의 각 플레이어와 적이 1:1로 매칭되어 공격
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];

            //플레이어와 적의 공격력 및 피해 계산


            //코인 리롤
            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.CharName}, 적: {targetObject.CharName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);


            //최종 데미지
            CalculateDamage(playerObject, targetObject);


            //합 진행
            //둘 중 한명이라도 코인이 없다면 바로 피해를 줌
            if (!(playerObject.Coin > 0 || targetObject.Coin > 0))
            {
                ApplyDamageNoCoins(playerObject, targetObject);
            }
            else
            {
                //교착 상태 처리 호출
                if (targetObject.Dmg == playerObject.Dmg)
                {
                    HandleDraw(ref i, playerObject, targetObject);
                }
                else
                {
                    //승패 처리
                    HandleBattleResult(playerObject, targetObject, ref i);
                }
            }

            //캐릭터들 체력 확인 후 사망 처리
            CheckHealth(playerObject, targetObject);
        }

        

        //공격 인식 종료
        Attaking = false;
    }

    //데미지 연산 함수
    void CalculateDamage(CharacterManager playerObject, CharacterManager targetObject)
    {
        playerObject.Dmg = Random.Range(playerObject.MaxDmg, playerObject.MinDmg) + playerObject.coinbonus + playerObject.bonusdmg;
        targetObject.Dmg = Random.Range(targetObject.MaxDmg, targetObject.MinDmg) + targetObject.coinbonus + targetObject.bonusdmg;
        Debug.Log($"{playerObject.CharName}의 최종 데미지: {playerObject.Dmg} (기본 데미지: {playerObject.MinDmg} - {playerObject.MaxDmg}, 코인 보너스: {playerObject.coinbonus}, 공격 보너스: {playerObject.bonusdmg})");
        Debug.Log($"{targetObject.CharName}의 최종 데미지: {targetObject.Dmg} (기본 데미지: {targetObject.MinDmg} - {targetObject.MaxDmg}, 코인 보너스: {targetObject.coinbonus}, 공격 보너스: {targetObject.bonusdmg})");
    }

    //코인들이 남아있지 않다면
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

    //남아있는 코인 만큼 타격
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
            victim.MenTality -= 2;  //패배 시 정신력 -2
            if (attacker.MenTality < 100)
            {
                attacker.MenTality += 1;    //승리 시 정신력 +1
            }
            Debug.Log($"{attacker.CharName}이(가) 가한 피해: {attacker.Dmg}");
        }
        
    }

    //교착 상태 함수
    void HandleDraw(ref int i, CharacterManager playerObject, CharacterManager targetObject)
    {
        draw++;
        Debug.Log($"교착 상태 발생 {draw} 회");
        
        if (draw < 3)
        {
            i--;
        }
        if (draw >= 3)
        {
            playerObject.MenTality -= 10;
            targetObject.MenTality -= 10;
            Debug.Log($"{playerObject.CharName}과 {targetObject.CharName} 의 정신력 감소");
            draw = 0;
        }
    }
    
    //재대결
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
            i--;    //다시 싸우기
        }
        else
        {
            ApplyRemainingDamage(winner, loser);
        }
    }

    //배틀 시작할 때 인식한 아군&적군의 총 갯수를 체력이 0이하가 됐다면 차감하기.
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
            Debug.Log("승리");
            EndBattle();
        }
        else if (PlayerCheck == 0)
        {
            state = GameState.lose;
            Debug.Log("패배");
            EndBattle();
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
