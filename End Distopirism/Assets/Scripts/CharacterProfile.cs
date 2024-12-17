using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class SkillHistory
{
    public string skillName;
    public int useCount;
    public bool isActive;
    public string lastUsedTime;  // 마지막 사용 시간 기록
}

[System.Serializable]
public class StatusEffectHistory
{
    public string effectName;
    public int occurCount;
    public bool isActive;
    public int maxDuration;  // 가장 길었던 지속시간
}

public class CharacterProfile : MonoBehaviour
{
    [SerializeField]
    private Player player;
    public Player GetPlayer => player;

    private SkillManager skillManager;

    public bool live;

    public int bonusDmg = 0;    //diff 차이에 따른 데미지 증가값

    public int coinBonus = 0; //코인 보너스
    public int successCount = 0;  //성공 횟수

    public bool isSelected = false; // 선택된 상태를 나타내는 변수

    private float initialYRotation;

    private GameObject healthBar;
    private GameObject mentalityBar;
    private Image healthFill;
    private Image mentalityFill;

    private Transform hpPosition; // HPPosition 트랜스폼 참조
    private Transform mtPosition; // MTPosition 트랜스폼 참조

    private Transform skillPosition; // SkillPosition 트랜스폼 참조
    private GameObject currentSkillEffect; // 현재 활성화된 스킬 이펙트

    private Skill selectedSkill; // 현재 선택된 스

    [Header("Sound Effects")]
    public AudioClip dashSound;  // 대시 사운드
    public AudioClip hitSound;   // 피해 사운드
    private AudioSource audioSource;  // 오디오 소스 컴포넌트

    private Image healthBarFill;
    private Image mentalityBarFill;
    private float currentHealthDisplay;
    private float currentMentalityDisplay;
    private float smoothSpeed = 5f;  // 감소 속도 조절값

    [Header("Skill History")]
    [SerializeField] private List<SkillHistory> skillHistories = new List<SkillHistory>();
    
    [Header("Status Effect History")]
    [SerializeField] private List<StatusEffectHistory> effectHistories = new List<StatusEffectHistory>();

    private bool hasDrawnCards = false; // 카드를 이미 뽑았는지 체크하는 변수 추가

    [Header("Battle Cards")]
    private List<Skill> drawnCards = new List<Skill>(); // 현재 뽑은 카드들
    private bool hasInitializedCards = false; // 카드가 초기화되었는지 확인하는 플래그

    private TextMeshProUGUI playerHPText;
    private TextMeshProUGUI playerMTText;
    private TextMeshProUGUI enemyHPText;
    private TextMeshProUGUI enemyMTText;

    [Header("Skill Card Objects")]
    [SerializeField] private Transform selectedSkillObject;  // SerializeField 추가
    [SerializeField] private Transform skillCard1Object;     // SerializeField 추가
    [SerializeField] private Transform skillCard2Object;     // SerializeField 추가
    [SerializeField] private Transform skillCard3Object;     // SerializeField 추가

    [Header("Skill Card Components")]
    private SpriteRenderer skillCard1Renderer;
    private SpriteRenderer skillCard2Renderer;
    private SpriteRenderer skillCard3Renderer;

    void Start()
    {
        player.coin = player.maxCoin;
        if (live == false)
        {
            if (player.hp > 0)
            {
                live = true;
            }
        }

        initialYRotation = transform.rotation.eulerAngles.y;

        // HP와 MT Position 찾기
        hpPosition = transform.Find("HPPosition");
        mtPosition = transform.Find("MTPosition");
        
        if (hpPosition == null)
        {
            Debug.LogError($"캐릭터 {gameObject.name}에서 HPPosition을 찾을 수 없습니.");
            return;
        }
        if (mtPosition == null)
        {
            Debug.LogError($"캐릭터 {gameObject.name}에서 MTPosition을 찾을 수 없습니다.");
            return;
        }

        // DOTween 애니메이션이 완료된 후 상태바 생성
        StartCoroutine(WaitForScaleAndCreateBars());

        // SkillPosition 찾기
        skillPosition = transform.Find("SkillPosition");
        if (skillPosition == null)
        {
            Debug.LogError($"캐릭터 {gameObject.name}에서 SkillPosition을 찾을 수 없습니다.");
        }

        // SkillManager 찾기
        skillManager = GameObject.FindObjectOfType<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError($"캐릭터 {gameObject.name}에서 SkillManager를 찾을 수 없습니다.");
        }

        // AudioSource 컴포넌트 추가
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;  // 2D 사운드로 설정

        // 체력바와 정신력바 초기화
        Transform healthBar = transform.Find("HealthBar");
        Transform mentalityBar = transform.Find("MentalityBar");
        
        if (healthBar != null)
            healthBarFill = healthBar.Find("Fill").GetComponent<Image>();
        
        if (mentalityBar != null)
            mentalityBarFill = mentalityBar.Find("Fill").GetComponent<Image>();

        // 현재 표시값 초기화
        currentHealthDisplay = GetPlayer.hp;
        currentMentalityDisplay = GetPlayer.menTality;

        // 캐릭터 스프라이트를 약간 회전시켜 3/4 뷰 효과를 줍니다
        //if (CompareTag("Player"))
        //{
            // 플레이어 캐릭터는 오른쪽으로 20도 회전
        //    transform.rotation = Quaternion.Euler(0, 20f, 0);
        //}
        //else if (CompareTag("Enemy"))
        //{
            // 적 캐릭터는 왼쪽으로 20도 회전
        //    transform.rotation = Quaternion.Euler(0, 20f, 0);
        //}

        // 참조가 없는 경우에만 자식 오브젝트에서 찾기
        if (selectedSkillObject == null)
            selectedSkillObject = transform.Find("SelectedSkill");
        if (skillCard1Object == null)
            skillCard1Object = transform.Find("SkillCard1");
        if (skillCard2Object == null)
            skillCard2Object = transform.Find("SkillCard2");
        if (skillCard3Object == null)
            skillCard3Object = transform.Find("SkillCard3");

        // 찾지 못한 경우 경고
        if (selectedSkillObject == null) 
            Debug.LogError($"{gameObject.name}에서 SelectedSkill 오브젝트를 찾을 수 없습니다.");
        if (skillCard1Object == null) 
            Debug.LogError($"{gameObject.name}에서 SkillCard1 오브젝트를 찾을 수 없습니다.");
        if (skillCard2Object == null) 
            Debug.LogError($"{gameObject.name}에서 SkillCard2 오브젝트를 찾을 수 없습니다.");
        if (skillCard3Object == null) 
            Debug.LogError($"{gameObject.name}에서 SkillCard3 오브젝트를 찾을 수 없습니다.");

        // 초기화 로그 추가
        Debug.LogWarning($"[{gameObject.name}] 스킬 카드 참조 상태:");
        Debug.LogWarning($"- SelectedSkill: {(selectedSkillObject != null ? "참조됨" : "참조 없음")}");
        Debug.LogWarning($"- SkillCard1: {(skillCard1Object != null ? "참조됨" : "참조 없음")}");
        Debug.LogWarning($"- SkillCard2: {(skillCard2Object != null ? "참조됨" : "참조 없음")}");
        Debug.LogWarning($"- SkillCard3: {(skillCard3Object != null ? "참조됨" : "참조 없음")}");

        InitializeCardRenderers();

        // 초기에는 모든 카드 오브젝트 비활성화
        HideCardObjects();
        if (selectedSkillObject != null)
            selectedSkillObject.gameObject.SetActive(false);

        // 마우스 오버 이벤트를 위한 컴포넌트 추가
        if (CompareTag("Enemy"))
        {
            // BoxCollider가 없다면 추가
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f, 0.1f); // 적절한 크기로 조정
                boxCollider.isTrigger = true;
            }

            // MouseOverHandler 추가
            MouseOverHandler mouseHandler = gameObject.AddComponent<MouseOverHandler>();
            mouseHandler.Initialize(this);
        }
    }

    private void InitializeCardRenderers()
    {
        // 각 카드 오브젝트의 SpriteRenderer 컴포넌트 가져오기
        if (skillCard1Object != null)
            skillCard1Renderer = skillCard1Object.GetComponent<SpriteRenderer>();
        if (skillCard2Object != null)
            skillCard2Renderer = skillCard2Object.GetComponent<SpriteRenderer>();
        if (skillCard3Object != null)
            skillCard3Renderer = skillCard3Object.GetComponent<SpriteRenderer>();

        // 각 카드에 클릭 이벤트 추가
        if (skillCard1Object != null)
        {
            BoxCollider clickCollider = skillCard1Object.gameObject.AddComponent<BoxCollider>();
            clickCollider.size = new Vector3(1f, 1f, 0.1f);
            CardClickHandler clickHandler = skillCard1Object.gameObject.AddComponent<CardClickHandler>();
            clickHandler.Initialize(this, 0);
        }
        if (skillCard2Object != null)
        {
            BoxCollider clickCollider = skillCard2Object.gameObject.AddComponent<BoxCollider>();
            clickCollider.size = new Vector3(1f, 1f, 0.1f);
            CardClickHandler clickHandler = skillCard2Object.gameObject.AddComponent<CardClickHandler>();
            clickHandler.Initialize(this, 1);
        }
        if (skillCard3Object != null)
        {
            BoxCollider clickCollider = skillCard3Object.gameObject.AddComponent<BoxCollider>();
            clickCollider.size = new Vector3(1f, 1f, 0.1f);
            CardClickHandler clickHandler = skillCard3Object.gameObject.AddComponent<CardClickHandler>();
            clickHandler.Initialize(this, 2);
        }

        // 선택한 스킬 오브젝트에 클릭 이벤트 추가
        if (selectedSkillObject != null)
        {
            BoxCollider clickCollider = selectedSkillObject.gameObject.AddComponent<BoxCollider>();
            clickCollider.size = new Vector3(1f, 1f, 0.1f);
            CardClickHandler clickHandler = selectedSkillObject.gameObject.AddComponent<CardClickHandler>();
            clickHandler.Initialize(this);
        }
    }

    private IEnumerator WaitForScaleAndCreateBars()
    {
        // 캐릭터의 스케일이 0이 아닐 때까지 대기
        while (transform.localScale.magnitude < 1f)
        {
            yield return null;
        }

        // 약간의 추가 딜레이
        yield return new WaitForSeconds(0.2f);

        // 체력바와 정신력바 생성
        CreateStatusBars();
        UpdateStatusBars();
    }

    private void CreateStatusBars()
    {
        // Canvas2 찾기
        GameObject canvas2 = GameObject.Find("Canvas2");
        if (canvas2 == null)
        {
            Debug.LogError("Canvas2를 찾을 수 없습니다!");
            return;
        }

        // 체력바 생성
        healthBar = Instantiate(UIManager.Instance.healthBarPrefab, hpPosition.position, Quaternion.identity, canvas2.transform);
        healthFill = healthBar.transform.Find("Fill").GetComponent<Image>();
        
        // Canvas 설정
        Canvas healthCanvas = healthBar.GetComponent<Canvas>();
        if (healthCanvas != null)
        {
            healthCanvas.renderMode = RenderMode.WorldSpace;
            healthCanvas.worldCamera = Camera.main;
            healthCanvas.sortingOrder = 3;
            
            RectTransform rectTransform = healthBar.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 10f);
        }

        // 체력바 초기 설정
        if (healthFill != null)
        {
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillAmount = (float)player.hp / player.maxHp;
        }

        // 정신력바 생성
        mentalityBar = Instantiate(UIManager.Instance.mentalityBarPrefab, mtPosition.position, Quaternion.identity, canvas2.transform);
        mentalityFill = mentalityBar.transform.Find("Fill").GetComponent<Image>();
        
        Canvas mentalityCanvas = mentalityBar.GetComponent<Canvas>();
        if (mentalityCanvas != null)
        {
            mentalityCanvas.renderMode = RenderMode.WorldSpace;
            mentalityCanvas.worldCamera = Camera.main;
            mentalityCanvas.sortingOrder = 1;
            
            RectTransform rectTransform = mentalityBar.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 10f);
        }

        // 정신력바 초기 설정
        if (mentalityFill != null)
        {
            mentalityFill.type = Image.Type.Filled;
            mentalityFill.fillMethod = Image.FillMethod.Horizontal;
            mentalityFill.fillAmount = player.menTality / 100f;
        }

        // 력바 생성 후 텍스트 컴포넌트 찾기
        playerHPText = healthBar.transform.Find("PlayerHP")?.GetComponent<TextMeshProUGUI>();
        enemyHPText = healthBar.transform.Find("EnemyHP")?.GetComponent<TextMeshProUGUI>();

        // 정신력바 생성 후 텍스트 컴포넌트 찾기
        playerMTText = mentalityBar.transform.Find("PlayerMT")?.GetComponent<TextMeshProUGUI>();
        enemyMTText = mentalityBar.transform.Find("EnemyMT")?.GetComponent<TextMeshProUGUI>();

        // 태그 따라 적절한 텍스트 활성화/비활성화
        if (CompareTag("Player"))
        {
            if (enemyHPText) enemyHPText.gameObject.SetActive(false);
            if (enemyMTText) enemyMTText.gameObject.SetActive(false);
        }
        else if (CompareTag("Enemy"))
        {
            if (playerHPText) playerHPText.gameObject.SetActive(false);
            if (playerMTText) playerMTText.gameObject.SetActive(false);
        }

        // 초기 텍스트 업데이트
        UpdateStatusTexts();
    }

    private void UpdateStatusBars()
    {
        // 체력바 업데이트
        if (healthFill != null)
        {
            float targetHealth = Mathf.Clamp01((float)player.hp / player.maxHp);
            DOTween.To(() => healthFill.fillAmount, x => healthFill.fillAmount = x, targetHealth, 0.5f)
                .SetEase(Ease.OutQuad);
        }

        // 정신력바 업데이트
        if (mentalityFill != null)
        {
            float targetMentality = Mathf.Clamp01(player.menTality / 100f);
            DOTween.To(() => mentalityFill.fillAmount, x => mentalityFill.fillAmount = x, targetMentality, 0.5f)
                .SetEase(Ease.OutQuad);
        }
    }

    // 체력이나 정신력이 변경될 때 호출할 메서드
    public void UpdateStatus()
    {
        // 체력이 0 미만으로 내려가지 않도록
        GetPlayer.hp = Mathf.Max(0, GetPlayer.hp);
        
        // 정신력이 0~100 범위를 벗어나지 않도록
        GetPlayer.menTality = Mathf.Clamp(GetPlayer.menTality, 0f, 100f);

        // 체력바 색상 업데이트 및 피해 효과
        if (healthBarFill != null)
        {
            float healthPercentage = (float)GetPlayer.hp / GetPlayer.maxHp;
            healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
        }

        // 정신력바 색상 업데이트
        if (mentalityBarFill != null)
        {
            float mentalityPercentage = GetPlayer.menTality / 100f;
            mentalityBarFill.color = Color.Lerp(Color.red, Color.blue, mentalityPercentage);
        }

        // 텍스트 업데이트
        UpdateStatusTexts();
    }

    public void ShowCharacterInfo()
    {
        UIManager.Instance.ShowCharacterInfo(this, selectedSkill != null);
        
        if (CompareTag("Player") && BattleManager.Instance.state == GameState.playerTurn)
        {
            // 항상 선택된 스킬 오브젝트를 비활성화하고 카드 3장을 표시
            if (selectedSkillObject != null)
                selectedSkillObject.gameObject.SetActive(false);
            
            // 새로운 턴에서는 이전 선택을 초기화
            selectedSkill = null;
            
            // 카드 1,2,3 표시
            ShowCardObjects();
            UpdateCardSprites();
            
            Debug.LogWarning($"{GetPlayer.charName}의 카드 선택 UI 표시 (새 턴)");
        }
    }

    // 스킬 카드를 숨기는 메서드 추가
    public void HideSkillCards()
    {
        UIManager.Instance.HideSkillCards(this.CompareTag("Player") ? UIManager.Instance.playerProfilePanel : UIManager.Instance.enemyProfilePanel);
    }

    public void ShowSkillCards()
    {
        UIManager.Instance.ShowSkillCards(this.CompareTag("Player") ? UIManager.Instance.playerProfilePanel : UIManager.Instance.enemyProfilePanel);
    }

    public void OnDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 상태바 페이드 아웃
        if (healthBar != null)
        {
            CanvasGroup healthBarCanvas = healthBar.GetComponent<CanvasGroup>();
            if (healthBarCanvas == null)
            {
                healthBarCanvas = healthBar.AddComponent<CanvasGroup>();
            }
            DOTween.To(() => healthBarCanvas.alpha, x => healthBarCanvas.alpha = x, 0f, 1f);
        }

        if (mentalityBar != null)
        {
            CanvasGroup mentalityBarCanvas = mentalityBar.GetComponent<CanvasGroup>();
            if (mentalityBarCanvas == null)
            {
                mentalityBarCanvas = mentalityBar.AddComponent<CanvasGroup>();
            }
            DOTween.To(() => mentalityBarCanvas.alpha, x => mentalityBarCanvas.alpha = x, 0f, 1f);
        }

        // 캐릭터 스프라이트 페이드 아웃
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 이드 아이 시퀀스 생성
            Sequence deathSequence = DOTween.Sequence();
            
            // 캐릭터를 위로 살짝 띄우면서 페이드 아웃
            deathSequence.Join(transform.DOMoveY(transform.position.y + 1f, 1f).SetEase(Ease.OutQuad));
            deathSequence.Join(spriteRenderer.DOFade(0f, 1f).SetEase(Ease.InQuad));
            
            // 캐릭터 회전 효과 추가
            deathSequence.Join(transform.DORotate(new Vector3(0f, 360f, 0f), 1f, RotateMode.FastBeyond360));
            
            // 크기 줄이기 효과
            deathSequence.Join(transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack));

            yield return deathSequence.WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // 스킬 이펙트 제거
        DestroySkillEffect();

        // 상태바 완전히 제거
        if (healthBar != null) Destroy(healthBar);
        if (mentalityBar != null) Destroy(mentalityBar);

        // 캐릭터 비활성화
        gameObject.SetActive(false);
    }

    void Update()
    {
        // 캐릭터가 카메라의 X 회전 값만 따라가도록 설정
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = mainCamera.transform.eulerAngles.x; // X 회전 카메라를 따라감
            euler.y = mainCamera.transform.eulerAngles.y; // Y 회전 카메라를 따라감
            euler.z = mainCamera.transform.eulerAngles.z; // Z 회전 카메라를 ��라감
            // Y축과 Z축은 현재 값을 유지
            transform.rotation = Quaternion.Euler(euler);
        }

        // 상태바 위치 회전 업데이트
        if (healthBar != null && mentalityBar != null)
        {
            healthBar.transform.position = hpPosition.position;
            mentalityBar.transform.position = mtPosition.position;
            
            healthBar.transform.rotation = Camera.main.transform.rotation;
            mentalityBar.transform.rotation = Camera.main.transform.rotation;

            // 상태바 값 업데이트
            UpdateStatusBars();

            // 텍스트 업데이트 추가
            UpdateStatusTexts();
        }

        // 스킬 이펙트 치와 회전 업데이
        if (currentSkillEffect != null && skillPosition != null)
        {
            currentSkillEffect.transform.position = skillPosition.position;
            currentSkillEffect.transform.rotation = Camera.main.transform.rotation;
        }

        // 체력 부드러운 감소 효과
        float targetHealth = (float)GetPlayer.hp / GetPlayer.maxHp;
        currentHealthDisplay = Mathf.Lerp(currentHealthDisplay, targetHealth, Time.deltaTime * smoothSpeed);
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealthDisplay;
        }

        // 정신력 부드러운 감소 효과
        float targetMentality = GetPlayer.menTality / 100f;
        currentMentalityDisplay = Mathf.Lerp(currentMentalityDisplay, targetMentality, Time.deltaTime * smoothSpeed);
        if (mentalityBarFill != null)
        {
            mentalityBarFill.fillAmount = currentMentalityDisplay;
        }
    }

    // 캐릭터가 데미지를 받을 때 호출할 메서드
    public void TakeDamage(int damage)
    {
        player.hp = Mathf.Max(0, player.hp - damage);
        UpdateStatusBars();
    }

    // 정신력이 변경될 때 호출할 메서드
    public void UpdateMentality(float newMentality)
    {
        float oldMentality = player.menTality;
        player.menTality = Mathf.Clamp(newMentality, 0f, 100f);
        
        // 값 실제로 변경되었 때만 업데이트
        if (oldMentality != player.menTality)
        {
            UpdateStatusBars();
            Debug.Log($"정신력 변경: {oldMentality} -> {player.menTality}");
        }
    }

    // 스킬 이펙트 생성 및 표시
    public void ShowSkillEffect(int initialDamage)
    {
        GameObject canvas2 = GameObject.Find("Canvas2");
        if (skillPosition == null) return;

        if (currentSkillEffect != null)
        {
            Destroy(currentSkillEffect);
        }

        GameObject prefab = CompareTag("Player") ? 
            UIManager.Instance.playerSkillEffectPrefab : 
            UIManager.Instance.enemySkillEffectPrefab;

        currentSkillEffect = Instantiate(prefab, skillPosition.position, Quaternion.identity, UIManager.Instance.canvas2.transform);
        
        Canvas skillCanvas = currentSkillEffect.GetComponent<Canvas>();
        if (skillCanvas != null)
        {
            skillCanvas.renderMode = RenderMode.WorldSpace;
            skillCanvas.worldCamera = Camera.main;
            skillCanvas.sortingOrder = 5;
        }
        
        // TMP 컴포넌트 참조
        TextMeshProUGUI dmgText = currentSkillEffect.transform.Find("DmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI skillText = currentSkillEffect.transform.Find("SkillText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI coinText = currentSkillEffect.transform.Find("CoinText").GetComponent<TextMeshProUGUI>();
        
        if (dmgText != null)
        {
            dmgText.text = initialDamage.ToString();
        }
        
        if (skillText != null)
        {
            skillText.text = selectedSkill != null ? selectedSkill.skillName : "기본 공격";
        }

        if (coinText != null)
        {
            coinText.text = successCount.ToString();
        }

        currentSkillEffect.transform.position = skillPosition.position;
    }

    public void UpdateSkillEffectDamage(int newDamage)
    {
        if (currentSkillEffect != null)
        {
            TextMeshProUGUI dmgText = currentSkillEffect.transform.Find("DmgText").GetComponent<TextMeshProUGUI>();
            if (dmgText != null)
            {
                StartCoroutine(AnimateDamageTextUpdate(dmgText, newDamage));
            }
        }
    }

    public void UpdateCoinCount(int count)
    {
        if (currentSkillEffect != null)
        {
            TextMeshProUGUI coinText = currentSkillEffect.transform.Find("CoinText").GetComponent<TextMeshProUGUI>();
            if (coinText != null)
            {
                StartCoroutine(AnimateCoinTextUpdate(coinText, count));
            }
        }
    }

    private IEnumerator AnimateDamageTextUpdate(TextMeshProUGUI dmgText, int newDamage)
    {
        float duration = 0.4f;
        float elapsedTime = 0f;
        int startDamage = int.Parse(dmgText.text);
        Vector3 originalScale = dmgText.transform.localScale; ; // 원본 스케일을 68로 고정
        Color originalColor = dmgText.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 스케일 애니메이션
            float scaleFactor = 1f + Mathf.Sin(progress * Mathf.PI) * 0.4f; // 스케일 변화 폭을 0.2로 줄임
            dmgText.transform.localScale = originalScale * scaleFactor;
            
            // 색상 변경
            dmgText.color = Color.Lerp(Color.red, originalColor, progress);
            
            // 숫자 업데이트
            int currentDamage = (int)Mathf.Lerp(startDamage, newDamage, progress);
            dmgText.text = currentDamage.ToString();
            
            yield return null;
        }
        
        // 최종 상태로 설정
        dmgText.transform.localScale = originalScale; // 스케일을 원래대 복구
        dmgText.color = originalColor;
        dmgText.text = newDamage.ToString();
    }

    private IEnumerator AnimateCoinTextUpdate(TextMeshProUGUI coinText, int count)
    {
        float duration = 0.4f;
        Vector3 originalScale = coinText.transform.localScale;
        Color originalColor = coinText.color;
        
        // 텍스트 업데이트 - 숫자만 표시
        coinText.text = count.ToString();
        
        // 크기 확대 및 색상 변경
        coinText.transform.DOScale(originalScale * 1.4f, duration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                coinText.transform.DOScale(originalScale, duration * 0.5f)
                    .SetEase(Ease.InQuad);
            });
        
        // 색상 애니메이션
        Sequence colorSequence = DOTween.Sequence();
        colorSequence.Append(DOTween.To(() => coinText.color, x => coinText.color = x, Color.red, duration * 0.5f))
                    .Append(DOTween.To(() => coinText.color, x => coinText.color = x, originalColor, duration * 0.5f));
        
        yield return new WaitForSeconds(duration);
    }

    // 스킬 이펙트 제거 메서드 추가
    public void DestroySkillEffect()
    {
        if (currentSkillEffect != null)
        {
            Destroy(currentSkillEffect);
            currentSkillEffect = null;
        }
    }

    public void SelectSkill(Skill skill, bool showPanel = true)
    {
        selectedSkill = skill;
        ApplySelectedSkill();
        
        // 카드 UI 상태 복귀
        ReturnToSelectedSkill();
        
        if (showPanel)
        {
            UIManager.Instance.ShowCharacterInfo(this, true);
        }
        
        UpdateSkillHistory(skill.skillName);
        Debug.LogWarning($"[스킬 선택] {player.charName}이(가) {skill.skillName} 스킬을 선택습니다.");
    }

    private void ApplySelectedSkill()
    {
        if (selectedSkill != null)
        {
            // 이전 스탯 저장
            int previousMinDmg = player.minDmg;
            int previousDmgUp = player.dmgUp;
            
            // 새로운 스킬 스탯 적용
            player.minDmg = selectedSkill.minDmg;
            player.dmgUp = selectedSkill.dmgUp;
            
            // 현재 선택된 스킬을 플레이어의 첫 번째 스킬로 설정
            if (player.skills == null)
            {
                player.skills = new List<Skill>();
            }
            if (player.skills.Count > 0)
            {
                player.skills[0] = selectedSkill;
            }
            else
            {
                player.skills.Add(selectedSkill);
            }
            
            // 스탯 변경 그 출력
            Debug.Log($"[스킬 적용] 캐릭터: {player.charName}, 스킬: {selectedSkill.skillName}");
            Debug.Log($"[스킬 스탯 변경] 최소 데미지: {previousMinDmg} -> {selectedSkill.minDmg} ({selectedSkill.minDmg - previousMinDmg:+#;-#;0})");
            Debug.Log($"[스킬 스탯 변경] 데미지 증가: {previousDmgUp} -> {selectedSkill.dmgUp} ({selectedSkill.dmgUp - previousDmgUp:+#;-#;0})");
        }
    }

    private void UpdateSkillHistory(string skillName)
    {
        var existingHistory = skillHistories.Find(h => h.skillName == skillName);
        if (existingHistory != null)
        {
            existingHistory.useCount++;
            existingHistory.isActive = true;
            existingHistory.lastUsedTime = System.DateTime.Now.ToString();
        }
        else
        {
            skillHistories.Add(new SkillHistory
            {
                skillName = skillName,
                useCount = 1,
                isActive = true,
                lastUsedTime = System.DateTime.Now.ToString()
            });
        }
    }

    // 상태이상 히스토리 업데이트 메서드
    private void UpdateEffectHistory(string effectName, int duration)
    {
        var existingHistory = effectHistories.Find(h => h.effectName == effectName);
        if (existingHistory != null)
        {
            existingHistory.occurCount++;
            existingHistory.isActive = true;
            existingHistory.maxDuration = Mathf.Max(existingHistory.maxDuration, duration);
            Debug.Log($"[상태이상 기록 갱신] {effectName}: 발생 {existingHistory.occurCount}회, 최대 지속 {existingHistory.maxDuration}턴");
        }
        else
        {
            effectHistories.Add(new StatusEffectHistory
            {
                effectName = effectName,
                occurCount = 1,
                isActive = true,
                maxDuration = duration
            });
            Debug.Log($"[상태이상 기록 생성] {effectName}: 최초 발생");
        }
    }

    // 스킬 효과 종료 시 호출
    public void DeactivateSkill(string skillName)
    {
        var history = skillHistories.Find(h => h.skillName == skillName);
        if (history != null)
        {
            history.isActive = false;
        }
    }


    // 대시 사운드 재생 메서드 추가
    public void PlayDashSound()
    {
        if (audioSource != null && dashSound != null)
        {
            audioSource.clip = dashSound;
            audioSource.Play();
        }
    }

    // 피해 사운드 재생 메서드 추가
    public void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.clip = hitSound;
            audioSource.Play();
        }
    }



    // 턴이 끝날 때 호될 메서드 추가
    public void OnTurnEnd()
    {
        hasDrawnCards = false;
        selectedSkill = null;
        hasInitializedCards = false; // 다음 턴을 위해 초기화 플래그 리셋
        InitializeRandomCards(); // 새로운 카드 세트 뽑기
    }

    // 적 캐릭터의 초기 스킬 설정
    public void InitializeEnemySkill()
    {
        if (CompareTag("Enemy") && player.skills != null && player.skills.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, player.skills.Count);
            // 널을 표시하지 않도록 false 전달
            SelectSkill(player.skills[randomIndex], false);
            Debug.LogWarning($"[적 초기 스킬] {player.charName}이(가) {player.skills[randomIndex].skillName} 스킬을 선택");
        }
    }

    // 적 캐릭터의 턴마다 새로운 스킬 선택
    public void SelectRandomEnemySkill()
    {
        if (CompareTag("Enemy") && player.skills != null && player.skills.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, player.skills.Count);
            // 패널을 표시하지 않도록 false 전달
            SelectSkill(player.skills[randomIndex], false);
            Debug.LogWarning($"[적 턴 스킬] {player.charName}이(가) {player.skills[randomIndex].skillName} 스킬을 선택");
        }
    }

    // 드 초기화 및 랜덤 드로우 메서드
    public void InitializeRandomCards()
    {
        if (hasInitializedCards) return;

        drawnCards.Clear();
        List<Skill> availableSkills = new List<Skill>(player.skills);

        // 3장의 카드를 랜덤하게 뽑음
        for (int i = 0; i < 3 && availableSkills.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableSkills.Count);
            drawnCards.Add(availableSkills[randomIndex]);
            availableSkills.RemoveAt(randomIndex);
        }

        hasInitializedCards = true;
        
        // 카드 스프라이트 업데이트
        UpdateCardSprites();
        
        Debug.LogWarning($"{player.charName}의 카드 초기화: {string.Join(", ", drawnCards.Select(s => s.skillName))}");
    }

    // 현재 뽑은 카드 리스트 반환
    public List<Skill> GetDrawnCards()
    {
        return drawnCards;
    }


    // 새운 메서드 추가: 상태 텍스트 업데이트
    private void UpdateStatusTexts()
    {
        if (CompareTag("Player"))
        {
            if (playerHPText) playerHPText.text = player.hp.ToString();
            if (playerMTText) playerMTText.text = $"{Mathf.RoundToInt(player.menTality)}%";
        }
        else if (CompareTag("Enemy"))
        {
            if (enemyHPText) enemyHPText.text = player.hp.ToString();
            if (enemyMTText) enemyMTText.text = $"{Mathf.RoundToInt(player.menTality)}%";
        }
    }

    public void UpdateCardSprites()
    {
        List<Skill> currentCards = GetDrawnCards();
        
        // 모든 카드에 sprite 사용 (nomalSprite 대신)
        if (currentCards.Count > 0 && skillCard1Renderer != null)
        {
            skillCard1Renderer.sprite = currentCards[0].sprite;  // nomalSprite 대신 sprite 사용
            Debug.LogWarning($"카드1 스프라이트 설정: {currentCards[0].skillName} (sprite)");
        }
        
        if (currentCards.Count > 1 && skillCard2Renderer != null)
        {
            skillCard2Renderer.sprite = currentCards[1].sprite;  // nomalSprite 대신 sprite 사용
            Debug.LogWarning($"카드2 스프라이트 설정: {currentCards[1].skillName} (sprite)");
        }
        
        if (currentCards.Count > 2 && skillCard3Renderer != null)
        {
            skillCard3Renderer.sprite = currentCards[2].sprite;  // nomalSprite 대신 sprite 사용
            Debug.LogWarning($"카드3 스프라이트 설정: {currentCards[2].skillName} (sprite)");
        }

        // 선택된 스킬이 있다면 해당 스킬의 sprite 사용 (이미 sprite 사용 중)
        if (selectedSkill != null && selectedSkillObject != null)
        {
            SpriteRenderer selectedRenderer = selectedSkillObject.GetComponent<SpriteRenderer>();
            if (selectedRenderer != null)
            {
                selectedRenderer.sprite = selectedSkill.sprite;
                Debug.LogWarning($"선택된 스킬 스프라이트 설정: {selectedSkill.skillName} (sprite)");
            }
        }
    }

    // 카드 1,2,3을 숨기는 메서드
    private void HideCardObjects()
    {
        if (skillCard1Object == null) return;

        Vector3 card1LocalPosition = skillCard1Object.transform.localPosition;

        // Card2와 Card3를 Card1의 위치로 모으기 (로컬 좌표 사용)
        if (skillCard2Object != null && skillCard2Object.gameObject.activeSelf)
        {
            skillCard2Object.transform.DOLocalMove(card1LocalPosition, 0.2f)
                .SetEase(Ease.InQuart)
                .OnComplete(() => skillCard2Object.gameObject.SetActive(false));
        }
        if (skillCard3Object != null && skillCard3Object.gameObject.activeSelf)
        {
            skillCard3Object.transform.DOLocalMove(card1LocalPosition, 0.2f)
                .SetEase(Ease.InQuart)
                .OnComplete(() => skillCard3Object.gameObject.SetActive(false));
        }

        // Card1은 약간 딜레이 후 비활성화
        if (skillCard1Object.gameObject.activeSelf)
        {
            DOVirtual.DelayedCall(0.2f, () => skillCard1Object.gameObject.SetActive(false));
        }

        Debug.LogWarning("카드 모으기 애니메이션 시작");
    }

    // 카드 1,2,3을 표시하는 메서드
    private void ShowCardObjects()
    {
        if (!CompareTag("Player") || BattleManager.Instance.state != GameState.playerTurn)
            return;

        // 먼저 모든 카드를 Card1의 위치에 배치
        Vector3 card1LocalPosition = skillCard1Object.transform.localPosition;
        if (skillCard2Object != null)
            skillCard2Object.transform.localPosition = card1LocalPosition;
        if (skillCard3Object != null)
            skillCard3Object.transform.localPosition = card1LocalPosition;

        // 카드 활성화
        if (skillCard1Object != null)
        {
            skillCard1Object.gameObject.SetActive(true);
        }
        if (skillCard2Object != null)
        {
            skillCard2Object.gameObject.SetActive(true);
            // Card2를 왼쪽으로 이동 (로컬 좌표 사��)
            skillCard2Object.transform.DOLocalMove(card1LocalPosition + new Vector3(-0.9f, 0f, 0f), 0.3f)
                .SetEase(Ease.OutQuart);
        }
        if (skillCard3Object != null)
        {
            skillCard3Object.gameObject.SetActive(true);
            // Card3를 오른쪽으로 이동 (로컬 좌표 사용)
            skillCard3Object.transform.DOLocalMove(card1LocalPosition + new Vector3(0.9f, 0f, 0f), 0.3f)
                .SetEase(Ease.OutQuart);
        }

        Debug.LogWarning("카드 펼치기 애니메이션 시작");
    }

    // 선택한 스킬 카드 클릭 이벤트 처리
    public void OnSelectedSkillClicked()
    {
        if (selectedSkillObject != null)
        {
            selectedSkillObject.gameObject.SetActive(false);
            ShowCardObjects();
            UpdateCardSprites(); // 카드 스프라이트 업데이트
        }
    }

    // 카드 선택 후 원래 상태로 돌아가기
    public void ReturnToSelectedSkill()
    {
        HideCardObjects();
        if (selectedSkillObject != null)
            selectedSkillObject.gameObject.SetActive(true);
    }

    // 스킬 카드 1,2,3 중 하나가 선택되었을 때 호출
    public void OnSkillCardSelected(int cardIndex)
    {
        List<Skill> currentCards = GetDrawnCards();
        if (cardIndex < 0 || cardIndex >= currentCards.Count) return;

        // 선택된 스킬 설정
        Skill selectedCard = currentCards[cardIndex];
        
        // 선택한 스킬 오브젝트의 스프라이트 업데이트
        SpriteRenderer selectedRenderer = selectedSkillObject?.GetComponent<SpriteRenderer>();
        if (selectedRenderer != null)
        {
            selectedRenderer.sprite = selectedCard.sprite;
        }

        // 스킬 선택 처리
        SelectSkill(selectedCard);
        
        // 카드 UI 상태 복귀
        ReturnToSelectedSkill();

        // 적 선택 모드로 전환
        BattleManager.Instance.OnSkillSelected();

        Debug.LogWarning($"카드 {cardIndex + 1} 선택됨: {selectedCard.skillName}");
    }

    // 캐릭터 선택 해제 시 호출되는 메서드
    public void OnDeselected()
    {
        if (CompareTag("Player"))
        {
            HideCardObjects();
            if (selectedSkillObject != null)
                selectedSkillObject.gameObject.SetActive(false);
        }
    }

    // 전투 시작 시 호출될 메서드 추가
    public void OnBattleStart()
    {
        // 모든 카드 비활성화
        HideCardObjects();
        if (selectedSkillObject != null)
            selectedSkillObject.gameObject.SetActive(false);
        
        selectedSkill = null;
        Debug.LogWarning($"{GetPlayer.charName}의 카드 상태 초기화 (전투 시작)");
    }

    // 전투 종료 시 호출될 메서드 추가
    public void OnBattleEnd()
    {
        // 모든 카드 비활성화
        HideCardObjects();
        if (selectedSkillObject != null)
            selectedSkillObject.gameObject.SetActive(false);
        
        // 상태 초기화
        selectedSkill = null;
        hasDrawnCards = false;
        hasInitializedCards = false;
        drawnCards.Clear();
        
        Debug.LogWarning($"{GetPlayer.charName}의 카드 상태 초기화 (전투 종료)");
    }

    // PlayerAttackButton에서 호출될 메서드 추가
    public void OnAttackStart()
    {
        // 모든 카드 비활성화
        HideCardObjects();
        if (selectedSkillObject != null)
            selectedSkillObject.gameObject.SetActive(false);
        
        Debug.LogWarning($"{GetPlayer.charName}의 카드 숨 (공격 시작)");
    }

    // 선택된 스킬 오브젝트의 위치를 반환하는 public 메서드
    public Vector3? GetSelectedSkillPosition()
    {
        if (selectedSkillObject != null)
            return selectedSkillObject.position;
        return null;
    }

    // 스킬 선택을 완전히 초기화하는 메서��� 추가
    public void ResetSkillSelection()
    {
        selectedSkill = null;
        
        // 선택된 스킬 카드 비활성화
        if (selectedSkillObject != null)
            selectedSkillObject.gameObject.SetActive(false);
        
        // 카드들 초기 상태로
        ShowCardObjects();
        UpdateCardSprites();
        
        Debug.LogWarning($"{GetPlayer.charName}의 스킬 선택 초기화");
    }
}

public class MouseOverHandler : MonoBehaviour
{
    private CharacterProfile characterProfile;
    private bool isMouseOver = false;

    public void Initialize(CharacterProfile profile)
    {
        characterProfile = profile;
    }

    private void OnMouseEnter()
    {
        if (characterProfile == null || 
            BattleManager.Instance.state == GameState.win || 
            BattleManager.Instance.state == GameState.lose ||
            BattleManager.Instance.targetObjects.Contains(characterProfile)) // 이미 선택된 적인지 확인
        {
            return;
        }

        isMouseOver = true;
        characterProfile.ShowCharacterInfo();
        Debug.LogWarning($"마우스 오버: {characterProfile.GetPlayer.charName}의 정보 표시");
    }

    private void OnMouseExit()
    {
        if (characterProfile == null || 
            BattleManager.Instance.state == GameState.win || 
            BattleManager.Instance.state == GameState.lose)
        {
            return;
        }

        isMouseOver = false;
        
        // 약간의 딜레이 후에 패널 비활성화 체크
        StartCoroutine(CheckPanelDeactivation());
    }

    private IEnumerator CheckPanelDeactivation()
    {
        // 0.1초 대기
        yield return new WaitForSeconds(0.1f);

        // 마우스가 벗어난 상태이고, 해당 적이 선택되지 않은 경우에만 패널 비활성화
        if (!isMouseOver && !BattleManager.Instance.targetObjects.Contains(characterProfile))
        {
            UIManager.Instance.enemyProfilePanel.SetActive(false);
            Debug.LogWarning($"마우스 아웃: {characterProfile.GetPlayer.charName}의 정보 숨김");
        }
    }

    private void OnDisable()
    {
        // 컴포넌트가 비활성화될 때 패널도 비활성화
        if (!BattleManager.Instance.targetObjects.Contains(characterProfile))
        {
            UIManager.Instance.enemyProfilePanel.SetActive(false);
        }
    }
}
