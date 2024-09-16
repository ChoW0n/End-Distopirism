using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public enum GameState
{
    start, 
    playerTurn, 
    enemyTurn, 
    win, 
    lose, 
    pause
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameState state;
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

        // 게임 시작시 전투 시작
        BattleStart();
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
        // 전투 시작 시 캐릭터 등장 애니메이션 등 효과를 넣고 싶으면 여기 아래에

        // 플레이어나 적에게 턴 넘기기
        // 추후 랜덤 가능성 있음.
        state = GameState.playerTurn;
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
        //클릭된 오브젝트 가져오기
        GameObject clickObject = UIManager.Instance.MouseGetObject();

        if (clickObject == null)
            return;

        if (clickObject.CompareTag("Enemy"))
        {
            CharacterManager selectedEnemy = clickObject.GetComponent<CharacterManager>();

            //동일한 플레이어 클릭 시 선택 취소
            if (targetObjects.Contains(selectedEnemy))
            {
                targetObjects.Remove(selectedEnemy);
                Debug.Log("적 캐릭터 선택 취소됨");
                Selecting = true;
            }

            if (Selecting)
            {
                //새로운 적 선택
                targetObjects.Add(selectedEnemy);
                Debug.Log("적 캐릭터 선택됨");
                Selecting = false;
            }
        }

        // 플레이어 캐릭터 선택 또는 재선택
        if (clickObject.CompareTag("Player"))
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
                if (index != -1 && index < targetObjects.Count)
                {
                    targetObjects.RemoveAt(index);
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

    void CoinRoll(CharacterManager Object, ref int succesCount)// 정신력에 비례하여 코인 결과 조정
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            float maxMenTality = 100f; // 최대 정신력
            float maxProbability = 0.6f; // 최대 확률 (60%)

            // 정신력에 따른 확률 계산
            float currentProbability = Mathf.Max(0f, maxProbability * (Object.GetPlayer.MenTality / maxMenTality));

            for (int j = 0; j < Object.GetPlayer.Coin - 1; j++)
            {
                // 코인 던지기: 현재 확률에 따라 성공 여부 결정
                if (Random.value < currentProbability)
                {
                    Object.successCount++;
                }
            }
            Object.coinbonus = succesCount * Object.GetPlayer.DmgUp;
            Debug.Log($"{Object.GetPlayer.CharName}의 코인 던지기 성공 횟수: {succesCount} / {Object.GetPlayer.Coin} ");
            Debug.Log($"{Object.GetPlayer.CharName}의 남은 코인: {Object.GetPlayer.Coin} / {Object.GetPlayer.MaxCoin}");
        }
    }

    void DiffCheck()// 공격 레벨과 방어 레벨을 비교하여 보너스 및 패널티 적용
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterManager playerObject = playerObjects[i];
            CharacterManager targetObject = targetObjects[i];


            if (playerObject.GetPlayer.DmgLevel > (targetObject.GetPlayer.DefLevel + 4))
            {
                playerObject.bonusdmg = ((playerObject.GetPlayer.DmgLevel - playerObject.GetPlayer.DefLevel) / 4) * 1;
            }
            if (targetObject.GetPlayer.DmgLevel > (playerObject.GetPlayer.DefLevel + 4))
            {
                targetObject.bonusdmg = ((targetObject.GetPlayer.DmgLevel - playerObject.GetPlayer.DefLevel) / 4) * 1;
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
            Debug.Log($"플레이어: {playerObject.GetPlayer.CharName}, 적: {targetObject.GetPlayer.CharName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);


            //최종 데미지
            CalculateDamage(playerObject, targetObject);


            //합 진행
            //둘 중 한명이라도 코인이 없다면 바로 피해를 줌
            if (!(playerObject.GetPlayer.Coin > 0 || targetObject.GetPlayer.Coin > 0))
            {
                ApplyDamageNoCoins(playerObject, targetObject);
            }
            else
            {
                //교착 상태 처리 호출
                if (targetObject.GetPlayer.Dmg == playerObject.GetPlayer.Dmg)
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
        playerObject.GetPlayer.Dmg = Random.Range(playerObject.GetPlayer.MaxDmg, playerObject.GetPlayer.MinDmg) + playerObject.coinbonus + playerObject.bonusdmg;
        targetObject.GetPlayer.Dmg = Random.Range(targetObject.GetPlayer.MaxDmg, targetObject.GetPlayer.MinDmg) + targetObject.coinbonus + targetObject.bonusdmg;
        Debug.Log($"{playerObject.GetPlayer.CharName}의 최종 데미지: {playerObject.GetPlayer.Dmg} (기본 데미지: {playerObject.GetPlayer.MinDmg} - {playerObject.GetPlayer.MaxDmg}, 코인 보너스: {playerObject.coinbonus}, 공격 보너스: {playerObject.bonusdmg})");
        Debug.Log($"{targetObject.GetPlayer.CharName}의 최종 데미지: {targetObject.GetPlayer.Dmg} (기본 데미지: {targetObject.GetPlayer.MinDmg} - {targetObject.GetPlayer.MaxDmg}, 코인 보너스: {targetObject.coinbonus}, 공격 보너스: {targetObject.bonusdmg})");
    
        //플레이어와 적의 위치 설정
        Vector2 playerObjectPosition = playerObject.transform.position;
        Vector2 targetObjectPosition = targetObject.transform.position;

        //더 높은 데미지만 표시
        if (playerObject.GetPlayer.Dmg > targetObject.GetPlayer.Dmg)
        {
            UIManager.Instance.ShowDamageText(playerObject.GetPlayer.Dmg, targetObjectPosition + Vector2.up * 2f);
        }
        else
        {
            UIManager.Instance.ShowDamageText(targetObject.GetPlayer.Dmg, playerObjectPosition + Vector2.up * 2f);
        }

    }

    //코인들이 남아있지 않다면
    void ApplyDamageNoCoins(CharacterManager playerObject, CharacterManager targetObject)
    {
        if (playerObject.GetPlayer.Coin == 0)
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
        for (int j = 0; j < attacker.GetPlayer.Coin; j++)
        {
            int successCount = 0;
            int coinBonus = 0;
            if (j > 0)
            {
                CoinRoll(attacker, ref successCount);
                attacker.GetPlayer.Dmg = Random.Range(attacker.GetPlayer.MaxDmg, attacker.GetPlayer.MinDmg) + coinBonus + attacker.bonusdmg;
            }
            victim.GetPlayer.hp -= attacker.GetPlayer.Dmg - victim.GetPlayer.DefLevel;
            
            if (0 >= victim.GetPlayer.hp)
            {
                victim.gameObject.SetActive(false);
            }

            victim.GetPlayer.MenTality -= 2;  //패배 시 정신력 -2
            if (attacker.GetPlayer.MenTality < 100)
            {
                attacker.GetPlayer.MenTality += 1;    //승리 시 정신력 +1
            }
            Debug.Log($"{attacker.GetPlayer.CharName}이(가) 가한 피해: {attacker.GetPlayer.Dmg}");
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
            playerObject.GetPlayer.MenTality -= 10;
            targetObject.GetPlayer.MenTality -= 10;
            Debug.Log($"{playerObject.GetPlayer.CharName}과 {targetObject.GetPlayer.CharName} 의 정신력 감소");
            draw = 0;
        }
    }
    
    //재대결
    void HandleBattleResult(CharacterManager playerObject, CharacterManager targetObject, ref int i)
    {
        CharacterManager winner;
        CharacterManager loser;
        if(playerObject.GetPlayer.Dmg > targetObject.GetPlayer.Dmg)
        {
            winner = playerObject;
            loser = targetObject;
        }
        else
        {
            winner = targetObject;
            loser = playerObject;
        }

        if (loser.GetPlayer.Coin > 0)
        {
            loser.GetPlayer.Coin--;
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
        if (playerObject.GetPlayer.hp <= 0)
        {
            PlayerCheck--;
            playerObject.OnDeath();
        }
        if (targetObject.GetPlayer.hp <= 0)
        {
            EnemyCheck--;
            targetObject.OnDeath();
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
