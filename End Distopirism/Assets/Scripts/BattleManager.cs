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
    public bool enemySelect; //적 선택 여부
    public bool playerSelect; //내 캐릭터 선택 여부
    public int draw = 0; //교착 횟수
    public int playerCheck = 0;
    public int enemyCheck = 0;
    public bool allTargetSelected = false; //모든 타겟을 설정했는가
    public bool isAttacking = false;
    public bool selecting = false;  //적을 선택해야하는 상태일 때

    //다수의 적과 플레이어를 선택할 수 있도록 List 사용
    public List<CharacterProfile> targetObjects = new List<CharacterProfile>();
    public List<CharacterProfile> playerObjects = new List<CharacterProfile>();

    public Vector3 centerPosition; // 중앙 위치를 저장할 변수

    // 승자와 패자의 이동 거리를 정의합니다.
    public float winnerMoveDistance = 1f;
    public float loserMoveDistance = -1f;

    public float moveSpeed = 5f; // 이동 속도 (단위: 초당 유닛)
    public float battleSpacing = 2f; // 전투 시 적과 플레이어 사이의 간격

    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.5f;

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
        playerCheck = players.Length;
        enemyCheck = enemys.Length;

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
        if (state != GameState.playerTurn || !allTargetSelected)
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

    void SelectTarget()  //내 캐릭터&타겟 선택
    {
        //클릭된 오브젝트 가져오기
        GameObject clickObject = UIManager.Instance.MouseGetObject();

        if (clickObject == null)
            return;

        if (clickObject.CompareTag("Enemy"))
        {
            CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();

            //동일한 플레이어 클릭 시 선택 취소
            if (targetObjects.Contains(selectedEnemy))
            {
                targetObjects.Remove(selectedEnemy);
                Debug.Log("적 캐릭터 선택 취소됨");
                selecting = true;
            }

            if (selecting)
            {
                //새로운 적 선택
                targetObjects.Add(selectedEnemy);
                Debug.Log("적 캐릭터 선택됨");
                selecting = false;

                UIManager.Instance.ShowCharacterInfo(targetObjects[0]);
            }
        }

        // 플레이어 캐릭터 선택 또는 재선택
        if (clickObject.CompareTag("Player"))
        {
            if (selecting)
            {
                Debug.Log("적을 선택해주세요.");
                return;
            }

            CharacterProfile selectedPlayer = clickObject.GetComponent<CharacterProfile>();

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
                selecting = true;

                UIManager.Instance.ShowCharacterInfo(playerObjects[0]);
            }
        }

        //아군과 적이 모두 선택되었는지 확인
        playerSelect = playerObjects.Count > 0;
        enemySelect = targetObjects.Count > 0;
        if (playerObjects.Count == playerCheck && targetObjects.Count == enemyCheck)
        {
            allTargetSelected = true;
        }
        else
        {
            allTargetSelected = false;
        }


    }

    void CoinRoll(CharacterProfile Object, ref int successCount)// 정신력에 비례하여 코인 결과 조정
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            float maxMenTality = 100f; // 최대 정신력
            float maxProbability = 0.6f; // 최대 확률 (60%)

            // 정신력에 따른 확률 계산
            float currentProbability = Mathf.Max(0f, maxProbability * (Object.GetPlayer.menTality / maxMenTality));

            for (int j = 0; j < Object.GetPlayer.coin - 1; j++)
            {
                // 코인 던지기: 현재 확률에 따라 성공 여부 결정
                if (Random.value < currentProbability)
                {
                    Object.successCount++;
                }
            }
            Object.coinBonus = Object.successCount * Object.GetPlayer.dmgUp;
            Debug.Log($"{Object.GetPlayer.charName}의 코인 던지기 성공 횟수: {Object.successCount} / {Object.GetPlayer.coin} ");
            Debug.Log($"{Object.GetPlayer.charName}의 남은 코인: {Object.GetPlayer.coin} / {Object.GetPlayer.maxCoin}");
        }
    }

    void DiffCheck()// 공격 레벨과 방어 레벨을 비교하여 보너스 및 패널티 적용
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterProfile playerObject = playerObjects[i];
            CharacterProfile targetObject = targetObjects[i];


            if (playerObject.GetPlayer.dmgLevel > (targetObject.GetPlayer.defLevel + 4))
            {
                playerObject.bonusDmg = ((playerObject.GetPlayer.dmgLevel - playerObject.GetPlayer.defLevel) / 4) * 1;
            }
            if (targetObject.GetPlayer.dmgLevel > (playerObject.GetPlayer.defLevel + 4))
            {
                targetObject.bonusDmg = ((targetObject.GetPlayer.dmgLevel - playerObject.GetPlayer.defLevel) / 4) * 1;
            }
        }
    }

    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        isAttacking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");
        DiffCheck();

        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);

        for (int i = 0; i < matchCount; i++)
        {
            if (i >= playerObjects.Count || i >= targetObjects.Count)
            {
                Debug.LogError($"인덱스 오류: i={i}, playerObjects.Count={playerObjects.Count}, targetObjects.Count={targetObjects.Count}");
                continue;
            }

            CharacterProfile playerObject = playerObjects[i];
            CharacterProfile targetObject = targetObjects[i];

            if (playerObject == null || targetObject == null)
            {
                Debug.LogError($"Null 객체 오류: playerObject={playerObject}, targetObject={targetObject}");
                continue;
            }

            Vector3 playerOriginalPosition = playerObject.transform.position;
            Vector3 targetOriginalPosition = targetObject.transform.position;

            // 중앙으로 이동
            yield return StartCoroutine(MoveToBattlePosition(playerObject, targetObject));

            // 기존의 전투 로직
            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.GetPlayer.charName}, 적: {targetObject.GetPlayer.charName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);

            CalculateDamage(playerObject, targetObject);

            yield return new WaitForSeconds(1f);

            // 전투 결과에 따라 위치 변경
            yield return StartCoroutine(MoveBattleResult(playerObject, targetObject));

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
                    yield return StartCoroutine(MoveBattleResult(playerObject, targetObject));
                }

                yield return new WaitForSeconds(1f);
            }

            CheckHealth(playerObject, targetObject);

            // 원래 위치로 돌아가기
            yield return StartCoroutine(MoveBack(playerObject, playerOriginalPosition));
            yield return StartCoroutine(MoveBack(targetObject, targetOriginalPosition));
        }

        isAttacking = false;
        CheckBattleEnd();
    }

    IEnumerator MoveToBattlePosition(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        Vector3 midpoint = (playerObject.transform.position + targetObject.transform.position) / 2;
        Vector3 playerDirection = (midpoint - playerObject.transform.position).normalized;
        Vector3 targetDirection = (midpoint - targetObject.transform.position).normalized;

        Vector3 playerDestination = midpoint - playerDirection * (battleSpacing / 2);
        Vector3 targetDestination = midpoint - targetDirection * (battleSpacing / 2);

        yield return StartCoroutine(MoveCharacter(playerObject, playerDestination));
        yield return StartCoroutine(MoveCharacter(targetObject, targetDestination));
    }

    IEnumerator MoveCharacter(CharacterProfile character, Vector3 destination)
    {
        while (character.transform.position != destination)
        {
            character.transform.position = Vector3.MoveTowards(character.transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator MoveBattleResult(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        Vector3 midpoint = (playerObject.transform.position + targetObject.transform.position) / 2;
        Vector3 playerDirection = (midpoint - playerObject.transform.position).normalized;
        Vector3 targetDirection = (midpoint - targetObject.transform.position).normalized;

        Vector3 playerDestination, targetDestination;
        CharacterProfile loser, winner;

        if (playerObject.GetPlayer.dmg > targetObject.GetPlayer.dmg)
        {
            // 플레이어가 이긴 경우
            winner = playerObject;
            loser = targetObject;
            playerDestination = playerObject.transform.position + playerDirection * winnerMoveDistance;
            targetDestination = targetObject.transform.position - targetDirection * loserMoveDistance;
        }
        else if (playerObject.GetPlayer.dmg < targetObject.GetPlayer.dmg)
        {
            // 적이 이긴 경우
            winner = targetObject;
            loser = playerObject;
            playerDestination = playerObject.transform.position - playerDirection * loserMoveDistance;
            targetDestination = targetObject.transform.position + targetDirection * winnerMoveDistance;
        }
        else
        {
            // 무승부인 경우 이동하지 않음
            yield break;
        }

        // 패자를 먼저 이동
        yield return StartCoroutine(MoveCharacter(loser, loser == playerObject ? playerDestination : targetDestination));

        // 승자를 나중에 이동
        yield return StartCoroutine(MoveCharacter(winner, winner == playerObject ? playerDestination : targetDestination));
    }

    IEnumerator MoveBack(CharacterProfile character, Vector3 originalPosition)
    {
        yield return StartCoroutine(MoveCharacter(character, originalPosition));
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
                CoinRoll(attacker, ref attacker.successCount);
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
        allTargetSelected = false;
    }
}