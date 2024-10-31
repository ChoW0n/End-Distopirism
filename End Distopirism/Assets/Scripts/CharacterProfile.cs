using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CharacterProfile : MonoBehaviour
{
    [SerializeField]
    private Player player;
    public Player GetPlayer => player;

    [SerializeField]
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

        // 스킬 적용하여 캐릭터의 데미지 값을 설정합니다.
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

        // 체력바와 정신력바 생성
        CreateStatusBars();
        UpdateStatusBars();
    }

    private void CreateStatusBars()
    {
        // 체력바 생성
        healthBar = Instantiate(UIManager.Instance.healthBarPrefab, hpPosition.position, Quaternion.identity, UIManager.Instance.canvas.transform);
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
        mentalityBar = Instantiate(UIManager.Instance.mentalityBarPrefab, mtPosition.position, Quaternion.identity, UIManager.Instance.canvas.transform);
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
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealth, Time.deltaTime * 5f);
        }

        // 정신력바 업데이트
        if (mentalityFill != null)
        {
            float targetMentality = Mathf.Clamp01(player.menTality / 100f);
            mentalityFill.fillAmount = Mathf.Lerp(mentalityFill.fillAmount, targetMentality, Time.deltaTime * 5f);
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
        if (isSelected)
        {
            if (skillManager != null)
            {
                Debug.Log($"캐릭터 {player.charName}의 스킬 카드 활성화");
                skillManager.AssignRandomSkillSprites(player.skills);
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

    // 스킬을 캐릭터에 적용하는 메서드
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
        if (healthBar != null) Destroy(healthBar);
        if (mentalityBar != null) Destroy(mentalityBar);
        gameObject.SetActive(false);
        // todo: 이팩트 재생
    }

    void Update()
    {
        // 캐릭터가 카메라의 X 회전 값만 따라가도록 설정
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = mainCamera.transform.eulerAngles.x; // X축 회전만 카메라를 따라감
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
        player.menTality = Mathf.Clamp(newMentality, 0f, 100f);
        UpdateStatusBars();
    }
}
