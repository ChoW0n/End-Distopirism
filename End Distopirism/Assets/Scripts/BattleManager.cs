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

    public TargetArrowCreator arrowCreator; // 추가된 변

    public Transform[] playerBattlePositions; // 플레이어 전투 위치
    public Transform[] enemyBattlePositions;  // 적 전투 위치

    private float shakeDuration = 0.3f;  // 진동 지속 시간
    private float shakeIntensity = 4f; // 진동 강도

    private bool skillSelected = false;

    [Header("스테이지 설정")]
    [SerializeField] private StageData stageData;

    void Awake()
    {
        DOTween.Init(false, true, LogBehaviour.Verbose);
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
        DOTween.Init(false, true, LogBehaviour.Verbose);
        
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

        Debug.LogWarning($"생성된 플레이어 수: {players.Length}, 적 수: {enemys.Length}");

        // 적 정보 패널 비활성화
        if (UIManager.Instance != null && UIManager.Instance.enemyProfilePanel != null)
        {
            UIManager.Instance.enemyProfilePanel.SetActive(false);
        }

        // 게임 시작시 전투 시작
        BattleStart();

        // TargetArrowCreator 초기화
        arrowCreator = gameObject.AddComponent<TargetArrowCreator>();

        // 적 캐릭터들의 초기 스킬 설정
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            CharacterProfile profile = enemy.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                profile.InitializeEnemySkill();
            }
        }
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
        // UIManager가 없으면 생성되도록 수정
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager를 찾을 수 없습니다.");
            return;
        }
        
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
            Debug.Log($"전투 버튼 비활성화 상태 - state: {state}, allTargetSelected: {allTargetSelected}, isAttacking: {isAttacking}");
            return;
        }

        // 공격 시작 시 화살표 제거
        arrowCreator.ClearConnections();

        // 모든 캐릭터의 스킬 카드를 숨김
        foreach (var player in playerObjects)
        {
            player.HideSkillCards();
        }

        // 캐릭터들을  시킴
        MoveCombatants();
        StartCoroutine(WaitForMovementAndAttack());

        //전투 시작 시 캐릭터 정보 패널 비활성화
        UIManager.Instance.playerProfilePanel.SetActive(false);
        UIManager.Instance.enemyProfilePanel.SetActive(false);

        UIManager.Instance.TurnCount();
    }

    private IEnumerator WaitForMovementAndAttack()
    {
        yield return new WaitUntil(() => AllCombatantsStoppedMoving());
        StartCoroutine(PlayerAttack());
    }

    private bool AllCombatantsStoppedMoving()
    {
        foreach (CharacterProfile player in playerObjects.ToList())
        {
            if (player == null) continue;
            BattleMove playerMove = player.GetComponent<BattleMove>();
            if (playerMove != null && playerMove.IsMoving())
            {
                return false;
            }
        }

        foreach (CharacterProfile enemy in targetObjects.ToList())
        {
            if (enemy == null) continue;
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
                
                // 이미 다른 캐릭터가 선택되어 있고, 스킬-적 선택이 완료되지 않은 경우
                if (playerObjects.Count > 0 && targetObjects.Count == 0)
                {
                    // 이전 선택된 캐릭터의 선택 해제
                    foreach (var player in playerObjects)
                    {
                        player.isSelected = false;
                    }
                    playerObjects.Clear();
                    
                    // 화살표 연결 제거
                    arrowCreator.ClearConnections();
                }

                // 새로운 캐릭터 선택
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
            // 적 선택 로
            GameObject clickObject = UIManager.Instance.MouseGetObject();
            if (clickObject != null && clickObject.CompareTag("Enemy"))
            {
                CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();
                if (!targetObjects.Contains(selectedEnemy))
                {
                    targetObjects.Add(selectedEnemy);
                    selectedEnemy.isSelected = true;
                    selectedEnemy.ShowCharacterInfo();
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
        
        // allTargetSelected 조건 정
        int availableEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        allTargetSelected = (playerObjects.Count == playerCheck || playerObjects.Count == availableEnemies);
    }

    private void RedrawAllConnections()
    {
        arrowCreator.ClearConnections();
        for (int i = 0; i < Mathf.Min(playerObjects.Count, targetObjects.Count); i++)
        {
            arrowCreator.AddConnection(playerObjects[i].transform, targetObjects[i].transform);
        }
    }

    void CoinRoll(CharacterProfile Object, ref int successCount)
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 1; i < matchCount; i++)
        {
            float maxMenTality = 100f;
            float maxProbability = 0.6f;

            // 정신력 계산 시 혼란 효과 특별 처리
            float effectiveMentality = Object.GetPlayer.menTality;
            bool isConfused = Object.GetPlayer.statusEffects.Exists(e => e.effectName == "혼란");
            
            if (isConfused)
            {
                effectiveMentality -= 20f;
                Debug.LogWarning($"[혼란 효과] {Object.GetPlayer.charName}의 코인 굴림 시 정신력 {Object.GetPlayer.menTality} -> {effectiveMentality}");
            }

            float currentProbability = Mathf.Max(0f, maxProbability * (effectiveMentality / maxMenTality));
            Debug.LogWarning($"[코인 확률] {Object.GetPlayer.charName}의 현재 성공 확률: {currentProbability * 100:F1}%");

            Object.successCount = 0; // 코인 성공 횟수 초기화
            for (int j = 1; j < Object.GetPlayer.coin - 1; j++)
            {
                if (Random.value < currentProbability)
                {
                    Object.successCount++;
                    // 코인 성공 시마다 UI 업데이트
                    Object.UpdateCoinCount(Object.successCount);
                    
                    // "화염 공격" 스킬의 코인 성공 보너스
                    if (Object.GetPlayer.skills[0].skillName == "화염 공격")
                    {
                        Object.GetPlayer.dmgUp += 2;
                        Debug.LogWarning($"{Object.GetPlayer.charName}의 화염 공격으로 공격력 2 증가");
                    }
                }
            }
            Object.coinBonus = Object.successCount * Object.GetPlayer.dmgUp;
            Debug.LogWarning($"{Object.GetPlayer.charName}의 코인 던지기 성공 횟수: {Object.successCount} / {Object.GetPlayer.coin}");
            Debug.LogWarning($"{Object.GetPlayer.charName}의 남은 코인: {Object.GetPlayer.coin} / {Object.GetPlayer.maxCoin}");
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

        // 출혈 효과 적용
        foreach (var player in playerObjects.Where(p => p != null))
        {
            foreach (var effect in player.GetPlayer.statusEffects.ToList())
            {
                if (effect.effectName == "출혈")
                {
                    int bleedDamage = Mathf.RoundToInt(player.GetPlayer.maxHp * effect.healthPercentDamage);
                    player.GetPlayer.hp -= bleedDamage;
                    player.UpdateStatus();
                    UIManager.Instance.ShowDamageTextNearCharacter(bleedDamage, player.transform);
                    UIManager.Instance.CreateBloodEffect(player.transform.position);
                    Debug.LogWarning($"[출혈 피해] {player.GetPlayer.charName}이(가) 출혈로 {bleedDamage}의 피해를 입음");
                }
                effect.duration--;
                Debug.LogWarning($"[상태이상 지속시간] {player.GetPlayer.charName}의 {effect.effectName} 효과 남은 지속시간: {effect.duration}턴");
            }
            player.CheckStatusEffects();
        }

        Debug.Log("플레이어 격");
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

        // 모든 전투 끝난 후에 캐릭터들을 원래 위치로 돌려보내는 부분을 주석 처리
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

        playerObject.GetPlayer.dmg = playerDamage;
        targetObject.GetPlayer.dmg = targetDamage;

        Debug.Log($"{playerObject.GetPlayer.charName}의 최종 데미지: {playerDamage}");
        Debug.Log($"{targetObject.GetPlayer.charName}의 최종 데미지: {targetDamage}");

        Vector3 playerObjectPosition = playerObject.transform.position;
        Vector3 targetObjectPosition = targetObject.transform.position;

        if (playerDamage > targetDamage)
        {
            UIManager.Instance.ShowBattleResultText("승리", playerObjectPosition + Vector3.up * 250f);
            UIManager.Instance.ShowBattleResultText("패배", targetObjectPosition + Vector3.up * 250f);
            playerObject.PlayHitSound(); // 합 승리 시 히트 사드
        }
        else
        {
            UIManager.Instance.ShowBattleResultText("패배", playerObjectPosition + Vector3.up * 250f);
            UIManager.Instance.ShowBattleResultText("승리", targetObjectPosition + Vector3.up * 250f);
            targetObject.PlayHitSound(); // 합 승리 시 히트 사운드
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
            attacker.UpdateSkillEffectDamage(attacker.GetPlayer.dmg);
            // 코인 성공 횟수 UI 업데이트
            attacker.UpdateCoinCount(attacker.successCount);
        }

        // 공격자 전진, 피해자 후퇴 시작
        if (attackerMove != null)
        {
            attacker.PlayDashSound(); // 대시 사운드 먼저 재생
            yield return new WaitForSeconds(0.1f); // 약간의 딜레이
            attackerMove.Advance();
        }
        if (victimMove != null)
        {
            victim.PlayDashSound(); // 대시 사운드 먼저 생
            yield return new WaitForSeconds(0.1f); // 약간의 딜레이
            victimMove.Retreat();
        }

        // 전진과 후퇴가 끝날 때까지 기
        yield return StartCoroutine(WaitForMovement(attackerMove, victimMove));

        // 피해를 적용하기 전에 스킬 효과 확인
        if (attacker.GetPlayer.skills[0].skillName == "무모한 일격" && attacker.GetPlayer.dmg > victim.GetPlayer.dmg)
        {
            // 공격자의 체력 감소
            attacker.GetPlayer.hp -= 10;
            attacker.UpdateStatus();
            UIManager.Instance.ShowDamageTextNearCharacter(10, attacker.transform);
            Debug.LogWarning($"{attacker.GetPlayer.charName}이(가) 무모한 일격으로 10의 자해 피해");

            // 코인 성공 횟수에 따른 추가 피해
            int additionalDamage = attacker.successCount * 2;
            attacker.GetPlayer.dmg += additionalDamage;
            Debug.LogWarning($"{attacker.GetPlayer.charName}의 무모한 일격 추가 피해: {additionalDamage}");
        }

        // "강력한 한 방" 스킬 처리
        if (attacker.GetPlayer.skills[0].skillName == "강력한 한 방")
        {
            if (attacker.GetPlayer.dmg > victim.GetPlayer.dmg) // 합 승리 시
            {
                // 공격자의 피해량 8 증가
                attacker.GetPlayer.dmg += 8;
                Debug.LogWarning($"{attacker.GetPlayer.charName}의 강력한 한 방으로 피해량 8 증가");
                attacker.UpdateSkillEffectDamage(attacker.GetPlayer.dmg);
            }
            else // 합 패배 시
            {
                // 상대의 피해량 12 증가
                victim.GetPlayer.dmg += 12;
                Debug.LogWarning($"{attacker.GetPlayer.charName}의 강력한 한 방 실패로 {victim.GetPlayer.charName}의 피해량 12 증가");
                victim.UpdateSkillEffectDamage(victim.GetPlayer.dmg);
            }
        }

        // "독바르기" 스킬 처리
        if (attacker.GetPlayer.skills[0].skillName == "독바르기")
        {
            if (attacker.GetPlayer.dmg > victim.GetPlayer.dmg) // 합 승리 시
            {
                victim.GetPlayer.AddStatusEffect("독");
                Debug.LogWarning($"{victim.GetPlayer.charName}에게 독 효과가 부여되었습니다.");
                // 독 효과 시각화 (예: 초록색 파티클 등)
                UIManager.Instance.CreatePoisonEffect(victim.transform.position);
            }
        }

        // 피해량 계산 시 독 효과와 방어력 감소 효과 적용
        float finalDamage = attacker.GetPlayer.dmg;
        float originalDamage = finalDamage;

        // 공격자의 독 효과로 인한 피해량 감소
        foreach (var effect in attacker.GetPlayer.statusEffects)
        {
            if (effect.effectName == "독")
            {
                float beforePoison = finalDamage;
                finalDamage *= (1f + effect.outgoingDamageModifier);
                float poisonReduction = beforePoison - finalDamage;
                Debug.LogWarning($"[독 효과 - 공격자] {attacker.GetPlayer.charName}의 독으로 인한 피해량 감소: {poisonReduction:F1} ({effect.outgoingDamageModifier * 100}%)");
            }
        }

        // 피해자의 독 효과로 인한 피해량 증가
        foreach (var effect in victim.GetPlayer.statusEffects)
        {
            if (effect.effectName == "독")
            {
                float beforePoison = finalDamage;
                finalDamage *= (1f + effect.incomingDamageModifier);
                float poisonIncrease = finalDamage - beforePoison;
                Debug.LogWarning($"[독 효과 - 피해자] {victim.GetPlayer.charName}가 받는 추가 피해: {poisonIncrease:F1} ({effect.incomingDamageModifier * 100}%)");
            }
        }

        // 방어력 수정치 계산
        float defenseModifier = 1f;
        int originalDefense = victim.GetPlayer.defLevel;
        foreach (var effect in victim.GetPlayer.statusEffects)
        {
            if (effect.effectName == "방어력감소")
            {
                defenseModifier = effect.defenseModifier;
                Debug.LogWarning($"[방어력 감소] {victim.GetPlayer.charName}의 방어력이 {defenseModifier * 100}%로 감소됨");
                Debug.LogWarning($"[방어력 감소] 원래 방어력: {originalDefense} -> 감소된 방어력: {Mathf.RoundToInt(originalDefense * defenseModifier)}");
            }
        }

        // 최종 피해 적용
        int modifiedDefense = Mathf.RoundToInt(victim.GetPlayer.defLevel * defenseModifier);
        int finalDamageInt = Mathf.RoundToInt(finalDamage) - modifiedDefense;

        victim.GetPlayer.hp -= finalDamageInt;
        victim.UpdateStatus();
        UIManager.Instance.ShowDamageTextNearCharacter(finalDamageInt, victim.transform);
        UIManager.Instance.CreateBloodEffect(victim.transform.position);
        attacker.PlayHitSound();

        // 피해 상세 로그
        Debug.LogWarning($"[피해 상세 - {victim.GetPlayer.charName}]");
        Debug.LogWarning($"- 기본 피해량: {originalDamage}");
        Debug.LogWarning($"- 상태이상 적용 후 피해량: {finalDamage:F1} (차이: {finalDamage - originalDamage:+0.0;-0.0;0.0})");
        Debug.LogWarning($"- 방어력: {modifiedDefense} (원래: {originalDefense}, 감소: {originalDefense - modifiedDefense})");
        Debug.LogWarning($"- 최종 피해: {finalDamageInt}");

        // 피해를 입을 때 50% 확률로 정신력 감소
        if (Random.value < 0.5f)
        {
            victim.GetPlayer.menTality = Mathf.Max(0, victim.GetPlayer.menTality - 2);
            victim.UpdateStatus();
            Debug.LogWarning($"[정신력 감소] {victim.GetPlayer.charName}의 정신력 2 감소 (피해로 인한 감소)");
        }

        StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeIntensity));

        // 피해자가 사망는지 확인
        if (victim != null && 0 >= victim.GetPlayer.hp)
        {
            // OnDeath 메서드 호출 전에 필요한 정리 작업 수행
            victim.DestroySkillEffect();
            
            // 실루엣 매니저 비활성화 및 제거
            Silhouette silhouette = victim.GetComponent<Silhouette>();
            if (silhouette != null)
            {
                silhouette.Active = false;
                // 실루엣 크 찾아서 제거
                Transform bank = GameObject.Find($"{victim.gameObject.name}_SilhouetteBank")?.transform;
                if (bank != null)
                {
                    Destroy(bank.gameObject);
                }
            }
            
            // 체력 체크 및 전투 종료 체크
            if (victim.CompareTag("Player"))
            {
                playerCheck--;
            }
            else if (victim.CompareTag("Enemy"))
            {
                enemyCheck--;
            }
            CheckBattleEnd(); // 즉시 전투 종료 체크
            
            // OnDeath 메서드 호출 (페이드 아웃 애니메이션 실행)
            victim.OnDeath();
            
            // 페이드 아웃 애니메이션이 완료될 때까지 대기
            yield return new WaitForSeconds(1.5f);
            
            // 피해자 리스트에서 제거
            if (targetObjects.Contains(victim))
            {
                targetObjects.Remove(victim);
            }
            if (playerObjects.Contains(victim))
            {
                playerObjects.Remove(victim);
            }
            
            // 피해자 완전히 제거
            Destroy(victim.gameObject);
            break;
        }

        // 잠시 대기하여 움직임을 볼 수 있게 함
        yield return new WaitForSeconds(1f);
    }

    // 정신력 변경
    // 합 패배 시 정신력 -10
    victim.GetPlayer.menTality = Mathf.Max(0, victim.GetPlayer.menTality - 10);
    victim.UpdateStatus();
    Debug.LogWarning($"{victim.GetPlayer.charName}의 정신력 10 감소 (합 패배)");
    
    // 합 승리 시 정신력 +5 (최대 100)
    if (attacker.GetPlayer.menTality < 100)
    {
        attacker.GetPlayer.menTality = Mathf.Min(100, attacker.GetPlayer.menTality + 5);
        attacker.UpdateStatus();
        Debug.LogWarning($"{attacker.GetPlayer.charName}의 정신력 5 증가 (합 승리)");
    }

    // 캐릭터들의 움직임이 끝난 후 대기
    yield return StartCoroutine(WaitForMovement(attackerMove, victimMove));

    // 1초 대기
    yield return new WaitForSeconds(1f);

    // 캐릭터 null이 아닐 때만 원래 위치로 돌아가기
    if (attacker != null && attacker.gameObject != null)
    {
        StartCoroutine(ReturnCharacterToInitialPosition(attacker));
    }
    if (victim != null && victim.gameObject != null)
    {
        StartCoroutine(ReturnCharacterToInitialPosition(victim));
    }

    // 전투 종료 시 스킬 이펙트 제거
    attacker.DestroySkillEffect();
    
    // 피해자가 살아있을 때만 스킬 이펙트 제거
    if (victim != null && victim.gameObject != null)
    {
        victim.DestroySkillEffect();
    }

    //카메라를 초기 위치와 사이즈로 되돌리기
    CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
    if (cameraFollow != null)
    {
        cameraFollow.ResetCamera();
    }

    // "연속 찌르기" 스킬 처리
    if (attacker.GetPlayer.skills[0].skillName == "연속 찌르기")
    {
        if (attacker.GetPlayer.dmg > victim.GetPlayer.dmg) // 합 승리 시
        {
            victim.GetPlayer.AddStatusEffect("출혈");
            Debug.LogWarning($"{victim.GetPlayer.charName}에게 출혈 효과가 부여되었습니다.");
            // 출혈 이펙트 표시
            UIManager.Instance.CreateBloodEffect(victim.transform.position);
        }
        else // 합 패배 시
        {
            attacker.GetPlayer.AddStatusEffect("혼란");
            Debug.LogWarning($"{attacker.GetPlayer.charName}에게 혼란 효과가 부여되었습니다.");
        }
    }

    // "사기 진작" 스킬 처리
    if (attacker.GetPlayer.skills[0].skillName == "사기 진작")
    {
        if (attacker.GetPlayer.dmg > victim.GetPlayer.dmg) // 합 승리 시
        {
            // 상대에게 방어력 감소 효과 부여
            victim.GetPlayer.AddStatusEffect("방어력감소");
            Debug.LogWarning($"{victim.GetPlayer.charName}에게 방어력 감소 효과가 부여되었습니다.");
        }
        else // 합 패배 시
        {
            // 공격자에게 2턴 출혈 효과 부여
            attacker.GetPlayer.AddStatusEffect("출혈2턴");
            Debug.LogWarning($"{attacker.GetPlayer.charName}에게 2턴 출혈 효과가 부여되었습니다.");
            UIManager.Instance.CreateBloodEffect(attacker.transform.position);
        }
    }

    // "화염 공격" 스킬의 가 피해 계산
    if (attacker.GetPlayer.skills[0].skillName == "화염 공격")
    {
        int additionalDamage = attacker.successCount * 2;
        attacker.GetPlayer.dmg += additionalDamage;
        Debug.LogWarning($"{attacker.GetPlayer.charName}의 화염 공격 추가 피해: {additionalDamage} (성공한 코인 수: {attacker.successCount})");
        attacker.UpdateSkillEffectDamage(attacker.GetPlayer.dmg);
    }

    // "파열" 스킬 처리
    if (attacker.GetPlayer.skills[0].skillName == "파열")
    {
        if (attacker.GetPlayer.dmg > victim.GetPlayer.dmg) // 합 승리 시
        {
            // 상대에게 방어력 감소 효과 부여
            victim.GetPlayer.AddStatusEffect("방어력감소");
            Debug.LogWarning($"{victim.GetPlayer.charName}에게 방어력 감소 효과가 부여되었습니다.");
        }
        else // 합 패배 시
        {
            // 공격자에게 2턴 출혈 효과 부여
            attacker.GetPlayer.AddStatusEffect("출혈2턴");
            Debug.LogWarning($"{attacker.GetPlayer.charName}에게 2턴 출혈 효과가 부여되었습니다.");
            UIManager.Instance.CreateBloodEffect(attacker.transform.position);
        }
    }

    // 피해 적용 후 정신력 20 미만 체크 (혼란 자연 발생)
    if (victim.GetPlayer.menTality < 20 && !victim.GetPlayer.statusEffects.Exists(e => e.effectName == "혼란"))
    {
        victim.GetPlayer.AddStatusEffect("혼란");
        Debug.LogWarning($"{victim.GetPlayer.charName}의 정신력이 20 미만으로 떨어져 혼란 상태가 되었습니다.");
    }

    // 혼란 상태에서의 자해 체크 (정신력 20 미만에서 자연 발생한 경우)
    bool isNaturalConfusion = attacker.GetPlayer.menTality < 20 && 
                             attacker.GetPlayer.statusEffects.Exists(e => e.effectName == "혼란");
    
    if (isNaturalConfusion && attacker.GetPlayer.dmg > victim.GetPlayer.dmg)
    {
        if (Random.value < 0.1f) // 10% 확률로 자해
        {
            int selfDamage = Mathf.RoundToInt(attacker.GetPlayer.dmg * 0.5f); // 자신의 피해량의 50%
            attacker.GetPlayer.hp -= selfDamage;
            attacker.UpdateStatus();
            UIManager.Instance.ShowDamageTextNearCharacter(selfDamage, attacker.transform);
            UIManager.Instance.CreateBloodEffect(attacker.transform.position);
            Debug.LogWarning($"[혼란 자해] {attacker.GetPlayer.charName}이(가) 혼란으로 인해 {selfDamage}의 자해 피해를 입음");
        }
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
        Debug.LogWarning($"교착 상태 발생 {draw} 회");
        
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
            Debug.LogWarning($"{playerObject.GetPlayer.charName}과 {targetObject.GetPlayer.charName} 의 정신력 감소");
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
            
            // 일힐 시 정신력 2 회복 (최대 100)
            if (winner.GetPlayer.menTality < 100)
            {
                winner.GetPlayer.menTality = Mathf.Min(100, winner.GetPlayer.menTality + 2);
                winner.UpdateStatus();
                Debug.LogWarning($"{winner.GetPlayer.charName}의 정신력 2 회복 (일힐)");
            }
        }
        else
        {
            winner = targetObject;
            loser = playerObject;
            
            // 일힐 시 정신력 2 회복 (최대 100)
            if (winner.GetPlayer.menTality < 100)
            {
                winner.GetPlayer.menTality = Mathf.Min(100, winner.GetPlayer.menTality + 2);
                winner.UpdateStatus();
                Debug.LogWarning($"{winner.GetPlayer.charName}의 정신력 2 회복 (일힐)");
            }
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
        if (playerObject != null && playerObject.GetPlayer.hp <= 0)
        {
            playerCheck--;
            playerObject.OnDeath();
            CheckBattleEnd(); // 체력 확인 후 바로 전투 종료 체크
        }
        if (targetObject != null && targetObject.GetPlayer.hp <= 0)
        {
            enemyCheck--;
            targetObject.OnDeath();
            CheckBattleEnd(); // 체력 확인 후 바로 전투 종료 체크
        }
    }

    void CheckBattleEnd()
    {
        Debug.LogWarning($"전투 종료 체크 - 플레이어: {playerCheck}, 적: {enemyCheck}");
        if (enemyCheck <= 0)
        {
            StartCoroutine(EndBattleSequence(true));
        }
        else if (playerCheck <= 0)
        {
            StartCoroutine(EndBattleSequence(false));
        }
    }

    private IEnumerator EndBattleSequence(bool isVictory)
    {
        // 시간 슬로우 모션
        Time.timeScale = 0.5f;

        // 마지막 캐릭터가 원위치로 돌아올 때까지 대기
        foreach (var player in playerObjects)
        {
            if (player != null)
            {
                yield return StartCoroutine(ReturnCharacterToInitialPosition(player));
            }
        }
        foreach (var enemy in targetObjects)
        {
            if (enemy != null)
            {
                yield return StartCoroutine(ReturnCharacterToInitialPosition(enemy));
            }
        }

        // 시간 원래대로
        Time.timeScale = 1f;

        // 게임 상태 설정
        state = isVictory ? GameState.win : GameState.lose;
        
        // UI 표시
        UIManager.Instance.ShowGameEndUI();
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        
        // 모든 적 캐릭터의 새로운 스킬 선택
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            CharacterProfile profile = enemy.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                profile.SelectRandomEnemySkill();
            }
        }

        // 모든 플레이어 캐릭터의 상태 업데이트
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            CharacterProfile profile = player.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                profile.GetPlayer.coin = profile.GetPlayer.maxCoin + profile.GetPlayer.nextTurnCoinModifier;
                profile.GetPlayer.nextTurnCoinModifier = 0;
                profile.UpdateStatus();
                Debug.LogWarning($"{profile.GetPlayer.charName}의 코인이 {profile.GetPlayer.coin}개로 설정");
            }
        }

        Debug.LogWarning("적 공격");

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
                StartCoroutine(MoveWithDashSound(playerObjects[i], playerBattlePositions[i].position));
            }

            // 적 이동
            if (i < targetObjects.Count)
            {
                StartCoroutine(MoveWithDashSound(targetObjects[i], enemyBattlePositions[i].position));
            }
        }
    }

    private IEnumerator MoveWithDashSound(CharacterProfile character, Vector3 targetPosition)
    {
        // 대시 사운드 재생
        character.PlayDashSound();
        
        // 약간의 딜레이
        yield return new WaitForSeconds(0.1f);
        
        // 이동 시작
        BattleMove characterMove = character.GetComponent<BattleMove>();
        if (characterMove != null)
        {
            characterMove.MoveToPosition(targetPosition);
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
        if (character == null || !character.gameObject.activeSelf) yield break;
        
        // 대시 사운드 재생
        character.PlayDashSound();
        
        // 약간의 딜레이
        yield return new WaitForSeconds(0.1f);
        
        BattleMove characterMove = character.GetComponent<BattleMove>();
        if (characterMove != null)
        {
            characterMove.ReturnToInitialPosition();
            while (characterMove != null && characterMove.IsMoving())
            {
                yield return null;
            }
        }
    }

    public void OnSkillSelected()
    {
        skillSelected = true;
        selecting = true; // 적 선택 모드 활성화
        Debug.LogWarning("스킬 선택 완료, 적 선택 모드로 전환");
    }

    private void SpawnSelectedCharacters()
    {
        Debug.LogWarning($"캐릭터 생성 시작: {DeckData.selectedCharacterPrefabs.Count}개의 캐릭");
        
        for (int i = 0; i < DeckData.selectedCharacterPrefabs.Count; i++)
        {
            if (i < stageData.characterPositions.Length)
            {
                Vector3 spawnPosition = stageData.characterPositions[i];
                GameObject characterPrefab = DeckData.selectedCharacterPrefabs[i];
                
                Debug.LogWarning($"캐릭터 {i + 1} 생성 위치: {spawnPosition}");
                
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
                    Debug.LogWarning($"캐릭 {profile.GetPlayer.charName} 생성 완료");
                }
            }
            else
            {
                Debug.LogWarning($"캐릭터 {i + 1}의 생성 위치가 정의되지 않았습니다.");
            }
        }
    }
}