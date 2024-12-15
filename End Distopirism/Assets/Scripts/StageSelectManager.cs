using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StageSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject stageInfoPanel;
    [SerializeField] private Transform titleParent;
    [SerializeField] private Transform enemyInfoParent;
    [SerializeField] private Transform mapInfoParent;
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup infoPanelCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float slideOffset = 1000f;
    [SerializeField] private Ease slideEase = Ease.OutQuint;

    private RectTransform infoPanelRect;
    private Vector2 originalPosition;
    private Sequence currentAnimation;

    private void Start()
    {
        // RectTransform 컴포넌트 가져오기
        infoPanelRect = stageInfoPanel.GetComponent<RectTransform>();
        if (infoPanelRect != null)
        {
            originalPosition = infoPanelRect.anchoredPosition;
        }

        // CanvasGroup 컴포넌트 확인
        if (infoPanelCanvasGroup == null)
        {
            infoPanelCanvasGroup = stageInfoPanel.GetComponent<CanvasGroup>();
            if (infoPanelCanvasGroup == null)
            {
                infoPanelCanvasGroup = stageInfoPanel.AddComponent<CanvasGroup>();
            }
        }

        // 시작 시 Info 패널 초기화
        if (stageInfoPanel != null)
        {
            stageInfoPanel.SetActive(false);
            if (infoPanelRect != null)
            {
                infoPanelRect.anchoredPosition = originalPosition + new Vector2(slideOffset, 0);
            }
            infoPanelCanvasGroup.alpha = 0f;
        }

        // Start 버튼 이벤트 연결
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClick);
    }

    private void Update()
    {
        // ESC 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (stageInfoPanel != null && stageInfoPanel.activeSelf)
                HideStageInfo();
        }
    }

    // 스테이지 버튼 클릭 시 호출되는 메서드
    public void OnStageButtonClick(int stageNumber)
    {
        // Button 컴포넌트의 onClick 이벤트에 연결
        DeckData.currentStage = stageNumber;
        UpdateStageInfo(stageNumber);
        ShowStageInfo();
    }

    private void ShowStageInfo()
    {
        // 이전 애니메이션이 실행 중이라면 중단
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }

        stageInfoPanel.SetActive(true);
        infoPanelCanvasGroup.alpha = 0f;
        infoPanelRect.anchoredPosition = originalPosition + new Vector2(slideOffset, 0);

        // 새로운 시퀀스 생성
        currentAnimation = DOTween.Sequence();

        // 슬라이드와 페이드인 애니메이션 동시 실행
        currentAnimation
            .Join(infoPanelRect.DOAnchorPos(originalPosition, slideDuration)
                .SetEase(slideEase))
            .Join(infoPanelCanvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(Ease.InOutSine))
            .OnComplete(() => {
                Debug.Log("Info 패널 애니메이션 완료");
            });
    }

    private void HideStageInfo()
    {
        // 이전 애니메이션이 실행 중이라면 중단
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }

        // 새로운 시퀀스 생성
        currentAnimation = DOTween.Sequence();

        // 페이드아웃과 슬라이드 애니메이션 동시 실행
        currentAnimation
            .Join(infoPanelRect.DOAnchorPos(originalPosition + new Vector2(slideOffset, 0), slideDuration)
                .SetEase(slideEase))
            .Join(infoPanelCanvasGroup.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InOutSine))
            .OnComplete(() => {
                stageInfoPanel.SetActive(false);
                Debug.Log("Info 패널 숨김 완료");
            });
    }

    private void UpdateStageInfo(int stageNumber)
    {
        // 타이틀 정보 업데이트
        UpdateTitleInfo(stageNumber);

        // 적 정보 업데이트
        UpdateEnemyInfo(stageNumber);

        // 맵 정보 업데이트
        UpdateMapInfo(stageNumber);
    }

    private void UpdateTitleInfo(int stageNumber)
    {
        // 모든 타이틀 정보 비활성화
        foreach (Transform child in titleParent)
        {
            child.gameObject.SetActive(false);
        }

        // 해당 스테이지의 타이틀 정보만 활성화
        Transform stageTitle = titleParent.Find($"Stage{stageNumber}Title");
        if (stageTitle != null)
            stageTitle.gameObject.SetActive(true);
    }

    private void UpdateEnemyInfo(int stageNumber)
    {
        // 모든 적 정보 비활성화
        foreach (Transform child in enemyInfoParent)
        {
            child.gameObject.SetActive(false);
        }

        // 해당 스테이지의 적 정보만 활성화
        Transform stageEnemy = enemyInfoParent.Find($"Stage{stageNumber}Enemy");
        if (stageEnemy != null)
            stageEnemy.gameObject.SetActive(true);
    }

    private void UpdateMapInfo(int stageNumber)
    {
        // 모든 맵 정보 비활성화
        foreach (Transform child in mapInfoParent)
        {
            child.gameObject.SetActive(false);
        }

        // 해당 스테이지의 맵 정보만 활성화
        Transform stageMap = mapInfoParent.Find($"Stage{stageNumber}Map");
        if (stageMap != null)
            stageMap.gameObject.SetActive(true);
    }

    private void OnStartButtonClick()
    {
        // SceneButtonManager를 통해 덱 빌딩 씬으로 이동
        SceneButtonManager sceneManager = FindObjectOfType<SceneButtonManager>();
        if (sceneManager != null)
        {
            sceneManager.OnStage1ButtonClick();
        }
    }

    private void OnDestroy()
    {
        // 모든 DOTween 애니메이션 정리
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
        }
        if (infoPanelRect != null)
        {
            infoPanelRect.DOKill();
        }
        if (infoPanelCanvasGroup != null)
        {
            infoPanelCanvasGroup.DOKill();
        }
    }
} 