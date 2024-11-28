using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

        // 스킬 적용하여 캐릭터의 데미지 값을 설정
        ApplySkill();

        initialYRotation = transform.rotation.eulerAngles.y;

        // HP와 MT Position 찾기
        hpPosition = transform.Find("HPPosition");
        mtPosition = transform.Find("MTPosition");
        
        if (hpPosition == null)
        {
            Debug.LogError($"캐릭터 {gameObject.name}에서 HPPosition을 찾을 수 없습니다.");
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
        UpdateStatusBars();
    }

    public void ShowCharacterInfo()
    {
        UIManager.Instance.ShowCharacterInfo(this);
        
        // 선택 상태에 따라 스킬 카드 처리
        if (isSelected && BattleManager.Instance.state != GameState.enemyTurn)
        {
            if (skillManager != null && player.skills != null && player.skills.Count > 0)
            {
                Debug.Log($"캐릭터 {player.charName}의 스킬 카드 활성화");
                skillManager.AssignRandomSkillSprites(player.skills);
                skillManager.OnSkillSelected = (skill) => SelectSkill(skill);
            }
            else
            {
                Debug.LogWarning($"캐릭터 {player.charName}의 SkillManager 또는 스킬 리스트가 null입니다.");
            }
        }
        else
        {
            HideSkillCards();
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

    // 스킬을 캐릭터에 적용하는 서드
    void ApplySkill()
    {
        if (player.skills != null && player.skills.Count > 0)
        {
            // 스킬을 적용하여 캐릭터의 데미지 값을 설정하는 예시
            Skill skill = player.skills[0]; // 예시로 첫 번째 스킬만 적용
            player.maxDmg = skill.maxDmg;
            player.minDmg = skill.minDmg; 
            player.dmgUp = skill.dmgUp; 
            // 필요한 경우 추가로 처리할 코드
            Debug.Log("스킬이 캐릭터에 적용되습니다: " + skill.skillName);
        }
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
            // 페이드 아웃 시퀀스 생성
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
            euler.x = mainCamera.transform.eulerAngles.x; // X 회전만 카메라를 따라감
            // Y축과 Z축은 현재 값을 유지
            transform.rotation = Quaternion.Euler(euler);
        }

        // 상태바 위치와 회전 업데이트
        if (healthBar != null && mentalityBar != null)
        {
            healthBar.transform.position = hpPosition.position;
            mentalityBar.transform.position = mtPosition.position;
            
            healthBar.transform.rotation = Camera.main.transform.rotation;
            mentalityBar.transform.rotation = Camera.main.transform.rotation;

            // 상태바 값 업데이트
            UpdateStatusBars();
        }

        // 스킬 이펙트 위치와 회전 업데이트
        if (currentSkillEffect != null && skillPosition != null)
        {
            currentSkillEffect.transform.position = skillPosition.position;
            currentSkillEffect.transform.rotation = Camera.main.transform.rotation;
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
        
        // 값이 실제로 변경되었을 때만 업데이트
        if (oldMentality != player.menTality)
        {
            UpdateStatusBars();
            Debug.Log($"정신력 변경: {oldMentality} -> {player.menTality}");
        }
    }

    // 스킬 이펙트 생성 및 표시
    public void ShowSkillEffect(int initialDamage)
    {
        if (skillPosition == null) return;

        if (currentSkillEffect != null)
        {
            Destroy(currentSkillEffect);
        }

        GameObject prefab = CompareTag("Player") ? 
            UIManager.Instance.playerSkillEffectPrefab : 
            UIManager.Instance.enemySkillEffectPrefab;

        currentSkillEffect = Instantiate(prefab, skillPosition.position, Quaternion.identity, UIManager.Instance.canvas.transform);
        
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
        Vector3 originalScale = dmgText.transform.localScale;
        Color originalColor = dmgText.color;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 현기 애니메이션
            float scaleFactor = 1f + Mathf.Sin(progress * Mathf.PI) * 0.4f;
            dmgText.transform.localScale = originalScale * scaleFactor;
            
            // 색상 변경
            dmgText.color = Color.Lerp(Color.red, originalColor, progress);
            
            // 숫자 업데이트
            int currentDamage = (int)Mathf.Lerp(startDamage, newDamage, progress);
            dmgText.text = currentDamage.ToString();
            
            yield return null;
        }
        
        // 최종 상태로 설정
        dmgText.transform.localScale = originalScale;
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

    public void SelectSkill(Skill skill)
    {
        selectedSkill = skill;
        // 선택된 스킬을 플레이어에 적용
        ApplySelectedSkill();
        Debug.Log($"스킬 선택됨: {skill.skillName}");
    }

    private void ApplySelectedSkill()
    {
        if (selectedSkill != null)
        {
            player.maxDmg = selectedSkill.maxDmg;
            player.minDmg = selectedSkill.minDmg;
            player.dmgUp = selectedSkill.dmgUp;
            Debug.Log($"스킬 적용됨: {selectedSkill.skillName}, 데미지: {selectedSkill.minDmg}-{selectedSkill.maxDmg}");
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
}
