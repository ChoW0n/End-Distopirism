using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Timeline.Actions;
using UnityEngine;
using System.Linq;
using DG.Tweening;

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

    public TargetArrowCreator arrowCreator; // 추가된 변��

    public Transform[] playerBattlePositions; // 플레이어 전투 위치
    public Transform[] enemyBattlePositions;  // 적 전투 위치

    private float shakeDuration = 0.3f;  // 진동 지속 시간
    private float shakeIntensity = 4f; // 진동 강도

    private bool skillSelected = false;

    [Header("스테이지 설정")]
    [SerializeField] private StageData stageData;

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
        // StageData가 할당되지 않았을 경우 Resources 폴더에서 로드
        if (stageData == null)
        {
            stageData = Resources.Load<StageData>($"StageData/Stage{DeckData.currentStage}");
            if (stageData == null)
            {
                Debug.LogError($"Stage{DeckData.currentStage}의 StageData를 찾을 수 없습니다. Resources/StageData 폴더에 StageData 에셋이 있는지 확인해주세요.");
                return;
            }
        }

        // 선택된 캐릭터들이 있는지 확인
        if (DeckData.selectedCharacterPrefabs == null || DeckData.selectedCharacterPrefabs.Count == 0)
        {
            Debug.LogError("선택된 캐릭터가 없습니다.");
            return;
        }

        // 캐릭터 위치가 설정되어 있는지 확인
        if (stageData.characterPositions == null || stageData.characterPositions.Length == 0)
        {
            Debug.LogError("StageData에 캐릭터 위치가 설정되어 있지 않습니다.");
            return;
        }

        // 선택된 플레이어 캐릭터들 생성
        SpawnSelectedCharacters();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        playerCheck = players.Length;
        enemyCheck = enemys.Length;

        Debug.Log($"생성된 플레이어 수: {players.Length}, 적 수: {enemys.Length}");

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
        // 전투 시작 시 캐릭터 등장 애니메이션
        StartCoroutine(CharacterEntryAnimation());
        
        // UIManager에 전투 시작 알림
        UIManager.Instance.SetBattleUI(true);
    }

    private IEnumerator CharacterEntryAnimation()
    {
        // 플레이어와 적 캐릭터들 찾기
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Vector3 targetScale = new Vector3(100f, 100f, 100f); // 목표 스케일을 100으로 설정

        // 모든 캐릭터를 처음부터 활성화하고 크기만 0으로 설정
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
            // 스프라이트 렌더러의 알파값을 1로 설정하여 완전히 보이게 함
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
            player.transform.localScale = Vector3.zero;
        }

        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(true);
            // 스프라이트 렌더러의 알파값을 1로 설정하여 완전히 보이게 함
            SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
            enemy.transform.localScale = Vector3.zero;
        }

        // 플레이어 캐릭터들 등장
        float delay = 0.2f;
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            player.transform
                .DOScale(targetScale, 0.5f) // Vector3.one 대신 targetScale 사용
                .SetDelay(delay * i)
                .SetEase(Ease.OutBack)
                .OnStart(() => Debug.Log($"플레이어 캐릭터 {i} 등장 시작"))
                .OnComplete(() => Debug.Log($"플레이어 캐릭터 {i} 등장 완료"));
        }

        // 플레이어 캐릭터 등장이 끝나고 잠시 대기
        yield return new WaitForSeconds(players.Length * delay + 0.3f);

        // 적 캐릭터들 등장
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemy = enemies[i];
            enemy.transform
                .DOScale(targetScale, 0.5f) // Vector3.one 대신 targetScale 사용
                .SetDelay(delay * i)
                .SetEase(Ease.OutBack)
                .OnStart(() => Debug.Log($"적 캐릭터 {i} 등장 시작"))
                .OnComplete(() => Debug.Log($"적 캐릭터 {i} 등장 완료"));
        }

        // 모든 애니메이션이 끝나고 잠시 대기
        yield return new WaitForSeconds(enemies.Length * delay + 0.5f);

        // 투 시작
        state = GameState.playerTurn;
    }

    //공격&턴 종료 버튼
    public void PlayerAttackButton()
    {
        if (state != GameState.playerTurn || !allTargetSelected || isAttacking)
        {
            return;
        }

        // 공격 시작 시 화살표 제거
        arrowCreator.ClearConnections();

        // 모든 캐릭터의 스킬 카드를 숨김
        foreach (var player in playerObjects)
        {
            player.HideSkillCards();
        }

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

    void SelectTarget()
    {
        if (!skillSelected && !selecting)
        {
            // 플레이어 선택 로직
            GameObject clickObject = UIManager.Instance.MouseGetObject();
            if (clickObject != null && clickObject.CompareTag("Player"))
            {
                CharacterProfile selectedPlayer = clickObject.GetComponent<CharacterProfile>();
                if (!playerObjects.Contains(selectedPlayer))
                {
                    playerObjects.Add(selectedPlayer);
                    selectedPlayer.isSelected = true;
                    selectedPlayer.ShowCharacterInfo();
                }
            }
        }
        else if (skillSelected && selecting)
        {
            // 적 선택 로직
            GameObject clickObject = UIManager.Instance.MouseGetObject();
            if (clickObject != null && clickObject.CompareTag("Enemy"))
            {
                CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();
                if (!targetObjects.Contains(selectedEnemy))
                {
                    targetObjects.Add(selectedEnemy);
                    selectedEnemy.isSelected = true;
                    selecting = false;
                    skillSelected = false;
                    
                    // 화살표 연결
                    if (playerObjects.Count > 0)
                    {
                        arrowCreator.AddConnection(playerObjects[playerObjects.Count - 1].transform, selectedEnemy.transform);
                    }
                }
            }
        }

        // 선택 상태 업데이트
        playerSelect = playerObjects.Count > 0;
        enemySelect = targetObjects.Count > 0;
        allTargetSelected = (playerObjects.Count == playerCheck && targetObjects.Count == enemyCheck);
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
        for (int i = 1; i < matchCount; i++)
        {
            float maxMenTality = 100f; // 최대 정신력
            float maxProbability = 0.6f; // 최대 확률 (60%)

            // 정신력에 른 확률 계산
            float currentProbability = Mathf.Max(0f, maxProbability * (Object.GetPlayer.menTality / maxMenTality));

            for (int j = 1; j < Object.GetPlayer.coin - 1; j++)
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

            //카메라를 공격자에게 줌 인
            CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.ZoomInOnTarget(playerObject.transform);
            }

            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.GetPlayer.charName}, 적: {targetObject.GetPlayer.charName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);

            CalculateDamage(playerObject, targetObject, out int playerDamage, out int targetDamage);

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
            // 플레이어 턴 시작 시 기존 선택 리스트 초기화
            playerObjects.Clear();
            targetObjects.Clear();
            StartCoroutine(EnemyTurn());
        }
    }

    //데미지 연산 함수
    void CalculateDamage(CharacterProfile playerObject, CharacterProfile targetObject, out int playerDamage, out int targetDamage)
    {
        playerDamage = (int)(Random.Range(playerObject.GetPlayer.maxDmg, playerObject.GetPlayer.minDmg) + playerObject.coinBonus + playerObject.bonusDmg);
        targetDamage = (int)(Random.Range(targetObject.GetPlayer.maxDmg, targetObject.GetPlayer.minDmg) + targetObject.coinBonus + targetObject.bonusDmg);

        playerObject.GetPlayer.dmg = playerDamage; // 플레이어의 데미지 저장
        targetObject.GetPlayer.dmg = targetDamage; // 적의 데미지 저장

        Debug.Log($"{playerObject.GetPlayer.charName}의 최종 데미지: {playerDamage}");
        Debug.Log($"{targetObject.GetPlayer.charName}의 최종 데미지: {targetDamage}");

        // 승리/패배 문구 표시
        Vector3 playerObjectPosition = playerObject.transform.position;
        Vector3 targetObjectPosition = targetObject.transform.position;

        if (playerDamage > targetDamage)
        {
            UIManager.Instance.ShowBattleResultText("승리", playerObjectPosition + Vector3.up * 250f);
            UIManager.Instance.ShowBattleResultText("패배", targetObjectPosition + Vector3.up * 250f);
        }
        else
        {
            UIManager.Instance.ShowBattleResultText("패배", playerObjectPosition + Vector3.up * 250f);
            UIManager.Instance.ShowBattleResultText("승리", targetObjectPosition + Vector3.up * 250f);
        }
    }

    //코인들이 남아있지 않다면
    //void ApplyDamageNoCoins(CharacterProfile playerObject, CharacterProfile targetObject)
    //{
      //  CharacterProfile attacker = playerObject.GetPlayer.coin > 0 ? playerObject : targetObject;
        //CharacterProfile victim = attacker == playerObject ? targetObject : playerObject;

        //StartCoroutine(ApplyDamageAndMoveCoroutine(attacker, victim));
    //}

IEnumerator ApplyDamageAndMoveCoroutine(CharacterProfile attacker, CharacterProfile victim)
{
    BattleMove attackerMove = attacker.GetComponent<BattleMove>();
    BattleMove victimMove = victim.GetComponent<BattleMove>();

    if (attackerMove != null)
    {
        attackerMove.Attack(); // Hit 애니메이션 재생
        // 초기 스킬 이펙트 표시
        attacker.ShowSkillEffect(attacker.GetPlayer.dmg);
    }

    for (int j = 0; j < attacker.GetPlayer.coin; j++)
    {
        attacker.successCount = 0;
        attacker.coinBonus = 0;
        if (j > 0)
        {
            CoinRoll(attacker, ref attacker.successCount);
            attacker.GetPlayer.dmg = Random.Range(attacker.GetPlayer.maxDmg, attacker.GetPlayer.minDmg) + attacker.coinBonus + attacker.bonusDmg;
            // 데미지가 변경될 때마다 스킬 이펙트 업데이트
            attacker.UpdateSkillEffectDamage(attacker.GetPlayer.dmg);
        }

        // 피해를 적용하고 데미지 텍스트 표시
        victim.GetPlayer.hp -= attacker.GetPlayer.dmg - victim.GetPlayer.defLevel;
        victim.UpdateStatus(); // 피해를 입은 후 상태바 업데이트
        UIManager.Instance.ShowDamageTextNearCharacter(attacker.GetPlayer.dmg, victim.transform);
        Debug.Log($"{attacker.GetPlayer.charName}이(가) 가한 피해: {attacker.GetPlayer.dmg}");

        StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeIntensity));

        // 공격자 전진, 피해자 후퇴
        if (attackerMove != null)
        {
            attackerMove.Advance(); // Attack 애니메이션 재생
        }
        if (victimMove != null)
        {
            victimMove.Retreat();
        }

        // 전진과 후퇴가 끝날 때까지 대기
        yield return StartCoroutine(WaitForMovement(attackerMove, victimMove));

        // ���해자가 사망했는지 확인
        if (0 >= victim.GetPlayer.hp)
        {
            victim.gameObject.SetActive(false);
            break; // 피해자가 사망하면 루프 종료
        }

        // 잠시 대기하여 움직임을 볼 수 있게 함
        yield return new WaitForSeconds(1f); // 0.5초의 딜레이 추가
    }

    // 정신력 감소
    victim.GetPlayer.menTality -= 2;  // 패배 시 정신력 -2
    victim.UpdateStatus(); // 정신력 변경 후 상태바 업데이트
    
    if (attacker.GetPlayer.menTality < 100)
    {
        attacker.GetPlayer.menTality += 1;    // 승리 시 정신력 +1
        attacker.UpdateStatus(); // 정신력 변경 후 상태바 업데이트
    }

    // 캐릭터들의 움직임이 끝난 후 대기
    yield return StartCoroutine(WaitForMovement(attackerMove, victimMove));

    // 1초 대기
    yield return new WaitForSeconds(1f);

    StartCoroutine(ReturnCharacterToInitialPosition(attacker));
    StartCoroutine(ReturnCharacterToInitialPosition(victim));

    // 전투 종료 시 스킬 이펙트 제거
    attacker.DestroySkillEffect();
    victim.DestroySkillEffect();

    //카메라를 초기 위치와 사이즈로 되돌리기
    CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
    if (cameraFollow != null)
    {
        cameraFollow.ResetCamera();
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
            playerObject.UpdateStatus(); // 정신력 변경 후 상태바 업데이트
            targetObject.UpdateStatus(); // 정신력 변경 후 상태바 업데이트
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
        
        // 모든 캐릭터의 스킬 이펙트 제거
        foreach (var player in playerObjects)
        {
            player.DestroySkillEffect();
        }
        foreach (var enemy in targetObjects)
        {
            enemy.DestroySkillEffect();
        }
        
        // UIManager에 전투 종료 알림
        UIManager.Instance.SetBattleUI(false);
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
        // 각 전투 쌍마다 위치 오브젝트의 X값을 랜덤으로 설정
        for (int i = 0; i < Mathf.Min(playerBattlePositions.Length, enemyBattlePositions.Length); i++)
        {
            float randomOffsetX = Random.Range(-300f, 300f);

            // 위치 오브젝트의 X값 수정
            Vector3 playerPos = playerBattlePositions[i].position;
            Vector3 enemyPos = enemyBattlePositions[i].position;
            
            // 플레이어 위치 설정
            playerBattlePositions[i].position = new Vector3(playerPos.x + randomOffsetX, playerPos.y, playerPos.z);
            
            // 적 위치는 플레이어보다 200 오른쪽에 설정
            enemyBattlePositions[i].position = new Vector3(playerBattlePositions[i].position.x + 200f, enemyPos.y, enemyPos.z);

            // 플레이어 이동
            if (i < playerObjects.Count)
            {
                BattleMove playerMove = playerObjects[i].GetComponent<BattleMove>();
                if (playerMove != null)
                {

                    playerMove.MoveToPosition(playerBattlePositions[i].position);
                }
            }

            // 적 이동
            if (i < targetObjects.Count)
            {
                BattleMove enemyMove = targetObjects[i].GetComponent<BattleMove>();
                if (enemyMove != null)
                {

                    enemyMove.MoveToPosition(enemyBattlePositions[i].position);
                }
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
        
        // 든 데미지 적용과 움직임이 끝난 후에 체력 확인 및 전투 종료 처리
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

    public void OnSkillSelected()
    {
        skillSelected = true;
        selecting = true; // 적 선택 모드 활성화
        Debug.Log("스킬 선택 완료, 적 선택 모드로 전환");
    }

    private void SpawnSelectedCharacters()
    {
        Debug.Log($"캐릭터 생성 시작: {DeckData.selectedCharacterPrefabs.Count}개의 캐릭터");
        
        for (int i = 0; i < DeckData.selectedCharacterPrefabs.Count; i++)
        {
            if (i < stageData.characterPositions.Length)
            {
                Vector3 spawnPosition = stageData.characterPositions[i];
                GameObject characterPrefab = DeckData.selectedCharacterPrefabs[i];
                
                Debug.Log($"캐릭터 {i + 1} 생성 위치: {spawnPosition}");
                
                GameObject character = Instantiate(
                    characterPrefab,
                    spawnPosition,
                    Quaternion.identity
                );
                
                // 태그 설정
                character.tag = "Player";
                
                // 캐릭터 초기화 로직
                CharacterProfile profile = character.GetComponent<CharacterProfile>();
                if (profile != null)
                {
                    profile.GetPlayer.Init();
                    Debug.Log($"캐릭터 {profile.GetPlayer.charName} 생성 완료");
                }
            }
            else
            {
                Debug.LogWarning($"캐릭터 {i + 1}의 생성 위치가 정의되지 않았습니다.");
            }
        }
    }
}