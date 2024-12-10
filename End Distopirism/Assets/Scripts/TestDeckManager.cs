using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestDeckManager : MonoBehaviour
{
    [Header("캐릭터 선택 UI")]
    [SerializeField, Tooltip("캐릭터 선택 버튼 배열")]
    private Button[] characterButtons;
    
    [Header("선택된 캐릭터 UI")]
    [SerializeField, Tooltip("선택된 캐릭터들이 표시될 패널")]
    private Transform selectedCharacterPanel;
    [SerializeField, Tooltip("선택된 캐릭터 아이콘 프리팹")]
    private GameObject characterPrefab;
    
    [Header("캐릭터 패널 설정")]
    [SerializeField] private float characterSpacing = 10f;
    [SerializeField] private float characterSize = 100f;
    [SerializeField] private int maxCharactersPerRow = 5;
    
    [Header("카드 선택 UI")]
    [SerializeField, Tooltip("카드 선택 패널")]
    private GameObject cardSelectPanel;
    [SerializeField, Tooltip("카드 스크롤뷰의 Content Transform")]
    private Transform cardScrollContent;
    [SerializeField, Tooltip("카드 버튼 프리팹 (Resources/Prefabs/CardButton)")]
    private GameObject cardButtonPrefab;
    
    [Header("선택된 카드 UI")]
    [SerializeField, Tooltip("선택된 카드들이 표시될 패널")]
    private Transform selectedCardPanel;
    [SerializeField, Tooltip("선택된 카드 프리팹")]
    private GameObject cardPrefab;
    
    [Header("카드 패널 설정")]
    [SerializeField] private float cardSpacing = 10f;
    [SerializeField] private Vector2 cardSize = new Vector2(200f, 300f);
    [SerializeField] private int maxCardsPerRow = 5;
    [SerializeField] private float cardPanelHeight = 120f;
    [SerializeField] private Vector2 cardImageSize = new Vector2(80f, 80f);

    // 프라이빗 필드들
    private List<Button> cardButtons = new List<Button>();
    private List<int> selectedCharacterIndices = new List<int>();
    private List<GameObject> selectedCharacterObjects = new List<GameObject>();
    private List<GameObject> selectedCards = new List<GameObject>();
    private int currentCharacterIndex = -1;

    // 카드 카운트 관리를 위한 딕셔너리 추가
    private Dictionary<Skill, GameObject> selectedCardDict = new Dictionary<Skill, GameObject>();
    private Dictionary<Skill, int> cardCountDict = new Dictionary<Skill, int>();
    private const int MAX_CARD_COUNT = 3; // 카드당 최대 중복 가능 횟수

    [Header("캐릭터 스크롤뷰")]
    [SerializeField] private Transform characterScrollContent;
    [SerializeField] private GameObject characterButtonPrefab;

    // 캐릭터 프리팹 참조 저장
    private Dictionary<Button, GameObject> characterPrefabDict = new Dictionary<Button, GameObject>();

    // 더블 클릭 감지를 위한 변수들
    private float doubleClickTime = 0.3f;  // 더블 클릭 인식 시간
    private float lastClickTime = -1f;
    private int lastClickedIndex = -1;

    [Header("스테이지 설정")]
    [SerializeField] private StageData currentStageData;
    private int maxSelectableCharacters;

    [Header("UI 버튼")]
    [SerializeField] private Button startButton; // 시작 버튼

    [Header("카드 확대 설정")]
    [SerializeField] private float cardHoverScale = 1.5f;  // 카드 확대 배율
    [SerializeField] private float cardHoverDuration = 0.2f;  // 확대/축소 애니메이션 시간

    // 클래스 상단에 변수 추가
    private GameObject currentHoveredCard = null;
    private Canvas cardScrollCanvas;
    private Canvas selectedCardCanvas;

    [Header("선택된 카드 패널 설정")]
    [SerializeField] private int selectedCardsPerRow = 5;  // 한 줄당 최대 카드 수
    [SerializeField] private float selectedCardSpacing = 10f;  // 카드 간격
    [SerializeField] private float selectedPanelPadding = 10f;  // 패널 여백

    [SerializeField] private GameObject characterInfoPanelPrefab; // 캐릭터 정보 패널 프리팹
    [SerializeField] private Transform characterInfoParent; // 캐릭터 정보 패널이 생성될 부모 Transform
    private GameObject currentCharacterInfoPanel; // 현재 표시 중인 캐릭터 정보 패널

    private void Awake()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (cardButtonPrefab == null)
        {
            Debug.LogError("CardButton 프리팹이 할당되지 않습니다. Resources/Prefabs 폴더에서 CardButton 프리팹을 할당해주세요.");
        }
        if (cardScrollContent == null)
        {
            Debug.LogError("Card Scroll Content가 할당되지 않았습니다. Card Select Panel의 Scroll View > Viewport > Content를 할당해주세요.");
        }
        if (selectedCardPanel == null)
        {
            Debug.LogError("Selected Card Panel이 할당되지 않았습니다.");
        }
        if (cardPrefab == null)
        {
            Debug.LogError("Card 프리팹이 할당되지 않았습니다.");
        }
    }

    private void Start()
    {
        // StageData가 할당되지 않았을 경우 Resources 폴더에서 로드
        if (currentStageData == null)
        {
            currentStageData = Resources.Load<StageData>($"StageData/Stage{DeckData.currentStage}");
            if (currentStageData == null)
            {
                Debug.LogError($"Stage{DeckData.currentStage}의 StageData를 찾을 수 없습니다. Resources/StageData 폴더에 StageData 에셋이 있는지 확인해주세요.");
                maxSelectableCharacters = 3; // 기본값 설정
            }
            else
            {
                maxSelectableCharacters = currentStageData.requiredCharacterCount;
            }
        }
        else
        {
            maxSelectableCharacters = currentStageData.requiredCharacterCount;
        }

        LoadCharacterPrefabs();
        cardSelectPanel.SetActive(false);
        SetupSelectedCharacterPanel();
        SetupSelectedCardPanel();
        LoadSkillCards();
        InitializeButtons();

        // 시작 버튼 이벤트 연결
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            startButton.onClick.AddListener(OnStartButtonClick);
        }
        else
        {
            Debug.LogError("시작 버튼이 할당되지 않았습니다.");
        }

        // 시작 버튼 초기 상태 설정
        UpdateStartButtonState();
    }

    private void LoadCharacterPrefabs()
    {
        // 캐릭터 스크롤 컨텐츠의 GridLayoutGroup 설정
        GridLayoutGroup characterGridLayout = characterScrollContent.GetComponent<GridLayoutGroup>();
        if (characterGridLayout == null)
        {
            characterGridLayout = characterScrollContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 수평 방향으로 캐릭터 버튼 배치
        characterGridLayout.cellSize = new Vector2(characterSize, characterSize);
        characterGridLayout.spacing = new Vector2(characterSpacing, characterSpacing);
        characterGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        characterGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        characterGridLayout.childAlignment = TextAnchor.UpperLeft;
        characterGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        characterGridLayout.constraintCount = maxCharactersPerRow;
        characterGridLayout.padding = new RectOffset(10, 10, 10, 10);

        // ContentSizeFitter 설정
        ContentSizeFitter characterSizeFitter = characterScrollContent.GetComponent<ContentSizeFitter>();
        if (characterSizeFitter == null)
        {
            characterSizeFitter = characterScrollContent.gameObject.AddComponent<ContentSizeFitter>();
        }
        characterSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        characterSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Resources/Characters 폴더에서 모든 캐릭터 프리팹 로드
        GameObject[] characterPrefabs = Resources.LoadAll<GameObject>("Characters");
        
        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Resources/Characters 폴더에서 캐릭 수 없습니다.");
            return;
        }
        
        // 효한 캐릭터 프리팹만 필터링
        List<GameObject> validPrefabs = new List<GameObject>();
        foreach (GameObject prefab in characterPrefabs)
        {
            CharacterProfile profile = prefab.GetComponent<CharacterProfile>();
            if (profile != null && profile.GetPlayer != null)
            {
                validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogError("유효한 캐릭터 프리팹이 없습니다. CharacterProfile 컴포넌트를 확인해주세요.");
            return;
        }

        // 유효한 프리팹 수만큼 버튼 배열 초기화
        characterButtons = new Button[validPrefabs.Count];

        for (int i = 0; i < validPrefabs.Count; i++)
        {
            GameObject prefab = validPrefabs[i];
            CharacterProfile characterProfile = prefab.GetComponent<CharacterProfile>();
            
            // 캐릭터 버튼 생성
            GameObject buttonObj = Instantiate(characterButtonPrefab, characterScrollContent);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button != null)
            {
                // 버튼 이미지에 캐릭터 스프라이트 적용
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = characterProfile.GetPlayer.charSprite;
                    buttonImage.preserveAspect = true;
                }

                // TextMeshPro 텍스트 컴포넌트 제거
                TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    Destroy(tmpText.gameObject);
                }

                // Legacy Text 컴포넌트 제거
                Text legacyText = buttonObj.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    Destroy(legacyText.gameObject);
                }

                // 딕셔너리에 프리팹 참조 저장
                characterPrefabDict.Add(button, prefab);
                characterButtons[i] = button;
            }
        }
    }

    private void SetupCardCanvases()
    {
        // CardScroll Canvas 정
        cardScrollCanvas = cardScrollContent.gameObject.AddComponent<Canvas>();
        cardScrollCanvas.overrideSorting = true;
        cardScrollCanvas.sortingOrder = 1;

        // SelectedCard Canvas 설정
        selectedCardCanvas = selectedCardPanel.gameObject.AddComponent<Canvas>();
        selectedCardCanvas.overrideSorting = true;
        selectedCardCanvas.sortingOrder = 2;

        // GraphicRaycaster 추가
        if (cardScrollContent.GetComponent<GraphicRaycaster>() == null)
            cardScrollContent.gameObject.AddComponent<GraphicRaycaster>();
        if (selectedCardPanel.GetComponent<GraphicRaycaster>() == null)
            selectedCardPanel.gameObject.AddComponent<GraphicRaycaster>();
    }

    private void LoadSkillCards()
    {
        SetupCardCanvases();

        // GridLayoutGroup 설정
        GridLayoutGroup gridLayout = cardScrollContent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = cardScrollContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 프리팹의 RectTransform에서 크기 가져오기
        RectTransform cardButtonRect = cardButtonPrefab.GetComponent<RectTransform>();
        Vector2 originalCardSize = new Vector2(185.6f, 246.4f);  // 프리팹의 정확한 크기로 설정

        // GridLayout 설정
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.cellSize = originalCardSize;
        gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = maxCardsPerRow;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

        // Content에 Canvas 컴포넌트 추가
        Canvas contentCanvas = cardScrollContent.GetComponent<Canvas>();
        if (contentCanvas == null)
        {
            contentCanvas = cardScrollContent.gameObject.AddComponent<Canvas>();
            contentCanvas.overrideSorting = true;
            contentCanvas.sortingOrder = 1;
        }

        // 상단에서 아래로 카드가 생성되도록 설정
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.cellSize = new Vector2(cardSize.x, cardSize.y);
        gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = maxCardsPerRow;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

        // Content의 ContentSizeFitter 설정
        ContentSizeFitter contentSizeFitter = cardScrollContent.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = cardScrollContent.gameObject.AddComponent<ContentSizeFitter>();
        }
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 스킬 카드 생성
        Skill[] skills = Resources.LoadAll<Skill>("Skills");
        
        for (int i = 0; i < skills.Length; i++)
        {
            Skill skill = skills[i];
            GameObject cardButtonObj = Instantiate(cardButtonPrefab, cardScrollContent);
            
            // 카기 크 설정
            RectTransform cardRect = cardButtonObj.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.sizeDelta = originalCardSize;
                cardRect.localScale = Vector3.one;  // 스케일을 1로 설정
            }

            // 카드 버튼 이름 지정
            cardButtonObj.name = $"CardButton{i + 1}_{skill.name}";
            
            cardButtons.Add(cardButtonObj.GetComponent<Button>());
            
            Image cardImage = cardButtonObj.GetComponent<Image>();
            if (cardImage != null && skill.nomalSprite != null)
            {
                cardImage.sprite = skill.nomalSprite;
                cardImage.preserveAspect = true;
            }

            // SkillName 텍스트 설정
            Transform skillNameTransform = cardButtonObj.transform.Find("SkillName");
            if (skillNameTransform != null)
            {
                RectTransform skillNameRect = skillNameTransform.GetComponent<RectTransform>();
                if (skillNameRect != null)
                {
                    // 기존 anchoredPosition에서 Y값만 6.1 증가
                    Vector2 currentPos = skillNameRect.anchoredPosition;
                    skillNameRect.anchoredPosition = new Vector2(currentPos.x, currentPos.y + 6.1f);
                }

                TextMeshProUGUI skillNameText = skillNameTransform.GetComponent<TextMeshProUGUI>();
                if (skillNameText != null)
                {
                    skillNameText.text = skill.skillName;
                }
                else
                {
                    Debug.LogError($"SkillName에 TextMeshProUGUI 컴포넌트가 없습니다: {skill.skillName}");
                }
            }
            else
            {
                Debug.LogError($"SkillName 오브젝트를 찾을 수 없습니다: {skill.skillName}");
            }

            // SkillInfo 텍스트 설정
            Transform skillInfoTransform = cardButtonObj.transform.Find("SkillInfo");
            if (skillInfoTransform != null)
            {
                TextMeshProUGUI skillInfoText = skillInfoTransform.GetComponent<TextMeshProUGUI>();
                if (skillInfoText != null)
                {
                    skillInfoText.text = skill.skillEffect;
                }
                else
                {
                    Debug.LogError($"SkillInfo에 TextMeshProUGUI 포넌트가 없습니다: {skill.skillName}");
                }
            }
            else
            {
                Debug.LogError($"SkillInfo 오브젝트를 찾을 수 없습니다: {skill.skillName}");
            }
            
            // Count 텍스트 초기화
            Transform countTransform = cardButtonObj.transform.Find("Count");
            if (countTransform != null)
            {
                TextMeshProUGUI countText = countTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (countText != null)
                {
                    int currentCount = GetCurrentCardCount(skill);
                    countText.text = $"{currentCount}/{MAX_CARD_COUNT}";
                }
            }
            
            cardButtonObj.AddComponent<SkillReference>().skill = skill;

            // 이벤트 트리거 설정
            EventTrigger eventTrigger = cardButtonObj.GetComponent<EventTrigger>();
            if (eventTrigger != null)
                Destroy(eventTrigger);
            
            eventTrigger = cardButtonObj.AddComponent<EventTrigger>();

            int index = i; // 클로저를 위한 인덱스 복사

            // 마우스 진입 이벤트
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { OnCardHoverEnter(cardButtonObj); });
            eventTrigger.triggers.Add(enterEntry);

            // 마우스 이탈 이벤트
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { OnCardHoverExit(cardButtonObj); });
            eventTrigger.triggers.Add(exitEntry);

            // 클릭 이벤트
            var button = cardButtonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnCardSelected(index));
            }
        }
    }

    // 현재 선택된 카드의 수를 반환하는 메서드
    private int GetCurrentCardCount(Skill skill)
    {
        if (cardCountDict.TryGetValue(skill, out int count))
        {
            return count;
        }
        return 0;
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => OnCharacterButtonClick(index));
        }
        
        for (int i = 0; i < cardButtons.Count; i++)
        {
            int index = i;
            cardButtons[i].onClick.AddListener(() => OnCardSelected(index));
        }
    }

    private void OnCardSelected(int cardIndex)
    {
        if (currentCharacterIndex == -1) return;
        
        SkillReference skillRef = cardButtons[cardIndex].GetComponent<SkillReference>();
        if (skillRef == null || skillRef.skill == null) return;
        
        Skill selectedSkill = skillRef.skill;

        // 이미 선택된 카드인 경우 카운트만 증가
        if (selectedCardDict.ContainsKey(selectedSkill))
        {
            int currentCount = cardCountDict[selectedSkill];
            int maxCount = selectedSkill.maxCardCount > 0 ? selectedSkill.maxCardCount : MAX_CARD_COUNT;
            
            if (currentCount >= maxCount)
            {
                Debug.Log($"{selectedSkill.skillName} 카드는 최대 {maxCount}장까지만 추가할 수 있습니다.");
                return;
            }

            cardCountDict[selectedSkill]++;
            UpdateCardCount(selectedCardDict[selectedSkill], cardCountDict[selectedSkill]);
        }
        else // 새로운 카드 추가
        {
            GameObject newCard = Instantiate(cardPrefab, selectedCardPanel);
            
            // Canvas 설정을 먼저 수행
            Canvas cardCanvas = newCard.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
            cardCanvas.sortingOrder = 10 + selectedCards.Count;  // 기본 정렬 순서를 10부터 시작

            // GraphicRaycaster 추가
            GraphicRaycaster raycaster = newCard.AddComponent<GraphicRaycaster>();

            // 카드 크기 설정
            RectTransform cardRect = newCard.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.sizeDelta = new Vector2(185.6f, 246.4f);
                cardRect.localScale = Vector3.one;

                // SkillName 설정
                Transform skillNameTr = newCard.transform.Find("SkillName");
                if (skillNameTr != null)
                {
                    TextMeshProUGUI skillNameText = skillNameTr.GetComponent<TextMeshProUGUI>();
                    if (skillNameText != null)
                    {
                        skillNameText.text = selectedSkill.skillName;
                    }
                }

                // SkillInfo 설정
                Transform skillInfoTr = newCard.transform.Find("SkillInfo");
                if (skillInfoTr != null)
                {
                    RectTransform skillInfoRect = skillInfoTr.GetComponent<RectTransform>();
                    if (skillInfoRect != null)
                    {
                        skillInfoRect.anchorMin = new Vector2(0.5f, 0.5f);
                        skillInfoRect.anchorMax = new Vector2(0.5f, 0.5f);
                        skillInfoRect.sizeDelta = new Vector2(100f, 58.4557f);
                        skillInfoRect.anchoredPosition = new Vector2(0f, -38.4f);
                        skillInfoRect.localScale = new Vector3(1.209023f, 1.209023f, 1.209023f);
                    }

                    // SkillInfo 텍스트 - 텍스트 내용만 변경
                    TextMeshProUGUI skillInfoText = skillInfoTr.GetComponent<TextMeshProUGUI>();
                    if (skillInfoText != null)
                    {
                        skillInfoText.text = selectedSkill.skillEffect;
                    }
                }

                // Count 설정
                Transform countTr = newCard.transform.Find("Count");
                if (countTr != null)
                {
                    RectTransform countRect = countTr.GetComponent<RectTransform>();
                    if (countRect != null)
                    {
                        countRect.anchorMin = new Vector2(0.5f, 0.5f);
                        countRect.anchorMax = new Vector2(0.5f, 0.5f);
                        countRect.sizeDelta = new Vector2(60f, 60f);
                        countRect.anchoredPosition = new Vector2(0f, -97.628f);
                        countRect.localScale = new Vector3(0.6108197f, 0.6108197f, 0.6108197f);
                    }

                    TextMeshProUGUI countText = countTr.GetComponentInChildren<TextMeshProUGUI>();
                    if (countText != null)
                    {
                        countText.text = "1";
                    }
                }
            }

            // 이미지 설정
            Image cardImage = newCard.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.sprite = selectedSkill.nomalSprite;
                cardImage.preserveAspect = true;
                cardImage.raycastTarget = true;
            }

            selectedCards.Add(newCard);
            selectedCardDict.Add(selectedSkill, newCard);
            cardCountDict.Add(selectedSkill, 1);
            
            newCard.AddComponent<SkillReference>().skill = selectedSkill;

            // 이벤트 트리거 설정
            EventTrigger eventTrigger = newCard.AddComponent<EventTrigger>();
            eventTrigger.triggers.Clear();  // 기존 트리거 제거

            // 마우스 진입 이벤트
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { OnCardHoverEnter(newCard); });
            eventTrigger.triggers.Add(enterEntry);

            // 마우스 이탈 이벤트
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { OnCardHoverExit(newCard); });
            eventTrigger.triggers.Add(exitEntry);

            // 클릭 이벤트
            Button button = newCard.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => UnselectCard(newCard));
            }
        }

        // 카드 선택 후 모든 카드의 카운트 텍스트 업데이트
        UpdateAllCardCounts();
    }

    private void UnselectCard(GameObject card)
    {
        SkillReference skillRef = card.GetComponent<SkillReference>();
        if (skillRef != null && skillRef.skill != null)
        {
            Skill skill = skillRef.skill;
            
            // 카운트가 1보다 크면 카운트만 감소
            if (cardCountDict[skill] > 1)
            {
                cardCountDict[skill]--;
                UpdateCardCount(card, cardCountDict[skill]);
            }
            else // 카운트가 1이면 카드 제거
            {
                selectedCardDict.Remove(skill);
                cardCountDict.Remove(skill);
                selectedCards.Remove(card);
                Destroy(card);
            }
        }

        // 카드 제거 후 모든 카드의 카운트 텍스트 업데이트
        UpdateAllCardCounts();
    }
    
    private void SetupSelectedCharacterPanel()
    {
        // GridLayoutGroup 제거 (단일 캐릭터가 패널을 꽉 채우도록)
        GridLayoutGroup gridLayout = selectedCharacterPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            Destroy(gridLayout);
        }

        // ContentSizeFitter 제거 (패널 크기 고정)
        ContentSizeFitter sizeFitter = selectedCharacterPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            Destroy(sizeFitter);
        }
    }

    private void SetupSelectedCardPanel()
    {
        RectTransform panelRect = selectedCardPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // 프리팹의 원본 크기를 기준으로 패널 높이 설정
            RectTransform cardPrefabRect = cardPrefab.GetComponent<RectTransform>();
            float cardHeight = cardPrefabRect != null ? cardPrefabRect.sizeDelta.y : cardSize.y;
            // 여러 줄을 수용할 수 있도록 높이 설정
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, (cardHeight + selectedPanelPadding) * 3);  // 최대 3줄까지 표시
        }

        // GridLayoutGroup 설정
        GridLayoutGroup gridLayout = selectedCardPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = selectedCardPanel.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 프리팹의 원본 크기 사용
        RectTransform prefabRect = cardPrefab.GetComponent<RectTransform>();
        Vector2 originalCardSize = prefabRect != null ? prefabRect.sizeDelta : cardSize;

        // GridLayout 설정 수정
        gridLayout.cellSize = originalCardSize;
        gridLayout.spacing = new Vector2(selectedCardSpacing, selectedCardSpacing);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;  // 열 수 제한으로 변경
        gridLayout.constraintCount = selectedCardsPerRow;  // 한 줄당 최대 카드 수
        gridLayout.padding = new RectOffset(
            (int)selectedPanelPadding, 
            (int)selectedPanelPadding, 
            (int)selectedPanelPadding, 
            (int)selectedPanelPadding
        );

        // ContentSizeFitter 추가
        ContentSizeFitter sizeFitter = selectedCardPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = selectedCardPanel.gameObject.AddComponent<ContentSizeFitter>();
        }
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;  // 세로 크기를 내용에 맞게 조절
    }
    
    private void OnCharacterButtonClick(int characterIndex)
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // 같은 버튼을 더블 클릭한 경우
        if (characterIndex == lastClickedIndex && timeSinceLastClick <= doubleClickTime)
        {
            OnCharacterDoubleClick(characterIndex);
        }
        // 단일 클릭
        else
        {
            OnCharacterSingleClick(characterIndex);
        }

        lastClickTime = Time.time;
        lastClickedIndex = characterIndex;
    }

    // 단일 클릭: 덱 상태 확인
    private void OnCharacterSingleClick(int characterIndex)
    {
        // 현재 선택된 캐릭터 변경
        currentCharacterIndex = characterIndex;

        // 캐릭터 정보 패널 활성화
        ShowCharacterInfo(characterIndex);
        
        // 카드 선택 패널 활성화
        cardSelectPanel.SetActive(true);
        
        // 기존 선택된 카드들 초기화
        ClearSelectedCards();
        
        // 해당 캐릭터 덱 정보 로드
        LoadExistingDeck(characterIndex);

        // 버튼 하이라이트 효과
        UpdateCharacterButtonsHighlight();

        // SelectedCharacterPanel 업데이트
        UpdateSelectedCharacterPanel();
    }

    private void UpdateSelectedCharacterPanel()
    {
        // SelectedCharacterPanel의 모든 자식 오브젝트 제거
        foreach (Transform child in selectedCharacterPanel)
        {
            Destroy(child.gameObject);
        }

        // 현재 선택된 캐릭터만 SelectedCharacterPanel에 추가
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characterButtons.Length)
        {
            CreateSelectedCharacterIcon(currentCharacterIndex);
        }
    }

    // 더블 클릭: 전투 대상으로 추가
    private void OnCharacterDoubleClick(int characterIndex)
    {
        // 이미 선택된 캐릭터인 경우 선택 해제
        if (selectedCharacterIndices.Contains(characterIndex))
        {
            UnselectCharacter(characterIndex);
        }
        else 
        {
            // 캐릭터 선택 제한 확인
            if (selectedCharacterIndices.Count >= maxSelectableCharacters)
            {
                Debug.LogWarning($"스테이지 {DeckData.currentStage}에서는 최대 {maxSelectableCharacters}명의 캐릭터만 선택할 수 있습니다.");
                return;
            }

            // 전투 대상으로 추가 (SelectedCharacterPanel에는 추가하지 않음)
            selectedCharacterIndices.Add(characterIndex);
            
            // 선택된 프리팹 저장
            if (characterPrefabDict.TryGetValue(characterButtons[characterIndex], out GameObject prefab))
            {
                DeckData.selectedCharacterPrefabs.Add(prefab);
            }
        }

        // 시작 버튼 상태 업데이트
        UpdateStartButtonState();
        
        // 버튼 하이라이트만 업데이트
        UpdateCharacterButtonsHighlight();
    }

    // 버튼 하이라이트 업데이트
    private void UpdateCharacterButtonsHighlight()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] != null)
            {
                // 현재 선택된 캐릭터는 회색으로 표시
                characterButtons[i].image.color = (i == currentCharacterIndex) ? Color.gray : Color.white;
                
                // 전투 대상으로 선택된 캐릭터들은 다른 색으로 표시
                if (selectedCharacterIndices.Contains(i))
                {
                    characterButtons[i].image.color = new Color(0.7f, 1f, 0.7f); // 연한 녹색
                }
            }
        }
    }

    // 하단 패널의 캐릭터 아이콘 클릭 처리
    private void OnSelectedCharacterClick(int characterIndex)
    {
        UnselectCharacter(characterIndex);
        UpdateCharacterButtonsHighlight();
    }

    private void CreateSelectedCharacterIcon(int characterIndex)
    {
        GameObject selectedChar = Instantiate(characterPrefab, selectedCharacterPanel);
        
        // RectTransform 설정으로 패널에 꽉 차게 표시
        RectTransform rectTransform = selectedChar.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
        
        Image charImage = selectedChar.GetComponent<Image>();
        if (charImage != null)
        {
            charImage.sprite = characterButtons[characterIndex].image.sprite;
            charImage.preserveAspect = true;
        }
        
        // 버튼 컴포넌트 제거
        Button charButton = selectedChar.GetComponent<Button>();
        if (charButton != null)
        {
            Destroy(charButton);
        }

        // 텍스트 컴포넌트가 있다면 제거
        TextMeshProUGUI[] texts = selectedChar.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            Destroy(text.gameObject);
        }
    }

    private void UnselectCharacter(int characterIndex)
    {
        // 선택된 캐릭터 목록에서 제거
        if (selectedCharacterIndices.Contains(characterIndex))
        {
            int index = selectedCharacterIndices.IndexOf(characterIndex);
            selectedCharacterIndices.Remove(characterIndex);

            // DeckData에서도 제거
            if (index < DeckData.selectedCharacterPrefabs.Count)
            {
                DeckData.selectedCharacterPrefabs.RemoveAt(index);
            }

            // 버튼 색상 초기화
            if (characterIndex < characterButtons.Length && characterButtons[characterIndex] != null)
            {
                characterButtons[characterIndex].image.color = Color.white;
            }
            
            // 현재 선택된 캐릭터와 동일한 경우 초기화
            if (currentCharacterIndex == characterIndex)
            {
                currentCharacterIndex = -1;
                cardSelectPanel.SetActive(false);
                ClearSelectedCards();
            }

            // 시작 버튼 상태 업데이트
            UpdateStartButtonState();
        }
        else
        {
            Debug.LogWarning($"선택된 캐릭터 목록에서 캐릭터를 찾을 수 없습니다. CharacterIndex: {characterIndex}");
        }
    }
    
    private void ClearSelectedCards()
    {
        foreach (GameObject card in selectedCards)
        {
            Destroy(card);
        }
        selectedCards.Clear();
        selectedCardDict.Clear();
        cardCountDict.Clear();
        
        // 모든 카드의 카운트 초기화
        UpdateAllCardCounts();
    }

    // 모든 카드의 카운트 텍스트를 업데이트하는 메서드
    private void UpdateAllCardCounts()
    {
        foreach (Button cardButton in cardButtons)
        {
            SkillReference skillRef = cardButton.GetComponent<SkillReference>();
            if (skillRef != null && skillRef.skill != null)
            {
                Transform countTransform = cardButton.transform.Find("Count");
                if (countTransform != null)
                {
                    TextMeshProUGUI countText = countTransform.GetComponentInChildren<TextMeshProUGUI>();
                    if (countText != null)
                    {
                        int currentCount = GetCurrentCardCount(skillRef.skill);
                        int maxCount = skillRef.skill.maxCardCount > 0 ? skillRef.skill.maxCardCount : MAX_CARD_COUNT;
                        countText.text = $"{currentCount}/{maxCount}";
                    }
                }
            }
        }
    }

    // 덱 저장 메서드 수정
    public void SaveDeck()
    {
        if (currentCharacterIndex == -1 || !characterPrefabDict.ContainsKey(characterButtons[currentCharacterIndex]))
        {
            Debug.LogWarning("저장할 캐릭터가 선택되지 않았습니다.");
            return;
        }

        GameObject characterPrefab = characterPrefabDict[characterButtons[currentCharacterIndex]];
        CharacterProfile characterProfile = characterPrefab.GetComponent<CharacterProfile>();

        if (characterProfile != null && characterProfile.GetPlayer != null)
        {
            // 기존 스킬 리스트 초기화
            characterProfile.GetPlayer.skills.Clear();

            // 선택된 카드들을 스킬 리스트에 추가
            foreach (var cardPair in selectedCardDict)
            {
                Skill skill = cardPair.Key;
                int count = cardCountDict[skill];

                for (int i = 0; i < count; i++)
                {
                    characterProfile.GetPlayer.skills.Add(skill);
                }
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(characterPrefab);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"캐릭터 {characterProfile.GetPlayer.charName}의 덱이 저장되었습니.");
            #endif
        }
    }

    // 기존 덱 로드 메서드 수정
    private void LoadExistingDeck(int characterIndex)
    {
        if (characterPrefabDict.TryGetValue(characterButtons[characterIndex], out GameObject prefab))
        {
            CharacterProfile characterProfile = prefab.GetComponent<CharacterProfile>();
            if (characterProfile != null && characterProfile.GetPlayer != null && characterProfile.GetPlayer.skills != null)
            {
                Dictionary<Skill, int> existingSkills = new Dictionary<Skill, int>();
                foreach (Skill skill in characterProfile.GetPlayer.skills)
                {
                    if (existingSkills.ContainsKey(skill))
                        existingSkills[skill]++;
                    else
                        existingSkills[skill] = 1;
                }

                foreach (var skillPair in existingSkills)
                {
                    for (int i = 0; i < skillPair.Value; i++)
                    {
                        int cardIndex = FindCardButtonIndex(skillPair.Key);
                        if (cardIndex != -1)
                        {
                            OnCardSelected(cardIndex);
                        }
                    }
                }
            }
        }
    }

    private int FindCardButtonIndex(Skill skill)
    {
        for (int i = 0; i < cardButtons.Count; i++)
        {
            SkillReference skillRef = cardButtons[i].GetComponent<SkillReference>();
            if (skillRef != null && skillRef.skill == skill)
            {
                return i;
            }
        }
        return -1;
    }

    private void UpdateStartButtonState()
    {
        if (startButton != null)
        {
            // 필요한 수만큼 캐릭터가 선택되었는지 확인
            bool canStart = selectedCharacterIndices.Count == maxSelectableCharacters;
            startButton.interactable = canStart;
        }
    }

    private void OnStartButtonClick()
    {
        // SceneButtonManager를 찾아서 스테이지 시작
        SceneButtonManager sceneManager = FindObjectOfType<SceneButtonManager>();
        if (sceneManager != null)
        {
            sceneManager.OnDeckBuildingComplete();
        }
        else
        {
            Debug.LogError("SceneButtonManager를 찾을 수 없습니다.");
        }
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    // OnCardHoverEnter 메서드 수정
    private void OnCardHoverEnter(GameObject card)
    {
        if (currentHoveredCard != null && currentHoveredCard != card)
        {
            OnCardHoverExit(currentHoveredCard);
        }

        currentHoveredCard = card;
        
        // 현재 진행 중인 트윈 중지
        DOTween.Kill(card.transform, true);
        
        // Canvas 정렬 순서를 최상위로 설정
        Canvas cardCanvas = card.GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = 1000;  // 호버 시 매우 높은 값으로 설정
        }
        
        // 카드 확대
        card.transform.DOScale(Vector3.one * cardHoverScale, cardHoverDuration)
            .SetEase(Ease.OutQuad);
    }

    // OnCardHoverExit 메서드 수정
    private void OnCardHoverExit(GameObject card)
    {
        if (currentHoveredCard != card)
            return;

        currentHoveredCard = null;
        
        // 현재 진행 중인 트윈 중지
        DOTween.Kill(card.transform, true);
        
        // Canvas 정렬 순서를 원래대로 복원
        Canvas cardCanvas = card.GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            int index = selectedCards.IndexOf(card);
            cardCanvas.sortingOrder = 10 + (index != -1 ? index : 0);  // 기본값 10에 인덱스 추가
        }
        
        // 카드 크기 복원
        card.transform.DOScale(Vector3.one, cardHoverDuration)
            .SetEase(Ease.OutQuad);
    }

    // 카드 자식 요소들의 레이아웃을 설정하는 새로운 메서드
    private void SetupCardChildElements(GameObject card)
    {
        // SkillInfo 설정
        Transform skillInfoTr = card.transform.Find("SkillInfo");
        if (skillInfoTr != null)
        {
            RectTransform skillInfoRect = skillInfoTr.GetComponent<RectTransform>();
            if (skillInfoRect != null)
            {
                // 프리팹의 정확한 설정값 적용
                skillInfoRect.anchorMin = new Vector2(0.5f, 0.5f);  // center
                skillInfoRect.anchorMax = new Vector2(0.5f, 0.5f);  // center
                skillInfoRect.sizeDelta = new Vector2(100f, 58.4557f);  // Width, Height
                skillInfoRect.anchoredPosition = new Vector2(0f, -38.4f);  // Pos X, Pos Y
                skillInfoRect.localScale = new Vector3(1.209023f, 1.209023f, 1.209023f);  // Scale
            }
        }

        // Count 설정
        Transform countTr = card.transform.Find("Count");
        if (countTr != null)
        {
            RectTransform countRect = countTr.GetComponent<RectTransform>();
            if (countRect != null)
            {
                // 프리팹의 정확한 설정값 적용
                countRect.anchorMin = new Vector2(0.5f, 0.5f);  // center
                countRect.anchorMax = new Vector2(0.5f, 0.5f);  // center
                countRect.sizeDelta = new Vector2(60f, 60f);  // Width, Height
                countRect.anchoredPosition = new Vector2(0f, -97.628f);  // Pos X, Pos Y
                countRect.localScale = new Vector3(0.6108197f, 0.6108197f, 0.6108197f);  // Scale
            }
        }
    }

    // UpdateCardCount 메서드 추가
    private void UpdateCardCount(GameObject card, int count)
    {
        Transform countTransform = card.transform.Find("Count");
        if (countTransform != null)
        {
            TextMeshProUGUI countText = countTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }

    // InitializeCardCount 메서드도 추가 (이미 있다면 무시)
    private void InitializeCardCount(GameObject card)
    {
        Transform countTransform = card.transform.Find("Count");
        if (countTransform != null)
        {
            TextMeshProUGUI countText = countTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = "1";
            }
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 캐릭터 스크롤 그리드 시각화
        DrawCharacterGridGizmos();
        
        // 선택된 카드 패널 그리드 시각화
        DrawSelectedCardGridGizmos();
    }

    private void DrawCharacterGridGizmos()
    {
        if (characterScrollContent != null)
        {
            GridLayoutGroup grid = characterScrollContent.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f); // 반투 녹색

                // 그리드 셀 표시
                Vector3 startPos = characterScrollContent.position;
                int columns = maxCharactersPerRow;
                int rows = Mathf.CeilToInt((float)characterButtons.Length / columns);

                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < columns; x++)
                    {
                        Vector3 cellPos = startPos + new Vector3(
                            (x * (characterSize + grid.spacing.x)) + grid.padding.left,
                            -(y * (characterSize + grid.spacing.y)) - grid.padding.top,
                            0
                        );

                        // 셀 영역 표시
                        Gizmos.DrawCube(cellPos + new Vector3(characterSize/2, -characterSize/2, 0), 
                            new Vector3(characterSize, characterSize, 1));

                        // 셀 테두리 표시
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(cellPos + new Vector3(characterSize/2, -characterSize/2, 0), 
                            new Vector3(characterSize, characterSize, 1));
                    }
                }

                // 전체 영역 테두리 표시
                Gizmos.color = Color.yellow;
                float totalWidth = columns * characterSize + (columns - 1) * grid.spacing.x + grid.padding.left + grid.padding.right;
                float totalHeight = rows * characterSize + (rows - 1) * grid.spacing.y + grid.padding.top + grid.padding.bottom;
                Vector3 areaCenter = startPos + new Vector3(totalWidth/2, -totalHeight/2, 0);
                Gizmos.DrawWireCube(areaCenter, new Vector3(totalWidth, totalHeight, 1));
            }
        }
    }

    private void DrawSelectedCardGridGizmos()
    {
        if (selectedCardPanel != null)
        {
            GridLayoutGroup grid = selectedCardPanel.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                Gizmos.color = new Color(0, 0.5f, 1f, 0.3f); // 반투명 파란색

                Vector3 startPos = selectedCardPanel.position;
                Vector2 cardSize = new Vector2(185.6f, 246.4f);  // 카드 크기

                // 그리드 셀 표시
                for (int x = 0; x < selectedCardsPerRow; x++)
                {
                    Vector3 cellPos = startPos + new Vector3(
                        (x * (cardSize.x + selectedCardSpacing)) + selectedPanelPadding,
                        -selectedPanelPadding,
                        0
                    );

                    // 셀 영역 표시
                    Gizmos.DrawCube(cellPos + new Vector3(cardSize.x/2, -cardSize.y/2, 0), 
                        new Vector3(cardSize.x, cardSize.y, 1));

                    // 셀 테두리 표시
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(cellPos + new Vector3(cardSize.x/2, -cardSize.y/2, 0), 
                        new Vector3(cardSize.x, cardSize.y, 1));
                }

                // 전체 영역 테두리 표시
                Gizmos.color = Color.yellow;
                float totalWidth = selectedCardsPerRow * cardSize.x + 
                    (selectedCardsPerRow - 1) * selectedCardSpacing + 
                    selectedPanelPadding * 2;
                float totalHeight = cardSize.y + selectedPanelPadding * 2;
                Vector3 areaCenter = startPos + new Vector3(totalWidth/2, -totalHeight/2, 0);
                Gizmos.DrawWireCube(areaCenter, new Vector3(totalWidth, totalHeight, 1));
            }
        }
    }
    #endif

    private void ShowCharacterInfo(int characterIndex)
    {
        // 이전 정보 패널 제거
        if (currentCharacterInfoPanel != null)
        {
            Destroy(currentCharacterInfoPanel);
        }

        // 캐릭터 프리팹 가져오기
        if (characterPrefabDict.TryGetValue(characterButtons[characterIndex], out GameObject characterPrefab))
        {
            CharacterProfile profile = characterPrefab.GetComponent<CharacterProfile>();
            if (profile != null)
            {
                // 새 정보 패널 생성
                currentCharacterInfoPanel = Instantiate(characterInfoPanelPrefab, characterInfoParent);
                
                // 정보 패널의 텍스트 컴포넌트들 찾기
                TextMeshProUGUI nameText = currentCharacterInfoPanel.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI dmgText = currentCharacterInfoPanel.transform.Find("DmgText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI defText = currentCharacterInfoPanel.transform.Find("DefText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI coinText = currentCharacterInfoPanel.transform.Find("CoinText")?.GetComponent<TextMeshProUGUI>();

                // 텍스트 업데이트
                if (nameText != null) nameText.text = "" + profile.GetPlayer.charName;
                if (dmgText != null) dmgText.text = "" + profile.GetPlayer.dmgLevel;
                if (defText != null) defText.text = "" + profile.GetPlayer.defLevel;
                if (coinText != null) coinText.text = "" + profile.GetPlayer.coin;
            }
        }
    }
}