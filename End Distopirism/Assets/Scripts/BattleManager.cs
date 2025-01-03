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
    pause,
    CheckBattleEnd
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

    private bool skillSelected = false;

    [Header("스테이지 설정")]
    [SerializeField] private StageData stageData;

    private BattleLine battleLine; // BattleLine 참조 추가

    [SerializeField] private SphereCollider battleZone; // 2D에서 3D로 변경
    [SerializeField] private float safeDistance = 150f; // 전투 쌍 사이의 최소 안전 거리

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

        // 적 캐릭터들 초기 스킬 설정
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            CharacterProfile profile = enemy.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                profile.InitializeEnemySkill();
            }
        }

        // 모든 플레이어 캐릭터의 카드 초기화
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            CharacterProfile profile = player.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                profile.InitializeRandomCards();
            }
        }

        // BattleLine 컴포넌트 찾기
        battleLine = FindObjectOfType<BattleLine>();
        if (battleLine == null)
        {
            Debug.LogWarning("BattleLine 컴포넌트를 찾을 수 없습니다.");
        }

        // 모든 캐릭터의 카드 상태 초기화
        foreach (var character in FindObjectsOfType<CharacterProfile>())
        {
            character.OnBattleStart();
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
                .OnStart(() => Debug.Log($"플���이어 캐릭터 {i} 등장 시작"))
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
                .OnStart(() => Debug.Log($"적 캐릭터 {i} 등장 "))
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

        isAttacking = true;

        // 모든 캐릭터의 카드 비활성화
        foreach (var player in playerObjects)
        {
            player.OnAttackStart();
        }
        foreach (var target in targetObjects)
        {
            target.OnAttackStart();
        }

        // 공격 시작 시 라인 활성화
        if (battleLine != null)
        {
            Debug.Log("전투 시작: 라인 활성화");
            battleLine.SetLinesActive(true);
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

        // 전투 시작 시 카메라 설정
        var battleCamera = FindObjectOfType<CameraFollow>();
        if (battleCamera != null)
        {
            battleCamera.SetupCombatView();
        }
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
            GameObject clickObject = UIManager.Instance.MouseGetObject();
            if (clickObject != null)
            {
                if (clickObject.CompareTag("Enemy"))
                {
                    // 선택 진행중이 아닐 때 적 클릭
                    CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();
                    if (selectedEnemy != null && targetObjects.Contains(selectedEnemy))
                    {
                        // 해당 적과 연결된 플레이어 찾기
                        int enemyIndex = targetObjects.IndexOf(selectedEnemy);
                        if (enemyIndex >= 0 && enemyIndex < playerObjects.Count)
                        {
                            // 화살표 제거
                            arrowCreator.RemoveConnection(playerObjects[enemyIndex].transform, selectedEnemy.transform);
                            
                            // 선택 상태 해제
                            selectedEnemy.isSelected = false;
                            playerObjects[enemyIndex].isSelected = false;
                            
                            // 플레이어의 스킬 선택 초기화 및 카드 숨기기
                            playerObjects[enemyIndex].OnDeselected();
                            
                            // 리스트에서 제거
                            targetObjects.RemoveAt(enemyIndex);
                            playerObjects.RemoveAt(enemyIndex);

                            // 선택 상태 초기화
                            skillSelected = false;
                            selecting = false;
                            
                            Debug.LogWarning($"선택 해제: 처음 상태��� 돌아감");
                        }
                    }
                }
                else if (clickObject.CompareTag("Player"))
                {
                    CharacterProfile selectedPlayer = clickObject.GetComponent<CharacterProfile>();
                    
                    // 살아있는 적의 수 확인
                    int aliveEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
                    
                    // 이미 최대 선택 가능한 플레이어 수에 도달했는지 확인
                    if (playerObjects.Count >= aliveEnemies && !playerObjects.Contains(selectedPlayer))
                    {
                        Debug.LogWarning($"더 이상 플레이어를 선택할 수 없습니다. (살아있는 적: {aliveEnemies}명)");
                        return;
                    }

                    // 이전 선택된 플레이어의 선택 해제
                    if (playerObjects.Count > 0 && targetObjects.Count == 0)
                    {
                        CharacterProfile previousPlayer = playerObjects[playerObjects.Count - 1];
                        previousPlayer.isSelected = false;
                        previousPlayer.OnDeselected();
                        previousPlayer.ResetSkillSelection(); // 스킬 선택도 초기화
                        playerObjects.Clear();
                    }

                    // 새로운 플레이어 선택
                    if (!playerObjects.Contains(selectedPlayer))
                    {
                        playerObjects.Add(selectedPlayer);
                        selectedPlayer.isSelected = true;
                        selectedPlayer.ShowCharacterInfo();
                    }
                }
            }
        }
        else if (skillSelected && selecting)
        {
            GameObject clickObject = UIManager.Instance.MouseGetObject();
            if (clickObject != null && clickObject.CompareTag("Enemy"))
            {
                CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();
                
                // 이미 선택된 적인지 확인
                if (targetObjects.Contains(selectedEnemy))
                {
                    Debug.LogWarning($"{selectedEnemy.GetPlayer.charName}는 이미 선택된 적입니다.");
                    return;
                }

                CharacterProfile currentPlayer = playerObjects[playerObjects.Count - 1];

                // 이미 이 플레이어의 타겟이 있는 경우, 기존 타겟과 화살표 제거
                int playerIndex = playerObjects.IndexOf(currentPlayer);
                if (playerIndex < targetObjects.Count)
                {
                    CharacterProfile oldTarget = targetObjects[playerIndex];
                    if (oldTarget != null)
                    {
                        oldTarget.isSelected = false;
                        arrowCreator.RemoveConnection(currentPlayer.transform, oldTarget.transform);
                    }
                    targetObjects[playerIndex] = selectedEnemy;
                }
                else
                {
                    targetObjects.Add(selectedEnemy);
                }

                selectedEnemy.isSelected = true;
                selectedEnemy.ShowCharacterInfo();
                selecting = false;
                skillSelected = false;
                
                // 화살표 연결
                arrowCreator.AddConnection(currentPlayer.transform, selectedEnemy.transform);

                Debug.LogWarning($"{currentPlayer.GetPlayer.charName}가 {selectedEnemy.GetPlayer.charName}를 타겟으로 선택");
            }
        }

        // 선택 상태 업데이트
        playerSelect = playerObjects.Count > 0;
        enemySelect = targetObjects.Count > 0;
        allTargetSelected = playerObjects.Count > 0 && targetObjects.Count == playerObjects.Count;
    }

    void CoinRoll(CharacterProfile Object, ref int successCount)
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        
        float maxMenTality = 100f;
        float maxProbability = 0.6f;

        float currentProbability = Mathf.Max(0f, maxProbability * maxMenTality);
        Debug.LogWarning($"[코인 확률] {Object.GetPlayer.charName}의 현재 성공 확률: {currentProbability * 100:F1}%");

        Object.successCount = 0; // 코인 성공 횟수 초기화
        
        // 전체 코인 수만큼 반복 (1부터 시작하는 대신 0부터 시작)
        for (int j = 0; j < Object.GetPlayer.coin; j++)
        {
            if (Random.value < currentProbability)
            {
                Object.successCount++;
                // 코인 성공 시마다 UI 업데이트
                Object.UpdateCoinCount(Object.successCount);
            }
        }
        
        Object.coinBonus = Object.successCount * Object.GetPlayer.dmgUp;
        Debug.LogWarning($"{Object.GetPlayer.charName}의 코인 던지기 성공 횟수: {Object.successCount} / {Object.GetPlayer.coin}");
        Debug.LogWarning($"{Object.GetPlayer.charName}의 남은 코인: {Object.GetPlayer.coin} / {Object.GetPlayer.maxCoin}");
    }

    void DiffCheck()// 공격 레벨과 방어 레벨을 비교하 보너스 및 패널티 적용
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
            // null 체크 추가
            if (i >= playerObjects.Count || i >= targetObjects.Count)
            {
                Debug.LogWarning("전투 참가자 수가 일치하지 않습니다.");
                break;
            }

            CharacterProfile playerObject = playerObjects[i];
            CharacterProfile targetObject = targetObjects[i];

            // null 체크
            if (playerObject == null || targetObject == null)
            {
                Debug.LogWarning("전투 참가자가 null입니다.");
                continue;
            }

            // 스킬 배열이 null이거나 비어있는지 확인
            if (playerObject.GetPlayer.skills == null || playerObject.GetPlayer.skills.Count == 0 ||
                targetObject.GetPlayer.skills == null || targetObject.GetPlayer.skills.Count == 0)
            {
                Debug.LogWarning("스킬이 설정되지 않은 캐릭터가 있습니다.");
                continue;
            }

            //카메라 공격자에게 줌 인
            //var battleCamera = FindObjectOfType<CameraFollow>();
            //if (battleCamera != null)
            //{
            //    battleCamera.ZoomInOnTarget(playerObject.transform);
            //    battleCamera.isFollowing = true;
            //}

            playerObject.successCount = targetObject.successCount = 0;
            Debug.Log($"플레이어: {playerObject.GetPlayer.charName}, 적: {targetObject.GetPlayer.charName}");
            CoinRoll(playerObject, ref playerObject.successCount);
            CoinRoll(targetObject, ref targetObject.successCount);

            CalculateDamage(playerObject, targetObject, out int playerDamage, out int targetDamage);

            yield return new WaitForSeconds(1f);

            if (!(playerObject.GetPlayer.coin > 0 || targetObject.GetPlayer.coin > 0))
            {
                bool isLastBattle = (i == matchCount - 1);
                yield return StartCoroutine(ApplyDamageAndMoveCoroutine(playerObject, targetObject, isLastBattle));
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

            yield return new WaitForSeconds(0.5f);
        }

        // 모든 전투가 끝난 후 8초 대기 후 카메라 리셋
        yield return new WaitForSeconds(8f);
        var mainCamera = FindObjectOfType<CameraFollow>();
        if (mainCamera != null)
        {
            mainCamera.isFollowing = false;
            mainCamera.ResetCamera();
        }

        isAttacking = false;

        yield return new WaitUntil(() => AllCombatantsStoppedMoving());

        //  종료 처리
        CheckBattleEnd();
        if (state == GameState.playerTurn)
        {
            state = GameState.enemyTurn;
            // 플레이어 턴 시작 시 기존 선택 리스트 기화
            playerObjects.Clear();
            targetObjects.Clear();
            StartCoroutine(EnemyTurn());
        }
    }

    //데미지 연산 함수
    void CalculateDamage(CharacterProfile playerObject, CharacterProfile targetObject, out int playerDamage, out int targetDamage)
    {
        playerDamage = (int)(playerObject.GetPlayer.minDmg + playerObject.coinBonus + playerObject.bonusDmg);
        targetDamage = (int)(targetObject.GetPlayer.minDmg + targetObject.coinBonus + targetObject.bonusDmg);

        playerObject.GetPlayer.dmg = playerDamage;
        targetObject.GetPlayer.dmg = targetDamage;

        Debug.Log($"{playerObject.GetPlayer.charName}의 최종 데미지: {playerDamage}");
        Debug.Log($"{targetObject.GetPlayer.charName}의 최종 데미지: {targetDamage}");

        Vector3 playerObjectPosition = playerObject.transform.position;
        Vector3 targetObjectPosition = targetObject.transform.position;

        if (playerDamage > targetDamage)
        {
            UIManager.Instance.ShowBattleResultText("승리", playerObject.transform);
            UIManager.Instance.ShowBattleResultText("패배", targetObject.transform);
            playerObject.PlayHitSound(); // 합 승리 시 히트 사드
        }
        else
        {
            UIManager.Instance.ShowBattleResultText("패배", playerObject.transform);
            UIManager.Instance.ShowBattleResultText("승리", targetObject.transform);
            targetObject.PlayHitSound(); // 합 승리 시 히트 사운드
        }
    }


IEnumerator ApplyDamageAndMoveCoroutine(CharacterProfile attacker, CharacterProfile victim, bool isLastBattle)
{
    // 시작 시 null 체크
    if (attacker == null || victim == null || 
        attacker.gameObject == null || victim.gameObject == null)
    {
        yield break;
    }

    BattleMove attackerMove = attacker.GetComponent<BattleMove>();
    BattleMove victimMove = victim.GetComponent<BattleMove>();

     // Hit 애니메이션 재생 및 애니메이션 길이 가져오기
    float animationLength = 0f;
    Animator attackerAnimator = attacker.GetComponent<Animator>();
    if (attackerAnimator != null)
    {
        AnimatorStateInfo stateInfo = attackerAnimator.GetCurrentAnimatorStateInfo(0);
        animationLength = stateInfo.length;
        attackerMove?.Attack(); // Hit 애니메이션 재생
    }
    else
    {
        attackerMove?.Attack();
        animationLength = 4f; // 애니메이터가 없는 경우 기본값
    }
    attacker.ShowSkillEffect(attacker.GetPlayer.dmg);
   

    for (int j = 0; j < attacker.GetPlayer.coin; j++)
    {
        attacker.successCount = 0;
        attacker.coinBonus = 0;
        if (j > 0)
        {
            CoinRoll(attacker, ref attacker.successCount);
            attacker.GetPlayer.dmg = attacker.GetPlayer.minDmg + attacker.coinBonus + attacker.bonusDmg;
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

        // 전진과 후퇴가 끝날 때까지 기달림
        yield return StartCoroutine(WaitForMovement(attackerMove, victimMove));

        // 피해량 계산
        float finalDamage = attacker.GetPlayer.dmg;
        float originalDamage = finalDamage;


        int finalDamageInt = Mathf.RoundToInt(finalDamage);

        victim.GetPlayer.hp -= finalDamageInt;
        victim.UpdateStatus();
        UIManager.Instance.ShowDamageTextNearCharacter(finalDamageInt, victim.transform);
        UIManager.Instance.CreateBloodEffect(victim.transform.position);
        attacker.PlayHitSound();


        // 피해 상세 로그
        Debug.LogWarning($"[피해 상세 - {victim.GetPlayer.charName}]");
        Debug.LogWarning($"- 기본 피해량: {originalDamage}");
        Debug.LogWarning($"- 최종 피해: {finalDamageInt}");

        // 피해를 입을 때 100% 확률 정신력 소 (기존 50% 확률에서 변경)
        victim.GetPlayer.menTality = Mathf.Max(0, victim.GetPlayer.menTality - 2);
        victim.UpdateStatus();
        Debug.LogWarning($"{victim.GetPlayer.charName}의 정신력 2 감소 (피해로 인한 감소)");


        // 피해자가 사망했는지 확인
        if (victim != null && 0 >= victim.GetPlayer.hp)
        {
            // OnDeath 메서드 호출 전에 필요한 정리 작업 수행
            victim.DestroySkillEffect();
            
            // 실루엣 매니저 비활성화 및 제거
            Silhouette silhouette = victim.GetComponent<Silhouette>();
            if (silhouette != null)
            {
                silhouette.Active = false;
                // 실루엣 크기 찾아서 제거
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
        yield return new WaitForSeconds(animationLength);
    }

    // 정신력 변경
    // 합 패배  정신력 -10
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
    
    // 피해자가 존재할 때만 스킬 이펙트 제거
    if (victim != null && victim.gameObject != null)
    {
        victim.DestroySkillEffect();
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
            
            // 이길 시 정신력 2 회복 (최대 100)
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
            
            // 이길 시 정신력 2 회복 (최대 100)
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

    //틀 시작할 때 인식한 아군&적군의 총 갯수를 체력이 0이 됐다면 차감하기.
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

        // 모든 캐릭터가 원위치로 돌아온 후 라인 비활성화
        if (battleLine != null)
        {
            battleLine.SetLinesActive(false);  // 이것이 UI를 다시 활성화할 것입니다
        }

        // 대기 시간
        yield return new WaitForSeconds(1f);

        // 시간 원래대로
        Time.timeScale = 1f;

        // 게임 상태 설정
        state = isVictory ? GameState.win : GameState.lose;
        
        // UI 표시
        UIManager.Instance.ShowGameEndUI();

        // 카메라 리셋 (isBattle 상태도 함께 해제)
        var battleCamera = FindObjectOfType<CameraFollow>();
        if (battleCamera != null)
        {
            battleCamera.isBattle = false;  // 명시적으로 isBattle 상태 해제
            battleCamera.ResetCamera();
        }

        // 모든 캐릭터의 카드 상태 초기화
        foreach (var character in FindObjectsOfType<CharacterProfile>())
        {
            character.OnBattleEnd();
        }
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        
        // 모든 적 캐릭터의 새로운 스킬 선택과 코인 초기화
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            CharacterProfile profile = enemy.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                // 적 캐릭터의 코인 초기화
                profile.GetPlayer.coin = profile.GetPlayer.maxCoin + profile.GetPlayer.nextTurnCoinModifier;
                profile.GetPlayer.nextTurnCoinModifier = 0;
                profile.UpdateStatus();
                Debug.LogWarning($"{profile.GetPlayer.charName}의 코인이 {profile.GetPlayer.coin}개로 설정");
                
                // 새로운 스킬 선택
                profile.SelectRandomEnemySkill();
            }
        }

        // 모든 플레이어 캐릭터의 상태 업데이트
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            CharacterProfile profile = player.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                // 코인 초기화
                profile.GetPlayer.coin = profile.GetPlayer.maxCoin + profile.GetPlayer.nextTurnCoinModifier;
                profile.GetPlayer.nextTurnCoinModifier = 0;
                profile.UpdateStatus();
                
                // 선택된 스킬 초기화
                profile.ResetSkillSelection();
                
                Debug.LogWarning($"{profile.GetPlayer.charName}의 상태 초기화 (새 턴)");
            }
        }
        state = GameState.playerTurn;
        allTargetSelected = false;
    }

    // BattleManager 클래스 내부 추가

    private void MoveCombatants()
    {
        if (battleZone == null)
        {
            Debug.LogError("BattleZone이 설정되지 않았습니다!");
            return;
        }

        Vector3 zoneCenter = battleZone.bounds.center;
        Vector3 zoneSize = battleZone.bounds.size;
        List<Vector3> occupiedPositions = new List<Vector3>(); // 이미 사용된 위치들을 저장

        for (int i = 0; i < Mathf.Min(playerObjects.Count, targetObjects.Count); i++)
        {
            Vector3 playerTargetPos;
            Vector3 enemyTargetPos;
            bool validPosition = false;
            int maxAttempts = 30; // 최대 시도 횟수
            int attempts = 0;

            do
            {
                // 랜덤 위치 생성 (X와 Z축 모두 랜덤)
                float randomX = Random.Range(zoneCenter.x - zoneSize.x/2, zoneCenter.x + zoneSize.x/2);
                float randomZ = Random.Range(zoneCenter.z - zoneSize.z/2, zoneCenter.z + zoneSize.z/2);

                playerTargetPos = new Vector3(randomX, zoneCenter.y, randomZ);
                
                // 적의 위치도 X와 Z축 모두 오프셋 적용
                float enemyOffsetX = Random.Range(150f, 250f); // X축 오프셋 랜덤화
                float enemyOffsetZ = Random.Range(-100f, 100f); // Z축 오프셋 랜덤화
                enemyTargetPos = new Vector3(randomX + enemyOffsetX, zoneCenter.y, randomZ + enemyOffsetZ);

                // 이전 위치들과의 거리 확인
                validPosition = true;
                foreach (Vector3 pos in occupiedPositions)
                {
                    if (Vector3.Distance(playerTargetPos, pos) < safeDistance ||
                        Vector3.Distance(enemyTargetPos, pos) < safeDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }

                attempts++;
                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning("안전한 위치를 찾지 못했습니다. 마지막 시도 위치를 사용합니다.");
                    validPosition = true;
                }

            } while (!validPosition);

            // 사용된 위치 저장
            occupiedPositions.Add(playerTargetPos);
            occupiedPositions.Add(enemyTargetPos);

            // 플레이어 이동
            if (i < playerObjects.Count && playerObjects[i] != null)
            {
                StartCoroutine(MoveWithDashSound(playerObjects[i], playerTargetPos));
            }

            // 적 이동
            if (i < targetObjects.Count && targetObjects[i] != null)
            {
                StartCoroutine(MoveWithDashSound(targetObjects[i], enemyTargetPos));
            }
        }
    }

    private IEnumerator MoveWithDashSound(CharacterProfile character, Vector3 targetPosition)
    {
        // 약간의 딜레이
        yield return new WaitForSeconds(0.1f);
        
        // 이동 시작
        BattleMove characterMove = character.GetComponent<BattleMove>();
        if (characterMove != null)
        {
            characterMove.MoveToPosition(targetPosition);
        }
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
        // false를 전달하여 마지막 전투가 아님을 표시
        yield return StartCoroutine(ApplyDamageAndMoveCoroutine(attacker, victim, false));
        
        // 모든 데미지 적용 움직임이 끝 후에 체력 확인 및 전투 종료 처리
        CheckHealth(attacker, victim);
        CheckBattleEnd();
    }

    public IEnumerator ReturnCharacterToInitialPosition(CharacterProfile character)
    {
        if (character == null || !character.gameObject.activeSelf) yield break;
        
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

        yield return new WaitForSeconds(0.5f);
    }


    public void OnSkillSelected()
    {
        skillSelected = true;
        selecting = true;
        
        // 현재 선택된 플레이어의 타겟이 있다면 제거
        if (playerObjects.Count > 0)
        {
            CharacterProfile currentPlayer = playerObjects[playerObjects.Count - 1];
            int playerIndex = playerObjects.IndexOf(currentPlayer);
            if (playerIndex < targetObjects.Count)
            {
                CharacterProfile oldTarget = targetObjects[playerIndex];
                if (oldTarget != null)
                {
                    oldTarget.isSelected = false;
                    arrowCreator.RemoveConnection(currentPlayer.transform, oldTarget.transform);
                    targetObjects[playerIndex] = null;
                }
            }
        }
        
        Debug.LogWarning("킬 선택 완료, 적 선택 모드로 전환");
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

    // 승리 처리
    public void HandleVictory()
    {
        if (battleLine != null)
        {
            battleLine.SetVictorySpeed();
        }
    }

    // 패배 처리
    public void HandleDefeat()
    {
        if (battleLine != null)
        {
            battleLine.SetDefeatSpeed();
        }
    }
}