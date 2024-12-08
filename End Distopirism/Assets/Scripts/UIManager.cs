using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab; 
    public Canvas canvas;
    private static UIManager uimInstance;

    public GameObject playerProfilePanel;
    public GameObject enemyProfilePanel;

    public GameObject healthBarPrefab;
    public GameObject mentalityBarPrefab;

    public GameObject playerSkillEffectPrefab;
    public GameObject enemySkillEffectPrefab;

    [SerializeField]
    private TextMeshProUGUI turnText;
    private int currentTurn = 0;

    public GameObject pausePanel;
    private Canvas pausePanelCanvas;

    private bool isPaused = false;

    [SerializeField] private Button speedButton;
    [SerializeField] private TextMeshProUGUI speedButtonText;
    private bool isSpeedUp = false;

    [Header("Game End UI")]
    public GameObject gameEndPanel;
    public Button menuButton;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Image fadeImage;

    [Header("Blood Effect")]
    public GameObject bloodEffectPrefab;
    public Sprite[] bloodSprites;      // 혈흔/출혈 효과 스프라이트
    public Sprite[] poisonSprites;     // 독 효과 스프라이트 (현재 미사용)
    public Sprite[] defenseSprites;    // 방어 효과 스프라이트 (현재 미사용)

    [Header("Effect Settings")]
    public float effectFadeInDuration = 0.3f;
    public float effectStayDuration = 0.5f;
    public float effectFadeOutDuration = 0.7f;
    public float effectScaleMin = 0.8f;
    public float effectScaleMax = 1.2f;

    [Header("Status Effect Icons")]
    [SerializeField] private GameObject statusEffectIconPrefab;
    [SerializeField] private Sprite bleedingSprite;
    [SerializeField] private Sprite confusionSprite;
    [SerializeField] private Sprite poisonSprite;
    [SerializeField] private Sprite defenseDownSprite;

    private Dictionary<CharacterProfile, List<GameObject>> characterStatusIcons = new Dictionary<CharacterProfile, List<GameObject>>();

    public static UIManager Instance
    {
        get
        {
            if (uimInstance == null)
            {
                uimInstance = FindObjectOfType<UIManager>();
                if (uimInstance == null)
                {
                    Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다.");
                }
            }
            return uimInstance;
        }
    }

    private void Awake()
    {
        if (uimInstance != null && uimInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        uimInstance = this;
    }

    public void Start()
    {
        currentTurn = 0;
        UpdateTurnText();

        if (pausePanel != null)
        {
            pausePanelCanvas = pausePanel.GetComponent<Canvas>();
            pausePanelCanvas.sortingOrder = 100;
        }

        if (speedButton != null)
        {
            speedButton.onClick.AddListener(ToggleGameSpeed);
            speedButtonText.text = "x1";
        }

        // GameEnd UI 초기화
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(false);

            // Menu 버튼 이벤트 설정
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(LoadMainScene);
            }
        }

        // Loading Screen 초기화
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, 0);
            }
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }

    public GameObject MouseGetObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject clickObject = null;

        if (Physics.Raycast(ray, out hit))
        {
            clickObject = hit.transform.gameObject;
            Debug.Log("클릭한 오브젝트: " + clickObject.name);
            Debug.Log("오브젝트 태그: " + clickObject.tag);

            return clickObject;
        }
        Debug.Log("충돌 없음");
        return null;
    }

    public void ToggleGameSpeed()
    {
        isSpeedUp = !isSpeedUp;
        Time.timeScale = isSpeedUp ? 2f : 1f;
        
        if (speedButtonText != null)
        {
            speedButtonText.text = isSpeedUp ? "x2" : "x1";
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pausePanel.transform.SetAsLastSibling();
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
    }

    public void ExitGame()
    {
        // 게임 속도 정상화
        Time.timeScale = 1f;
        
        // 페이드 아웃 후 StageSelect 씬으로 전환
        StartCoroutine(LoadStageSelectCoroutine());
    }

    private IEnumerator LoadStageSelectCoroutine()
    {
        // 로딩 화면 활성화
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            loadingScreen.transform.SetAsLastSibling();

            // 페이드 인
            if (fadeImage != null)
            {
                fadeImage.DOFade(1f, 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }

        // UIManager 인스턴스 제거
        if (uimInstance != null)
        {
            Destroy(uimInstance.gameObject);
            uimInstance = null;
        }

        // StageSelect 씬으로 전환
        SceneManager.LoadScene("StageSelect");
    }

    public void TurnCount()
    {
        currentTurn++;
        UpdateTurnText();
    }

    public void UpdateTurnText()
    {
        if (turnText != null)
        {
            turnText.text = "" + currentTurn;
        }
    }

    public void ResetTurnText()
    {
        currentTurn = 1;
        UpdateTurnText();
    }

    public void ShowCharacterInfo(CharacterProfile character, bool showSkill = false)
    {
        if (character.CompareTag("Player"))
        {
            playerProfilePanel.SetActive(true);
            UpdateCharacterInfoPanel(playerProfilePanel, character, showSkill, true);
        }
        else if (character.CompareTag("Enemy"))
        {
            enemyProfilePanel.SetActive(true);
            UpdateCharacterInfoPanel(enemyProfilePanel, character, showSkill, false);
        }
    }

    private void UpdateCharacterInfoPanel(GameObject panel, CharacterProfile character, bool showSkill, bool isPlayer)
    {
        // 기본 정보 업데이트
        TextMeshProUGUI dmgLevelText = panel.transform.Find("DmgLevelText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI defLevelText = panel.transform.Find("DefLevelText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI maxDmgText = panel.transform.Find("MaxDmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI minDmgText = panel.transform.Find("MinDmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI dmgUpText = panel.transform.Find("DmgUpText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI coinText = panel.transform.Find("CoinText").GetComponent<TextMeshProUGUI>();

        // 스킬 관련 UI 요소
        Image skillIcon = panel.transform.Find("SkillIcon").GetComponent<Image>();
        TextMeshProUGUI skillName = panel.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
        Transform skillCards = panel.transform.Find("SkillCards");

        // 기본 정보 설���
        dmgLevelText.text = character.GetPlayer.dmgLevel.ToString();
        defLevelText.text = character.GetPlayer.defLevel.ToString();
        maxDmgText.text = character.GetPlayer.maxDmg.ToString();
        minDmgText.text = character.GetPlayer.minDmg.ToString();
        dmgUpText.text = "+" + character.GetPlayer.dmgUp.ToString();
        coinText.text = character.GetPlayer.coin.ToString();

        // 스킬 정보 표시
        if (showSkill && character.GetPlayer.skills != null && character.GetPlayer.skills.Count > 0)
        {
            skillIcon.gameObject.SetActive(true);
            skillName.gameObject.SetActive(true);
            skillIcon.sprite = character.GetPlayer.skills[0].sprite;
            skillName.text = character.GetPlayer.skills[0].skillName;
        }
        else
        {
            skillIcon.gameObject.SetActive(false);
            skillName.gameObject.SetActive(false);
        }

        // SkillCards는 플레이어 패널에서만 사용
        if (skillCards != null)
        {
            skillCards.gameObject.SetActive(isPlayer);
        }

        Debug.LogWarning($"[캐릭터 정보 업데이트] {character.GetPlayer.charName} ({(isPlayer ? "플레이어" : "적")})" +
            $"\n- 공격력: {character.GetPlayer.dmgLevel}" +
            $"\n- 방어력: {character.GetPlayer.defLevel}" +
            $"\n- 데미지: {character.GetPlayer.minDmg}-{character.GetPlayer.maxDmg} (+{character.GetPlayer.dmgUp})" +
            $"\n- 코인: {character.GetPlayer.coin}" +
            (showSkill ? $"\n- 선택된 스킬: {character.GetPlayer.skills[0].skillName}" : ""));
    }

    public void ShowBattleResultText(string message, Vector3 position)
    {
        // Canvas2 찾기
        GameObject canvas2 = GameObject.Find("Canvas2");
        if (canvas2 == null)
        {
            Debug.LogError("Canvas2를 찾을 수 없습니다!");
            return;
        }

        GameObject damageText = Instantiate(damageTextPrefab, position, Quaternion.identity, canvas2.transform);
        TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
        textComponent.text = message;

        if (message == "승리")
        {
            textComponent.color = Color.green;
        }
        else if (message == "패배")
        {
            textComponent.color = Color.red;
        }

        StartCoroutine(AnimateDamageText(damageText));
    }

    public void ShowDamageTextNearCharacter(int damage, Transform characterTransform)
    {
        // Canvas2 찾기
        GameObject canvas2 = GameObject.Find("Canvas2");
        if (canvas2 == null)
        {
            Debug.LogError("Canvas2를 찾을 수 없습니다!");
            return;
        }

        Vector3 randomOffset = Random.insideUnitCircle * 50f;
        Vector3 spawnPosition = characterTransform.position + new Vector3(randomOffset.x, 100f + randomOffset.y, 0);

        GameObject damageText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, canvas2.transform);
        TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
        textComponent.text = damage < 0 ? damage.ToString() : "-" + damage.ToString();
        textComponent.color = Color.red;
        
        StartCoroutine(AnimateDamageText(damageText));
    }

    private IEnumerator AnimateDamageText(GameObject damageText)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Vector3 startPosition = damageText.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 50f;
        TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            damageText.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1 - t);
            yield return null;
        }

        Destroy(damageText);
    }

    public void SetBattleUI(bool isBattle)
    {
        if (isBattle)
        {
            if (playerProfilePanel != null)
                HideSkillCards(playerProfilePanel);
            if (enemyProfilePanel != null)
                HideSkillCards(enemyProfilePanel);
        }
        else
        {
            if (playerProfilePanel != null)
                ShowSkillCards(playerProfilePanel);
            if (enemyProfilePanel != null)
                ShowSkillCards(enemyProfilePanel);
        }
    }

    public void ShowGameEndUI()
    {
        if (gameEndPanel == null)
        {
            Debug.LogError("gameEndPanel이 할당되지 않았습니다.");
            return;
        }

        gameEndPanel.SetActive(true);
        gameEndPanel.transform.SetAsLastSibling();

        // Menu 버튼 이벤트 재설정
        if (menuButton != null)
        {
            Debug.Log("Menu 버튼 이벤트 설정");
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => {
                Debug.Log("Menu 버튼 클릭됨");
                LoadMainScene();
            });
        }
        else
        {
            Debug.LogError("menuButton이 할당���지 않았습니다.");
        }
    }

    // 씬 전환 메서드 추가
    public void LoadMainScene()
    {
        Debug.Log("LoadMainScene 호출됨");
        
        // 게임 속도 초기화
        Time.timeScale = 1f;
        
        // SceneButtonManager를 통해 다음 씬 이름 설정
        SceneButtonManager.nextSceneName = "MainScene";
        
        // 페이드 아웃 후 FakeLoading 씬으로 전환
        StartCoroutine(LoadMainSceneCoroutine());
    }

    private IEnumerator LoadMainSceneCoroutine()
    {
        // 로딩 화면 활성화
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            loadingScreen.transform.SetAsLastSibling();

            // 페이드 인
            if (fadeImage != null)
            {
                fadeImage.DOFade(1f, 0.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }

        // UIManager 인스턴스 제거
        if (uimInstance != null)
        {
            Destroy(uimInstance.gameObject);
            uimInstance = null;
        }

        // FakeLoading 씬으로 전환
        SceneManager.LoadScene("Loading");
    }

    // 혈흔 이펙트 생성 메서드
    public void CreateBloodEffect(Vector3 position)
    {
        if (bloodEffectPrefab == null || bloodSprites.Length == 0) return;

        GameObject bloodEffect = Instantiate(bloodEffectPrefab, position, Quaternion.identity);
        SpriteRenderer spriteRenderer = bloodEffect.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = bloodSprites[Random.Range(0, bloodSprites.Length)];
            bloodEffect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            float randomScale = Random.Range(effectScaleMin, effectScaleMax);
            bloodEffect.transform.localScale = Vector3.zero;

            Sequence bloodSequence = DOTween.Sequence();

            Color startColor = spriteRenderer.color;
            startColor.a = 0;
            spriteRenderer.color = startColor;

            bloodSequence.Append(bloodEffect.transform.DOScale(randomScale, effectFadeInDuration)
                .SetEase(Ease.OutQuart));
            bloodSequence.Join(spriteRenderer.DOFade(1f, effectFadeInDuration)
                .SetEase(Ease.OutQuart));

            bloodSequence.AppendInterval(effectStayDuration);

            bloodSequence.Append(bloodEffect.transform.DOScale(randomScale * 1.3f, effectFadeOutDuration)
                .SetEase(Ease.InQuart));
            bloodSequence.Join(spriteRenderer.DOFade(0f, effectFadeOutDuration)
                .SetEase(Ease.InQuart));

            bloodSequence.OnComplete(() => Destroy(bloodEffect));
        }
    }

    // 독 효과 생성 메서드
    public void CreatePoisonEffect(Vector3 position)
    {
        // 현재는 혈흔 이펙트를 녹색으로 변경하여 사용
        if (bloodEffectPrefab == null || bloodSprites.Length == 0) return;

        GameObject poisonEffect = Instantiate(bloodEffectPrefab, position, Quaternion.identity);
        SpriteRenderer spriteRenderer = poisonEffect.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = bloodSprites[Random.Range(0, bloodSprites.Length)];
            // 녹색으로 색상 변경
            spriteRenderer.color = new Color(0.2f, 1f, 0.2f, 0f);
            
            poisonEffect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            float randomScale = Random.Range(effectScaleMin, effectScaleMax);
            poisonEffect.transform.localScale = Vector3.zero;

            Sequence poisonSequence = DOTween.Sequence();

            poisonSequence.Append(poisonEffect.transform.DOScale(randomScale, effectFadeInDuration)
                .SetEase(Ease.OutQuart));
            poisonSequence.Join(spriteRenderer.DOFade(0.7f, effectFadeInDuration)
                .SetEase(Ease.OutQuart));

            poisonSequence.AppendInterval(effectStayDuration);

            poisonSequence.Append(poisonEffect.transform.DOScale(randomScale * 1.3f, effectFadeOutDuration)
                .SetEase(Ease.InQuart));
            poisonSequence.Join(spriteRenderer.DOFade(0f, effectFadeOutDuration)
                .SetEase(Ease.InQuart));

            poisonSequence.OnComplete(() => Destroy(poisonEffect));
        }
    }

    // 방어 효과 생성 메서드
    public void CreateDefenseEffect(Vector3 position)
    {
        // 현재는 혈흔 이펙트를 파란색으로 변경하여 사용
        if (bloodEffectPrefab == null || bloodSprites.Length == 0) return;

        GameObject defenseEffect = Instantiate(bloodEffectPrefab, position, Quaternion.identity);
        SpriteRenderer spriteRenderer = defenseEffect.GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = bloodSprites[Random.Range(0, bloodSprites.Length)];
            // 파란색으로 색상 변경
            spriteRenderer.color = new Color(0.2f, 0.2f, 1f, 0f);
            
            defenseEffect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            float randomScale = Random.Range(effectScaleMin * 1.5f, effectScaleMax * 1.5f);
            defenseEffect.transform.localScale = Vector3.zero;

            Sequence defenseSequence = DOTween.Sequence();

            defenseSequence.Append(defenseEffect.transform.DOScale(randomScale, effectFadeInDuration * 0.5f)
                .SetEase(Ease.OutBack));
            defenseSequence.Join(spriteRenderer.DOFade(0.7f, effectFadeInDuration * 0.5f)
                .SetEase(Ease.OutQuart));

            defenseSequence.AppendInterval(effectStayDuration * 0.5f);

            defenseSequence.Append(defenseEffect.transform.DOScale(randomScale * 1.2f, effectFadeOutDuration)
                .SetEase(Ease.InBack));
            defenseSequence.Join(spriteRenderer.DOFade(0f, effectFadeOutDuration)
                .SetEase(Ease.InQuart));

            defenseSequence.OnComplete(() => Destroy(defenseEffect));
        }
    }

    public void HideSkillCards(GameObject profilePanel)
    {
        Transform skillCards = profilePanel.transform.Find("SkillCards");
        if (skillCards != null)
        {
            foreach (Transform card in skillCards)
            {
                card.gameObject.SetActive(false);
            }
        }
    }

    public void ShowSkillCards(GameObject profilePanel)
    {
        Transform skillCards = profilePanel.transform.Find("SkillCards");
        if (skillCards != null)
        {
            foreach (Transform card in skillCards)
            {
                card.gameObject.SetActive(true);
            }
        }
    }

    public void UpdateStatusEffectIcons(CharacterProfile character)
    {
        Transform statusIconsParent = character.transform.Find("StatusIcons");
        if (statusIconsParent == null)
        {
            Debug.LogError($"{character.name}에 StatusIcons가 없습니다!");
            return;
        }

        // 각 상태이상 스프라이트 렌더러 찾기
        SpriteRenderer bleedingIcon = statusIconsParent.Find("BleedingIcon")?.GetComponent<SpriteRenderer>();
        SpriteRenderer confusionIcon = statusIconsParent.Find("ConfusionIcon")?.GetComponent<SpriteRenderer>();
        SpriteRenderer poisonIcon = statusIconsParent.Find("PoisonIcon")?.GetComponent<SpriteRenderer>();
        SpriteRenderer defenseDownIcon = statusIconsParent.Find("DefenseDownIcon")?.GetComponent<SpriteRenderer>();

        Player player = character.GetPlayer;

        // 출혈 상태 아이콘
        if (bleedingIcon != null)
        {
            bleedingIcon.gameObject.SetActive(player.isBleedingEffect);
            if (player.isBleedingEffect)
            {
                TextMeshPro durationText = bleedingIcon.GetComponentInChildren<TextMeshPro>();
                if (durationText != null) durationText.text = player.bleedingTurns.ToString();
            }
        }

        // 혼란 상태 아이콘
        if (confusionIcon != null)
        {
            confusionIcon.gameObject.SetActive(player.isConfusionEffect);
            if (player.isConfusionEffect)
            {
                TextMeshPro durationText = confusionIcon.GetComponentInChildren<TextMeshPro>();
                if (durationText != null) durationText.text = player.confusionTurns.ToString();
            }
        }

        // 독 상태 아이콘
        if (poisonIcon != null)
        {
            poisonIcon.gameObject.SetActive(player.isPoisonEffect);
            if (player.isPoisonEffect)
            {
                TextMeshPro durationText = poisonIcon.GetComponentInChildren<TextMeshPro>();
                if (durationText != null) durationText.text = player.poisonTurns.ToString();
            }
        }

        // 방어력감소 상태 아이콘
        if (defenseDownIcon != null)
        {
            defenseDownIcon.gameObject.SetActive(player.isDefenseDownEffect);
            if (player.isDefenseDownEffect)
            {
                TextMeshPro durationText = defenseDownIcon.GetComponentInChildren<TextMeshPro>();
                if (durationText != null) durationText.text = player.defenseDownTurns.ToString();
            }
        }
    }
}