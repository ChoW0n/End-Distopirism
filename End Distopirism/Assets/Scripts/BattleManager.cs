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
    public int draw = 0; //교착 횟수
    public int playerCheck = 0;
    public int enemyCheck = 0;
    public bool isAttacking = false;

    public Vector3 centerPosition; // 중앙 위치를 저장할 변수

    // 승자와 패자의 이동 거리를 정의합니다.
    public float winnerMoveDistance = 1f;
    public float loserMoveDistance = -1f;

    public float moveSpeed = 5f; // 이동 속도 (단위: 초당 유닛)
    public float battleSpacing = 2f; // 전투 시 적과 플레이어 사이의 간격

    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.5f;

    private BattleMoveManager battleMoveManager;
    private CoinManager coinManager;
    private TargetSelector targetSelector;
    private DiffCheckManager diffCheckManager;
    

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

        battleMoveManager = new BattleMoveManager(moveSpeed, battleSpacing, winnerMoveDistance, loserMoveDistance);
        coinManager = new CoinManager();
        targetSelector = new TargetSelector();
        diffCheckManager = new DiffCheckManager();
    }

    public void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        playerCheck = players.Length;
        enemyCheck = enemys.Length;

        targetSelector.InitializeSelector(playerCheck, enemyCheck);

        // 게임 시작시 전투 시작
        BattleStart();

        // 중앙 위치 계산
        CalculateCenterPosition();
    }

    void CalculateCenterPosition()
    {
        Vector3 totalPosition = Vector3.zero;
        int totalCount = 0;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            totalPosition += player.transform.position;
            totalCount++;
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            totalPosition += enemy.transform.position;
            totalCount++;
        }

        centerPosition = totalPosition / totalCount;
    }

    public void Update()
    {
        if (state == GameState.playerTurn && Input.GetMouseButtonDown(0))
        {
            targetSelector.SelectTarget();
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
        if (state != GameState.playerTurn || !targetSelector.allTargetSelected)
        {
            return;
        }
        if (isAttacking)
        {
            return;
        }
        StartCoroutine(PlayerAttack());

        //전투 시작 시 캐릭터 정보 패널 비활성화
        UIManager.Instance.playerProfilePanel.SetActive(false);
        UIManager.Instance.enemyProfilePanel.SetActive(false);
    }

    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        isAttacking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");
        diffCheckManager.DiffCheck(targetSelector.playerObjects, targetSelector.targetObjects);

        int matchCount = Mathf.Min(targetSelector.playerObjects.Count, targetSelector.targetObjects.Count);

        for (int i = 0; i < matchCount; i++)
        {
            if (i >= targetSelector.playerObjects.Count || i >= targetSelector.targetObjects.Count)
            {
                Debug.LogError($"인덱스 오류: i={i}, playerObjects.Count={targetSelector.playerObjects.Count}, targetObjects.Count={targetSelector.targetObjects.Count}");
                continue;
            }

            CharacterProfile playerObject = targetSelector.playerObjects[i];
            CharacterProfile targetObject = targetSelector.targetObjects[i];

            if (playerObject == null || targetObject == null)
            {
                Debug.LogError($"Null 객체 오류: playerObject={playerObject}, targetObject={targetObject}");
                continue;
            }

            Vector3 playerOriginalPosition = playerObject.transform.position;
            Vector3 targetOriginalPosition = targetObject.transform.position;

            // 중앙으로 이동
            yield return StartCoroutine(battleMoveManager.MoveToBattlePosition(playerObject, targetObject));

            // 기존의 전투 로직
            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.GetPlayer.charName}, 적: {targetObject.GetPlayer.charName}");
            coinManager.CoinRoll(playerObject);
            coinManager.CoinRoll(targetObject);

            CalculateDamage(playerObject, targetObject);

            yield return new WaitForSeconds(1f);

            // 전투 결과에 따라 위치 변경
            yield return StartCoroutine(battleMoveManager.MoveBattleResult(playerObject, targetObject));

            bool battleEnded = false;
            int drawCount = 0;

            while (!battleEnded)
            {
                if (playerObject.GetPlayer.coin <= 0 && targetObject.GetPlayer.coin <= 0)
                {
                    ApplyDamageNoCoins(playerObject, targetObject);
                    battleEnded = true;
                }
                else if (targetObject.GetPlayer.dmg == playerObject.GetPlayer.dmg)
                {
                    drawCount++;
                    if (drawCount >= 3)
                    {
                        HandleDraw(playerObject, targetObject);
                        battleEnded = true;
                    }
                }
                else
                {
                    battleEnded = HandleBattleResult(playerObject, targetObject);
                }

                if (!battleEnded)
                {
                    // 재대결을 위해 데미지 재계산
                    CalculateDamage(playerObject, targetObject);
                    yield return StartCoroutine(battleMoveManager.MoveBattleResult(playerObject, targetObject));
                }

                yield return new WaitForSeconds(1f);
            }

            CheckHealth(playerObject, targetObject);

            // 원래 위치로 돌아가기
            yield return StartCoroutine(battleMoveManager.MoveBack(playerObject, playerOriginalPosition));
            yield return StartCoroutine(battleMoveManager.MoveBack(targetObject, targetOriginalPosition));
        }

        isAttacking = false;
        CheckBattleEnd();

        targetSelector.ResetSelection();
    }

    //데미지 연산 함수
    void CalculateDamage(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        playerObject.GetPlayer.dmg = Random.Range(playerObject.GetPlayer.maxDmg, playerObject.GetPlayer.minDmg) + playerObject.coinBonus + playerObject.bonusDmg;
        targetObject.GetPlayer.dmg = Random.Range(targetObject.GetPlayer.maxDmg, targetObject.GetPlayer.minDmg) + targetObject.coinBonus + targetObject.bonusDmg;

        Debug.Log($"{playerObject.GetPlayer.charName}의 최종 데미지: {playerObject.GetPlayer.dmg} (기본 데미지: {playerObject.GetPlayer.minDmg} - {playerObject.GetPlayer.maxDmg}, 코인 보너스: {playerObject.coinBonus}, 공격 보너스: {playerObject.bonusDmg})");
        Debug.Log($"{targetObject.GetPlayer.charName}의 최종 데미지: {targetObject.GetPlayer.dmg} (기본 데미지: {targetObject.GetPlayer.minDmg} - {targetObject.GetPlayer.maxDmg}, 코인 보너스: {targetObject.coinBonus}, 공격 보너스: {targetObject.bonusDmg})");

        //플레이어와 적의 위치 설정
        Vector2 playerObjectPosition = playerObject.transform.position;
        Vector2 targetObjectPosition = targetObject.transform.position;

        //더 높은 데미지만 표시
        if (playerObject.GetPlayer.dmg > targetObject.GetPlayer.dmg)
        {
            UIManager.Instance.ShowDamageText(playerObject.GetPlayer.dmg, targetObjectPosition + Vector2.up * 250f);
            StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeIntensity));
        }
        else
        {
            UIManager.Instance.ShowDamageText(targetObject.GetPlayer.dmg, playerObjectPosition + Vector2.up * 250f);
            StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeIntensity));
        }
        
    }

    //코인들이 남아있지 않다면
    void ApplyDamageNoCoins(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        if (playerObject.GetPlayer.coin == 0)
        {
            ApplyRemainingDamage(targetObject, playerObject);
        }
        else
        {
            ApplyRemainingDamage(playerObject, targetObject);
        }
    }

    //남아있는 코인 만큼 타격
    void ApplyRemainingDamage(CharacterProfile attacker, CharacterProfile victim)
    {
        for (int j = 0; j < attacker.GetPlayer.coin; j++)
        {
            attacker.successCount = 0;
            attacker.coinBonus = 0;
            if (j > 0)
            {
                coinManager.CoinRoll(attacker);
                attacker.GetPlayer.dmg = Random.Range(attacker.GetPlayer.maxDmg, attacker.GetPlayer.minDmg) + attacker.coinBonus + attacker.bonusDmg;
            }
            victim.GetPlayer.hp -= attacker.GetPlayer.dmg - victim.GetPlayer.defLevel;
            
            if (0 >= victim.GetPlayer.hp)
            {
                victim.gameObject.SetActive(false);
            }

            victim.GetPlayer.menTality -= 2;  //패배 시 정신력 -2
            if (attacker.GetPlayer.menTality < 100)
            {
                attacker.GetPlayer.menTality += 1;    //승리 시 정신력 +1
            }
            Debug.Log($"{attacker.GetPlayer.charName}이(가) 가한 피해: {attacker.GetPlayer.dmg}");
            StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeIntensity));
        }
        
    }

    //교착 상태 함수
    void HandleDraw(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        Debug.Log("3회 연속 무승부 발생");
        playerObject.GetPlayer.menTality -= 10;
        targetObject.GetPlayer.menTality -= 10;
        Debug.Log($"{playerObject.GetPlayer.charName}과 {targetObject.GetPlayer.charName}의 정신력 감소");
    }
    
    //재대결
    bool HandleBattleResult(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        CharacterProfile winner = playerObject.GetPlayer.dmg > targetObject.GetPlayer.dmg ? playerObject : targetObject;
        CharacterProfile loser = winner == playerObject ? targetObject : playerObject;

        if (loser.GetPlayer.coin > 0)
        {
            loser.GetPlayer.coin--;
            loser.ShowCharacterInfo();
            return false; // 전투가 끝나지 않음
        }
        else
        {
            ApplyRemainingDamage(winner, loser);
            return true; // 전투가 끝남
        }
    }

    //배틀 시작할 때 인식한 아군&적군의 총 갯수를 체력이 0이하가 됐다면 차감하기.
    void CheckHealth(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        if (playerObject.GetPlayer.hp <= 0)
        {
            playerCheck--;
            playerObject.OnDeath();
        }
        if (targetObject.GetPlayer.hp <= 0)
        {
            enemyCheck--;
            targetObject.OnDeath();
        }
    }

    void CheckBattleEnd()
    {
        if (enemyCheck == 0)
        {
            state = GameState.win;
            Debug.Log("승리");
            EndBattle();
        }
        else if (playerCheck == 0)
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
        targetSelector.allTargetSelected = false;
    }
}