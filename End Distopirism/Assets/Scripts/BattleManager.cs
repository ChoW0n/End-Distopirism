using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using System.Linq;

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

    public TargetArrowCreator arrowCreator; // 추가된 변수

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

        // TargetArrowCreator 초기화
        arrowCreator = gameObject.AddComponent<TargetArrowCreator>();
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
        
        // 공격 시작 시 화살표 제거
        arrowCreator.ClearConnections();
        
        // 캐릭터들을 중앙으로 이동시킴
        MoveCombatants();
        StartCoroutine(WaitForMovementAndAttack());

        //전투 시작 시 캐릭터 정보 패널 비활성화
        UIManager.Instance.playerProfilePanel.SetActive(false);
        UIManager.Instance.enemyProfilePanel.SetActive(false);
    }

    private IEnumerator WaitForMovementAndAttack()
    {
        yield return new WaitUntil(() => AllCombatantsStoppedMoving());
        StartCoroutine(PlayerAttack());
    }

    private bool AllCombatantsStoppedMoving()
    {
        foreach (CharacterProfile player in playerObjects)
        {
            BattleMove playerMove = player.GetComponent<BattleMove>();
            if (playerMove != null && playerMove.IsMoving())
            {
                return false;
            }
        }

        foreach (CharacterProfile enemy in targetObjects)
        {
            BattleMove enemyMove = enemy.GetComponent<BattleMove>();
            if (enemyMove != null && enemyMove.IsMoving())
            {
                return false;
            }
        }

        return true;
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

            if (targetObjects.Contains(selectedEnemy))
            {
                targetObjects.Remove(selectedEnemy);
                Debug.Log("적 캐릭터 선택 취소됨");
                selecting = true;
                arrowCreator.ClearConnections();
                RedrawAllConnections();
            }

            if (selecting && playerObjects.Count > 0)
            {
                targetObjects.Add(selectedEnemy);
                Debug.Log("적 캐릭터 선택됨");
                selecting = false;

                UIManager.Instance.ShowCharacterInfo(targetObjects[0]);

                // 새로운 연결 추가 (색상 매개변수 제거)
                arrowCreator.AddConnection(playerObjects[playerObjects.Count - 1].transform, selectedEnemy.transform);
            }
        }

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
                Debug.Log("레이어 캐릭터 선택 취소됨");
            }
            else
            {
                //새로운 플레이어 선택
                playerObjects.Add(selectedPlayer);
                Debug.Log("플레이어 캐릭터 선택됨");
                selecting = true;

                UIManager.Instance.ShowCharacterInfo(playerObjects[0]);
            }

            RedrawAllConnections();
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

    private void RedrawAllConnections()
    {
        arrowCreator.ClearConnections();
        for (int i = 0; i < Mathf.Min(playerObjects.Count, targetObjects.Count); i++)
        {
            arrowCreator.AddConnection(playerObjects[i].transform, targetObjects[i].transform);
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
            CharacterProfile playerObject = playerObjects[i];
            CharacterProfile targetObject = targetObjects[i];

            CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.ZoomInOnTarget(playerObject.transform);
            }

            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.GetPlayer.charName}, 적: {targetObject.GetPlayer.charName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);

            CalculateDamage(playerObject, targetObject);

            yield return new WaitForSeconds(1f);

            if (!(playerObject.GetPlayer.coin > 0 || targetObject.GetPlayer.coin > 0))
            {
                yield return StartCoroutine(ApplyDamageAndMoveCoroutine(playerObject, targetObject));
            }
            else
            {
                if (targetObject.GetPlayer.dmg == playerObject.GetPlayer.dmg)
                {
                    HandleDraw(ref i, playerObject, targetObject);
                }
                else
                {
                    HandleBattleResult(playerObject, targetObject, ref i);
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        isAttacking = false;

        // 모든 전투가 끝난 후에 캐릭터들을 원래 위치로 돌려보내는 부분을 주석 처리
        // ReturnCombatantsToInitialPositions();

        yield return new WaitUntil(() => AllCombatantsStoppedMoving());

        // 턴 종료 처리
        CheckBattleEnd();
        if (state == GameState.playerTurn)
        {
            state = GameState.enemyTurn;
            StartCoroutine(EnemyTurn());
        }
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
            //TODO:합 승리를 나타내는 텍스트가 패배한 쪽에 뜨게끔 변경
            //UIManager.Instance.ShowDamageText("playerObject.GetPlayer.dmg", targetObjectPosition + Vector2.up * 250f);
        }
        else
        {
            //TODO:합 패배를 나타내는 텍스트가 패배한 쪽에 뜨게끔 변경
            //UIManager.Instance.ShowDamageText(targetObject.GetPlayer.dmg, playerObjectPosition + Vector2.up * 250f);
        }
        
    }

    //코인들이 남아있지 않다면
    void ApplyDamageNoCoins(CharacterProfile playerObject, CharacterProfile targetObject)
    {
        CharacterProfile attacker = playerObject.GetPlayer.coin > 0 ? playerObject : targetObject;
        CharacterProfile victim = attacker == playerObject ? targetObject : playerObject;

        StartCoroutine(ApplyDamageAndMoveCoroutine(attacker, victim));
    }

    IEnumerator ApplyDamageAndMoveCoroutine(CharacterProfile attacker, CharacterProfile victim)
    {
        BattleMove attackerMove = attacker.GetComponent<BattleMove>();
        BattleMove victimMove = victim.GetComponent<BattleMove>();

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

            // 공격자 전진, 피해자 후퇴
            if (attackerMove != null)
            {
                attackerMove.Advance();
            }
            if (victimMove != null)
            {
                victimMove.Retreat();
            }

            // 캐릭터들의 움직임이 끝날 때까지 대기
            yield return StartCoroutine(WaitForMovement(attackerMove, victimMove));

            // 잠시 대기하여 움직임을 볼 수 있게 함
            yield return new WaitForSeconds(0.8f);
        }
        StartCoroutine(ReturnCharacterToInitialPosition(attacker));
        StartCoroutine(ReturnCharacterToInitialPosition(victim));

        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        // 모든 전투가 끝난 후에 카메라를 초기 위치로 돌려보냄
        if (cameraFollow != null)
        {
            cameraFollow.ResetCamera();
            cameraFollow.target = null;
        }
    }

    IEnumerator WaitForMovement(params BattleMove[] moves)
    {
        while (moves.Any(move => move != null && move.IsMoving()))
        {
            yield return null;
        }
    }

    //교착 상태 함수
    void HandleDraw(ref int i, CharacterProfile playerObject, CharacterProfile targetObject)
    {
        draw++;
        Debug.Log($"교착 상태 발생 {draw} 회");
        
        if (draw < 3)
        {
            i--;
        }
        if (draw >= 3)
        {
            playerObject.GetPlayer.menTality -= 10;
            targetObject.GetPlayer.menTality -= 10;
            Debug.Log($"{playerObject.GetPlayer.charName}과 {targetObject.GetPlayer.charName} 의 정신력 감소");
            draw = 0;
            StartCoroutine(ReturnCharacterToInitialPosition(playerObject));
            StartCoroutine(ReturnCharacterToInitialPosition(targetObject));
        }
    }
    
    //재대결
    void HandleBattleResult(CharacterProfile playerObject, CharacterProfile targetObject, ref int i)
    {
        CharacterProfile winner;
        CharacterProfile loser;
        if(playerObject.GetPlayer.dmg > targetObject.GetPlayer.dmg)
        {
            winner = playerObject;
            loser = targetObject;
        }
        else
        {
            winner = targetObject;
            loser = playerObject;
        }
        
        if (loser.GetPlayer.coin > 0)
        {
            loser.GetPlayer.coin--;
            i--;    //다시 싸우기
            loser.ShowCharacterInfo();
        }
        else
        {
            StartCoroutine(ApplyRemainingDamageCoroutine(winner, loser));
        }
        MoveCharacters(winner, loser);
    }

    //배틀 시작할 때 인식한 아군&적군의 총 갯수를 체력이 0이 됐다면 차감하기.
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

    // BattleManager 클래스 내부에 추가

    private void MoveCombatants()
    {
        Vector3 centerPoint = CalculateCenterPoint();

        foreach (CharacterProfile player in playerObjects)
        {
            BattleMove playerMove = player.GetComponent<BattleMove>();
            if (playerMove != null)
            {
                playerMove.MoveTowardsCenter(centerPoint, true);  // true는 플레이어 측임을 나타냄
            }
        }

        foreach (CharacterProfile enemy in targetObjects)
        {
            BattleMove enemyMove = enemy.GetComponent<BattleMove>();
            if (enemyMove != null)
            {
                enemyMove.MoveTowardsCenter(centerPoint, false);  // false는 적 측임을 나타냄
            }
        }
    }

    private Vector3 CalculateCenterPoint()
    {
        Vector3 playerCenter = Vector3.zero;
        Vector3 enemyCenter = Vector3.zero;

        foreach (CharacterProfile player in playerObjects)
        {
            playerCenter += player.transform.position;
        }
        playerCenter /= playerObjects.Count;

        foreach (CharacterProfile enemy in targetObjects)
        {
            enemyCenter += enemy.transform.position;
        }
        enemyCenter /= targetObjects.Count;

        return (playerCenter + enemyCenter) / 2f;
    }

    void MoveCharacters(CharacterProfile advancer, CharacterProfile retreater)
    {
        BattleMove advancerMove = advancer.GetComponent<BattleMove>();
        BattleMove retreaterMove = retreater.GetComponent<BattleMove>();

        if (advancerMove != null)
        {
            advancerMove.Advance();
        }

        if (retreaterMove != null)
        {
            retreaterMove.Retreat();
        }

        StartCoroutine(WaitForMovement(advancerMove, retreaterMove));
    }

    // ApplyRemainingDamage 메서드를 코루틴으로 변경
    IEnumerator ApplyRemainingDamageCoroutine(CharacterProfile attacker, CharacterProfile victim)
    {
        yield return StartCoroutine(ApplyDamageAndMoveCoroutine(attacker, victim));
        
        // ��든 데미지 적용과 움직임이 끝난 후에 체력 확인 및 전투 종료 처리
        CheckHealth(attacker, victim);
        CheckBattleEnd();
    }

    public IEnumerator ReturnCharacterToInitialPosition(CharacterProfile character)
    {
        BattleMove characterMove = character.GetComponent<BattleMove>();
        if (characterMove != null)
        {
            characterMove.ReturnToInitialPosition();
            yield return new WaitUntil(() => !characterMove.IsMoving());
        }
    }
}