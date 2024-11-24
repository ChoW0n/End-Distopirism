using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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
    [SerializeField] private float characterPanelHeight = 120f;
    [SerializeField] private Vector2 characterImageSize = new Vector2(80f, 80f);
    
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
    [SerializeField] private float cardSize = 100f;
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
        characterSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        characterSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Resources/Characters 폴더에서 모든 캐릭터 프리팹 로드
        GameObject[] characterPrefabs = Resources.LoadAll<GameObject>("Characters");
        
        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Resources/Characters 폴더에서 캐릭터 프리팹을 찾을 수 없습니다.");
            return;
        }
        
        // 유효한 캐릭터 프리팹만 필터링
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
                    // CharacterProfile의 Player 정보에서 스프라이트 가져오기
                    buttonImage.sprite = characterProfile.GetPlayer.charSprite; // Player 클래스에 charSprite 필드 추가 필요
                    buttonImage.preserveAspect = true;
                }

                // 캐릭터 이름 설정
                TextMeshProUGUI nameText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = characterProfile.GetPlayer.charName;
                }

                // 딕셔너리에 프리팹 참조 저장
                characterPrefabDict.Add(button, prefab);
                characterButtons[i] = button;
            }
        }
    }

    private void LoadSkillCards()
    {
        // Content의 GridLayoutGroup 설정
        GridLayoutGroup gridLayout = cardScrollContent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = cardScrollContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 상단에서 아래로 카드가 생성되도록 설정
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.cellSize = new Vector2(cardSize, cardSize);
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
        
        foreach (Skill skill in skills)
        {
            GameObject cardButtonObj = Instantiate(cardButtonPrefab, cardScrollContent);
            Button cardButton = cardButtonObj.GetComponent<Button>();
            
            Image cardImage = cardButtonObj.GetComponent<Image>();
            if (cardImage != null && skill.nomalSprite != null)
            {
                cardImage.sprite = skill.nomalSprite;
                cardImage.preserveAspect = true;
            }
            
            // Count 텍스트 초기화
            Transform countTransform = cardButtonObj.transform.Find("Count");
            if (countTransform != null)
            {
                TextMeshProUGUI countText = countTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (countText != null)
                {
                    // 현재 보유한 카드 수를 표시
                    int currentCount = GetCurrentCardCount(skill);
                    countText.text = $"{currentCount}/{MAX_CARD_COUNT}";
                }
            }
            
            cardButton.gameObject.AddComponent<SkillReference>().skill = skill;
            cardButtons.Add(cardButton);
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
            if (currentCount >= MAX_CARD_COUNT)
            {
                Debug.Log($"{selectedSkill.skillName} 카드는 최대 {MAX_CARD_COUNT}장까지만 추가할 수 있습니다.");
                return;
            }

            cardCountDict[selectedSkill]++;
            UpdateCardCount(selectedCardDict[selectedSkill], cardCountDict[selectedSkill]);
        }
        else // 새로운 카드 추가
        {
            GameObject newCard = Instantiate(cardPrefab, selectedCardPanel);
            selectedCards.Add(newCard);
            selectedCardDict.Add(selectedSkill, newCard);
            cardCountDict.Add(selectedSkill, 1);
            
            RectTransform rectTransform = newCard.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = cardImageSize;
            }
            
            Image cardImage = newCard.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.sprite = selectedSkill.nomalSprite;
                cardImage.preserveAspect = true;
            }
            
            newCard.AddComponent<SkillReference>().skill = selectedSkill;
            
            // 카운트 텍스트 초기화
            InitializeCardCount(newCard);
            
            Button cardButton = newCard.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => UnselectCard(newCard));
            }
        }

        // 카드 선택 후 모든 카드의 카운트 텍스트 업데이트
        UpdateAllCardCounts();
    }

    private void InitializeCardCount(GameObject card)
    {
        // Count 오브젝트 찾기
        Transform countTransform = card.transform.Find("Count");
        if (countTransform != null)
        {
            // CountText (TMP) 컴포넌트 찾기
            TextMeshProUGUI countText = countTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = "1";
            }
        }
    }

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
        GridLayoutGroup gridLayout = selectedCharacterPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = selectedCharacterPanel.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 수평 방향으로 캐릭터 배치
        gridLayout.cellSize = new Vector2(characterSize, characterSize);
        gridLayout.spacing = new Vector2(characterSpacing, characterSpacing);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;  // 수평 방향 우선
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = maxCharactersPerRow;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

        // ContentSizeFitter 제거 (패널 크기 고정)
        ContentSizeFitter sizeFitter = selectedCharacterPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            Destroy(sizeFitter);
        }

        // 기존에 생성된 캐릭터들의 위치 재조정
        for (int i = 0; i < selectedCharacterObjects.Count; i++)
        {
            if (selectedCharacterObjects[i] != null)
            {
                selectedCharacterObjects[i].transform.SetSiblingIndex(i);
            }
        }
    }

    private void SetupSelectedCardPanel()
    {
        RectTransform panelRect = selectedCardPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, cardPanelHeight);
        }

        GridLayoutGroup gridLayout = selectedCardPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = selectedCardPanel.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.cellSize = new Vector2(cardSize, cardSize);
        gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridLayout.constraintCount = 1;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter sizeFitter = selectedCardPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = selectedCardPanel.gameObject.AddComponent<ContentSizeFitter>();
        }
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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
        
        // 카드 선택 패널 활성화
        cardSelectPanel.SetActive(true);
        
        // 기존 선택된 카드들 초기화
        ClearSelectedCards();
        
        // 해당 캐릭터의 덱 정보 로드
        LoadExistingDeck(characterIndex);

        // 버튼 하이라이트 효과
        UpdateCharacterButtonsHighlight();
    }

    // 더블 클릭: 전투 대상으로 추가
    private void OnCharacterDoubleClick(int characterIndex)
    {
        if (!selectedCharacterIndices.Contains(characterIndex))
        {
            // 캐릭터 선택 제한 확인
            if (selectedCharacterIndices.Count >= maxSelectableCharacters)
            {
                Debug.LogWarning($"스테이지 {DeckData.currentStage}에서는 최대 {maxSelectableCharacters}명의 캐릭터만 선택할 수 있습니다.");
                return;
            }

            // 전투 대상으로 추가
            selectedCharacterIndices.Add(characterIndex);
            CreateSelectedCharacterIcon(characterIndex);
            
            // 선택된 프리팹 저장
            if (characterPrefabDict.TryGetValue(characterButtons[characterIndex], out GameObject prefab))
            {
                DeckData.selectedCharacterPrefabs.Add(prefab);
            }

            // 시작 버튼 상태 업데이트
            UpdateStartButtonState();
        }
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
                
                // 전투 대상으로 선택된 캐릭터들은 다른 색상으로 표시
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
        selectedCharacterObjects.Add(selectedChar);
        
        selectedChar.transform.SetSiblingIndex(selectedCharacterObjects.Count - 1);
        
        RectTransform rectTransform = selectedChar.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = characterImageSize;
        }
        
        Image charImage = selectedChar.GetComponent<Image>();
        if (charImage != null)
        {
            charImage.sprite = characterButtons[characterIndex].image.sprite;
            charImage.preserveAspect = true;
        }
        
        Button charButton = selectedChar.GetComponent<Button>();
        if (charButton != null)
        {
            charButton.onClick.AddListener(() => OnSelectedCharacterClick(characterIndex));
        }

        UpdateCharacterButtonsHighlight();
    }

    private void UnselectCharacter(int characterIndex)
    {
        // 선택된 캐릭터 목록에서 해당 캐릭터의 인덱스 찾기
        int index = selectedCharacterIndices.IndexOf(characterIndex);
        if (index != -1 && index < selectedCharacterObjects.Count)
        {
            try
            {
                selectedCharacterIndices.RemoveAt(index);
                
                // 선택된 캐릭터 오브젝트가 유효한지 확인
                GameObject charObject = selectedCharacterObjects[index];
                if (charObject != null)
                {
                    selectedCharacterObjects.RemoveAt(index);
                    Destroy(charObject);
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

                // 선택 해제 시 DeckData에서도 제거
                if (index != -1 && index < DeckData.selectedCharacterPrefabs.Count)
                {
                    DeckData.selectedCharacterPrefabs.RemoveAt(index);
                }

                // 시작 버튼 상태 업데이트
                UpdateStartButtonState();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"캐릭터 선택 해제 중 오류 발생: {e.Message}");
                // 오류 발생 시 리스트 초기화
                selectedCharacterIndices.Clear();
                foreach (var obj in selectedCharacterObjects)
                {
                    if (obj != null) Destroy(obj);
                }
                selectedCharacterObjects.Clear();
                currentCharacterIndex = -1;
                cardSelectPanel.SetActive(false);
                ClearSelectedCards();
            }
        }
        else
        {
            Debug.LogWarning($"선택된 캐릭터를 찾을 수 없습니다. CharacterIndex: {characterIndex}, SelectedIndex: {index}");
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
                        countText.text = $"{currentCount}/{MAX_CARD_COUNT}";
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
            Debug.Log($"캐릭터 {characterProfile.GetPlayer.charName}의 덱이 저장되었습니다.");
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
}