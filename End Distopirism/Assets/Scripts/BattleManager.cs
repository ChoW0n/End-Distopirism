using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 게임의 현재 상태를 나타내는 열거형
public enum GameState
{
    start,      // 게임 시작
    playerTurn, // 플레이어 턴
    enemyTurn,  // 적 턴
    win,        // 승리
    lose,       // 패배
    pause       // 일시 정지
}

public class BattleManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static BattleManager Instance { get; private set; }

    // 게임 상태 및 전투 관련 변수
    public GameState state;
    public int draw = 0;        // 교착 상태 횟수
    public int playerCheck = 0; // 남은 플레이어 수
    public int enemyCheck = 0;  // 남은 적 수
    public bool isAttacking = false; // 공격 중 여부

    // 전투 위치 및 이동 관련 변수
    public Vector3 centerPosition;       // 중앙 위치
    public float winnerMoveDistance = 1f; // 승자 이동 거리
    public float loserMoveDistance = -1f; // 패자 이동 거리
    public float moveSpeed = 5f;         // 이동 속도
    public float battleSpacing = 2f;     // 전투 시 간격

    // 카메라 흔들림 효과 관련 변수
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.5f;

    // 전투 관련 매니저들
    private BattleMoveManager battleMoveManager;
    private CoinManager coinManager;
    private TargetSelector targetSelector;
    private DiffCheckManager diffCheckManager;

    // 초기화 메서드
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 게임 상태 초기화
        state = GameState.start;

        // 전투 관련 매니저 초기화
        battleMoveManager = new BattleMoveManager(moveSpeed, battleSpacing, winnerMoveDistance, loserMoveDistance);
        coinManager = new CoinManager();
        targetSelector = new TargetSelector();
        diffCheckManager = new DiffCheckManager();
    }

    // 게임 시작 시 호출되는 메서드
    public void Start()
    {
        // 플레이어와 적 수 초기화
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        playerCheck = players.Length;
        enemyCheck = enemys.Length;

        // 타겟 선택기 초기화
        targetSelector.InitializeSelector(playerCheck, enemyCheck);

        // 전투 시작
        BattleStart();

        // 중앙 위치 계산
        CalculateCenterPosition();
    }

    // 매 프레임마다 호출되는 메서드
    public void Update()
    {
        // 플레이어 턴에 마우스 클릭 시 타겟 선택
        if (state == GameState.playerTurn && Input.GetMouseButtonDown(0))
        {
            targetSelector.SelectTarget();
            Debug.Log("선택 실행 중");
        }
    }

    // 중앙 위치 계산 메서드
    void CalculateCenterPosition()
    {
        Vector3 totalPosition = Vector3.zero;
        int totalCount = 0;

        // 모든 플레이어와 적의 위치를 합산
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

        // 평균 위치 계산
        centerPosition = totalPosition / totalCount;
    }

    // 전투 시작 메서드
    void BattleStart()
    {
        // 전투 시작 시 캐릭터 등장 애니메이션 등 효과를 넣고 싶으면 여기 아래에 추가

        // 플레이어 턴으로 시작 (추후 랜덤 가능성 있음)
        state = GameState.playerTurn;
    }

    // 플레이어 공격 버튼 메서드
    public void PlayerAttackButton()
    {
        // 플레이어 턴이 아니거나 모든 타겟이 선택되지 않았을 때 방지
        if (state != GameState.playerTurn || !targetSelector.allTargetSelected)
        {
            return;
        }
        if (isAttacking)
        {
            return;
        }
        StartCoroutine(PlayerAttack());

        // 전투 시작 시 캐릭터 정보 패널 비활성화
        UIManager.Instance.playerProfilePanel.SetActive(false);
        UIManager.Instance.enemyProfilePanel.SetActive(false);
    }

    // 플레이어 공격 코루틴
    IEnumerator PlayerAttack()
    {
        isAttacking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");
        diffCheckManager.DiffCheck(targetSelector.playerObjects, targetSelector.targetObjects);

        int matchCount = Mathf.Min(targetSelector.playerObjects.Count, targetSelector.targetObjects.Count);

        // 각 플레이어와 타겟에 대해 전투 수행
        for (int i = 0; i < matchCount; i++)
        {
            // 인덱스 범위 체크
            if (i >= targetSelector.playerObjects.Count || i >= targetSelector.targetObjects.Count)
            {
                Debug.LogError($"인덱스 오류: i={i}, playerObjects.Count={targetSelector.playerObjects.Count}, targetObjects.Count={targetSelector.targetObjects.Count}");
                continue;
            }

            CharacterProfile playerObject = targetSelector.playerObjects[i];
            CharacterProfile targetObject = targetSelector.targetObjects[i];

            // null 체크
            if (playerObject == null || targetObject == null)
            {
                Debug.LogError($"Null 객체 오류: playerObject={playerObject}, targetObject={targetObject}");
                continue;
            }

            // 원래 위치 저장
            Vector3 playerOriginalPosition = playerObject.transform.position;
            Vector3 targetOriginalPosition = targetObject.transform.position;

            // 전투 위치로 이동
            yield return StartCoroutine(battleMoveManager.MoveToBattlePosition(playerObject, targetObject));

            // 전투 준비: 성공 횟수 초기화 및 코인 굴리기
            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.GetPlayer.charName}, 적: {targetObject.GetPlayer.charName}");
            coinManager.CoinRoll(playerObject);
            coinManager.CoinRoll(targetObject);

            // 데미지 계산
            CalculateDamage(playerObject, targetObject);

            yield return new WaitForSeconds(1f);

            // 전투 결과에 따라 위치 변경
            yield return StartCoroutine(battleMoveManager.MoveBattleResult(playerObject, targetObject));

            bool battleEnded = false;
            int drawCount = 0;

            // 전투 결과 처리 루프
            while (!battleEnded)
            {
                // 양쪽 모두 코인이 없는 경우
                if (playerObject.GetPlayer.coin <= 0 && targetObject.GetPlayer.coin <= 0)
                {
                    ApplyDamageNoCoins(playerObject, targetObject);
                    battleEnded = true;
                }
                // 무승부인 경우
                else if (targetObject.GetPlayer.dmg == playerObject.GetPlayer.dmg)
                {
                    drawCount++;
                    if (drawCount >= 3)
                    {
                        HandleDraw(playerObject, targetObject);
                        battleEnded = true;
                    }
                }
                // 승패가 결정된 경우
                else
                {
                    battleEnded = HandleBattleResult(playerObject, targetObject);
                }

                // 전투가 끝나지 않았다면 재대결
                if (!battleEnded)
                {
                    CalculateDamage(playerObject, targetObject);
                    yield return StartCoroutine(battleMoveManager.MoveBattleResult(playerObject, targetObject));
                }

                yield return new WaitForSeconds(1f);
            }

            // 체력 확인
            CheckHealth(playerObject, targetObject);

            // 원래 위치로 돌아가기
            yield return StartCoroutine(battleMoveManager.MoveBack(playerObject, playerOriginalPosition));
            yield return StartCoroutine(battleMoveManager.MoveBack(targetObject, targetOriginalPosition));
        }

        isAttacking = false;
        CheckBattleEnd();

        // 선택 초기화
        targetSelector.ResetSelection();
    }

    // 데미지 계산 메서드
    void CalculateDamage(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        // 플레이어와 적의 데미지 계산
        playerObject.GetPlayer.dmg = Random.Range(playerObject.GetPlayer.maxDmg, playerObject.GetPlayer.minDmg) + playerObject.coinBonus + playerObject.bonusDmg;
        targetObject.GetPlayer.dmg = Random.Range(targetObject.GetPlayer.maxDmg, targetObject.GetPlayer.minDmg) + targetObject.coinBonus + targetObject.bonusDmg;

        // 데미지 로그 출력
        Debug.Log($"{playerObject.GetPlayer.charName}의 최종 데미지: {playerObject.GetPlayer.dmg} (기본 데미지: {playerObject.GetPlayer.minDmg} - {playerObject.GetPlayer.maxDmg}, 코인 보너스: {playerObject.coinBonus}, 공격 보너스: {playerObject.bonusDmg})");
        Debug.Log($"{targetObject.GetPlayer.charName}의 최종 데미지: {targetObject.GetPlayer.dmg} (기본 데미지: {targetObject.GetPlayer.minDmg} - {targetObject.GetPlayer.maxDmg}, 코인 보너스: {targetObject.coinBonus}, 공격 보너스: {targetObject.bonusDmg})");

        // 플레이어와 적의 위치 설정
        Vector2 playerObjectPosition = playerObject.transform.position;
        Vector2 targetObjectPosition = targetObject.transform.position;

        // 더 높은 데미지만 표시
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

    // 코인이 없을 때 데미지 적용 메서드
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

    // 남은 코인으로 데미지를 적용하는 메서드
    void ApplyRemainingDamage(CharacterProfile attacker, CharacterProfile victim)
    {
        // 공격자의 남은 코인 수만큼 반복
        for (int j = 0; j < attacker.GetPlayer.coin; j++)
        {
            // 공격자의 성공 횟수와 코인 보너스 초기화
            attacker.successCount = 0;
            attacker.coinBonus = 0;
            
            // 첫 번째 공격이 아닌 경우
            if (j > 0)
            {
                // 코인 굴리기 실행
                coinManager.CoinRoll(attacker);
                // 공격자의 데미지 재계산
                attacker.GetPlayer.dmg = Random.Range(attacker.GetPlayer.maxDmg, attacker.GetPlayer.minDmg) + attacker.coinBonus + attacker.bonusDmg;
            }
            
            // 피해자의 체력에서 공격자의 데미지를 뺌 (방어력 고려)
            victim.GetPlayer.hp -= attacker.GetPlayer.dmg - victim.GetPlayer.defLevel;
            
            // 피해자의 체력이 0 이하가 되면 비활성화
            if (0 >= victim.GetPlayer.hp)
            {
                victim.gameObject.SetActive(false);
            }

            // 피해자의 정신력 감소
            victim.GetPlayer.menTality -= 2;  // 패배 시 정신력 -2
            
            // 공격자의 정신력이 100 미만일 때 증가
            if (attacker.GetPlayer.menTality < 100)
            {
                attacker.GetPlayer.menTality += 1;    // 승리 시 정신력 +1
            }
            
            // 로그에 가한 피해 출력
            Debug.Log($"{attacker.GetPlayer.charName}이(가) 가한 피해: {attacker.GetPlayer.dmg}");
            
            // 카메라 흔들기 효과 실행
            StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeIntensity));
        }
    }

    // 교착 상태 처리 메서드
    void HandleDraw(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        Debug.Log("3회 연속 무승부 발생");
        playerObject.GetPlayer.menTality -= 10;
        targetObject.GetPlayer.menTality -= 10;
        Debug.Log($"{playerObject.GetPlayer.charName}과 {targetObject.GetPlayer.charName}의 정신력 감소");
    }

    // 전투 결과 처리 메서드
    bool HandleBattleResult(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        // 데미지가 더 높은 캐릭터를 승자로 결정
        CharacterProfile winner = playerObject.GetPlayer.dmg > targetObject.GetPlayer.dmg ? playerObject : targetObject;
        // 승자가 아닌 캐릭터를 패자로 결정
        CharacterProfile loser = winner == playerObject ? targetObject : playerObject;

        // 패자의 코인이 남아있는 경우
        if (loser.GetPlayer.coin > 0)
        {
            loser.GetPlayer.coin--; // 패자의 코인을 1 감소
            loser.ShowCharacterInfo(); // 패자의 정보를 업데이트하여 표시
            return false; // 전투가 아직 끝나지 않았음을 반환
        }
        else // 패자의 코인이 없는 경우
        {
            ApplyRemainingDamage(winner, loser); // 승자가 패자에게 남은 데미지를 적용
            return true; // 전투가 끝났음을 반환
        }
    }

    // 체력 확인 메서드
    void CheckHealth(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        // 플레이어의 체력이 0 이하인 경우
        if (playerObject.GetPlayer.hp <= 0)
        {
            playerCheck--; // 남은 플레이어 수 감소
            playerObject.OnDeath(); // 플레이어 사망 처리
        }
        // 적의 체력이 0 이하인 경우
        if (targetObject.GetPlayer.hp <= 0)
        {
            enemyCheck--; // 남은 적 수 감소
            targetObject.OnDeath(); // 적 사망 처리
        }
    }

    // 전투 종료 확인 메서드
    void CheckBattleEnd()
    {
        // 모든 적이 제거된 경우
        if (enemyCheck == 0)
        {
            state = GameState.win; // 승리 상태로 변경
            Debug.Log("승리");
            EndBattle();
        }
        // 모든 플레이어가 제거된 경우
        else if (playerCheck == 0)
        {
            state = GameState.lose; // 패배 상태로 변경
            Debug.Log("패배");
            EndBattle();
        }
    }

    // 전투 종료 메서드
    void EndBattle()
    {
        Debug.Log("전투 종료");
        // 여기에 전투 종료 후 처리할 내용 추가
    }

    // 적 턴 코루틴
    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        // 적 공격 코드
        Debug.Log("적 공격");

        // 적 공격 끝났으면 플레이어에게 턴 넘기기
        state = GameState.playerTurn;
        targetSelector.allTargetSelected = false;
    }
}