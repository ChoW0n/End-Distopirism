using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 네임스페이스 추가
using UnityEngine.EventSystems;
using TMPro;
using System.Linq; // LINQ 추가

public class SkillManager : MonoBehaviour
{
    private Image card1Image;
    private Image card2Image;
    private Image card3Image;

    private Button card1Button;
    private Button card2Button;
    private Button card3Button;

    private Skill[] currentSkills = new Skill[3];
    public System.Action<Skill> OnSkillSelected;

    private TextMeshProUGUI card1Text;
    private TextMeshProUGUI card2Text;
    private TextMeshProUGUI card3Text;

    private TextMeshProUGUI card1NameText;
    private TextMeshProUGUI card2NameText;
    private TextMeshProUGUI card3NameText;

    [Header("Card Animation")]
    private float cardZoomScale = 1.5f;  // 확대 시 스케일
    private float zoomDuration = 0.3f;   // 확대/축소 애니메이션 시간
    private Vector3 screenCenter;         // 화면 중앙 위치
    private bool isCardZoomed = false;    // 현재 확대된 카드가 있는지
    private int zoomedCardIndex = -1;     // 현재 확대된 카드의 인덱스

    private Vector3[] originalPositions = new Vector3[3]; // 카드의 원래 위치 저장
    private int selectedCardIndex = -1; // 현재 선택된 카드의 인덱스

    void Start()
    {
        InitializeCardComponents();
        screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }

    private void InitializeCardComponents()
    {
        // PlayerInfoPanel 찾기 시도 (여러 경로)
        GameObject playerInfoPanel = null;
        Transform skillCardsTransform = null;

        // 1. 직접 경로로 찾기
        playerInfoPanel = GameObject.Find("PlayerInfoPanel");

        // 2. UIManager를 통해 찾기
        if (playerInfoPanel == null && UIManager.Instance != null)
        {
            playerInfoPanel = UIManager.Instance.playerProfilePanel;
        }

        // 3. 태그로 찾기
        if (playerInfoPanel == null)
        {
            GameObject[] uiPanels = GameObject.FindGameObjectsWithTag("UIPanel");
            playerInfoPanel = System.Array.Find(uiPanels, panel => panel.name == "PlayerInfoPanel");
        }

        if (playerInfoPanel == null)
        {
            Debug.LogError("PlayerInfoPanel을 찾을 수 없습니다!");
            return;
        }

        // SkillCards 찾기
        skillCardsTransform = playerInfoPanel.transform.Find("SkillCards");
        if (skillCardsTransform == null)
        {
            // 하위 객체들 중에서 SkillCards 찾기
            foreach (Transform child in playerInfoPanel.transform)
            {
                if (child.name.Contains("SkillCards"))
                {
                    skillCardsTransform = child;
                    break;
                }
            }
        }

        if (skillCardsTransform == null)
        {
            Debug.LogError("SkillCards를 찾을 수 없습니다!");
            // 현재 계층 구조 출력
            Debug.LogError("PlayerInfoPanel의 자식 오브젝트들:");
            foreach (Transform child in playerInfoPanel.transform)
            {
                Debug.LogError($"- {child.name}");
            }
            return;
        }

        Debug.Log($"SkillCards를 찾았습니다: {skillCardsTransform.name}");

        // 각 카드의 컴포넌트 초기화
        for (int i = 1; i <= 3; i++)
        {
            Transform cardTransform = skillCardsTransform.Find($"Card{i}");
            if (cardTransform != null)
            {
                // Image와 Button 컴포넌트 설정
                Image cardImage = cardTransform.GetComponent<Image>();
                Button cardButton = cardTransform.GetComponent<Button>();

                // SkillInfo와 SkillName 텍스트 컴포넌트 찾기
                TextMeshProUGUI skillInfoText = cardTransform.Find("SkillInfo")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI skillNameText = cardTransform.Find("SkillName")?.GetComponent<TextMeshProUGUI>();

                // 컴포넌트 할당
                switch (i)
                {
                    case 1:
                        card1Image = cardImage;
                        card1Button = cardButton;
                        card1Text = skillInfoText;
                        card1NameText = skillNameText;
                        break;
                    case 2:
                        card2Image = cardImage;
                        card2Button = cardButton;
                        card2Text = skillInfoText;
                        card2NameText = skillNameText;
                        break;
                    case 3:
                        card3Image = cardImage;
                        card3Button = cardButton;
                        card3Text = skillInfoText;
                        card3NameText = skillNameText;
                        break;
                }

                Debug.Log($"Card{i} 컴포넌트 초기화 - Image: {cardImage != null}, Button: {cardButton != null}, " +
                         $"SkillInfo: {skillInfoText != null}, SkillName: {skillNameText != null}");

                // 각 컴포넌트의 전체 경로 출력
                if (cardImage != null) Debug.Log($"Card{i} Image 경로: {GetFullPath(cardImage.transform)}");
                if (skillInfoText != null) Debug.Log($"Card{i} SkillInfo 경로: {GetFullPath(skillInfoText.transform)}");
                if (skillNameText != null) Debug.Log($"Card{i} SkillName 경로: {GetFullPath(skillNameText.transform)}");
            }
            else
            {
                Debug.LogError($"Card{i}를 찾을 수 없습니다!");
            }
        }

        DeactivateCards();
    }

    // 전체 경로를 가져오는 헬퍼 메서드
    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    public void DeactivateCards()
    {
        if (card1Image != null) card1Image.gameObject.SetActive(false);
        if (card2Image != null) card2Image.gameObject.SetActive(false);
        if (card3Image != null) card3Image.gameObject.SetActive(false);
    }

    public void AssignRandomSkillSprites(CharacterProfile character)
    {
        if (character == null)
        {
            Debug.LogError("캐릭터가 없습니다.");
            return;
        }

        List<Skill> characterCards = character.GetDrawnCards();
        if (characterCards == null || characterCards.Count == 0)
        {
            Debug.LogError($"{character.GetPlayer.charName}의 카드가 없습니다.");
            return;
        }

        // 카드 UI 컴포넌트 배열
        Image[] cardImages = { card1Image, card2Image, card3Image };
        TextMeshProUGUI[] skillInfoTexts = { card1Text, card2Text, card3Text };
        TextMeshProUGUI[] skillNameTexts = { card1NameText, card2NameText, card3NameText };

        // 모든 상태 초기화
        isCardZoomed = false;
        zoomedCardIndex = -1;
        selectedCardIndex = -1;

        // 각 카드 업데이트
        for (int i = 0; i < cardImages.Length; i++)
        {
            if (cardImages[i] != null && i < characterCards.Count)
            {
                // 카드 활성화 및 초기 상태 설정
                cardImages[i].gameObject.SetActive(true);
                cardImages[i].transform.localScale = Vector3.one;
                
                // 투명도 초기화
                Color color = cardImages[i].color;
                color.a = 1f;
                cardImages[i].color = color;

                // 현재 스킬 설정
                currentSkills[i] = characterCards[i];

                // 스프라이트 및 텍스트 업데이트
                cardImages[i].sprite = characterCards[i].nomalSprite;

                if (skillInfoTexts[i] != null)
                {
                    skillInfoTexts[i].text = characterCards[i].skillEffect;
                    skillInfoTexts[i].gameObject.SetActive(true);
                }

                if (skillNameTexts[i] != null)
                {
                    skillNameTexts[i].text = characterCards[i].skillName;
                    skillNameTexts[i].gameObject.SetActive(true);
                }

                // 원래 위치 저장
                originalPositions[i] = cardImages[i].transform.position;
            }
        }

        SetupCardInteractions();
        Debug.LogWarning($"{character.GetPlayer.charName}의 카드 UI 업데이트 완료");
    }

    private void SetupCardInteractions()
    {
        Image[] cardImages = { card1Image, card2Image, card3Image };
        Button[] buttons = { card1Button, card2Button, card3Button };
        
        for (int i = 0; i < cardImages.Length; i++)
        {
            if (cardImages[i] != null)
            {
                int index = i;
                
                // 버튼 컴포넌트 확인 및 추가
                Button button = buttons[i];
                if (button == null)
                {
                    button = cardImages[i].gameObject.AddComponent<Button>();
                    buttons[i] = button;
                }

                // 클릭 이벤트 설정
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnCardClicked(index));

                // EventTrigger 설정
                EventTrigger trigger = cardImages[i].gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = cardImages[i].gameObject.AddComponent<EventTrigger>();
                }
                trigger.triggers.Clear();

                // 우클릭 이벤트 추가
                EventTrigger.Entry rightClickEntry = new EventTrigger.Entry();
                rightClickEntry.eventID = EventTriggerType.PointerClick;
                rightClickEntry.callback.AddListener((data) => {
                    PointerEventData pData = (PointerEventData)data;
                    if (pData.button == PointerEventData.InputButton.Right)
                    {
                        Debug.Log($"Card {index} 우클릭됨");
                        OnCardRightClick(index, cardImages[index]);
                    }
                });
                trigger.triggers.Add(rightClickEntry);

                // 마우스 진입 이벤트 추가
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => {
                    Debug.Log($"Card {index} 마우스 진입");
                    if (!isCardZoomed)
                    {
                        cardImages[index].transform.DOScale(1.1f, 0.2f);
                    }
                });
                trigger.triggers.Add(enterEntry);

                // 마우스 퇴장 이벤트 추가
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => {
                    Debug.Log($"Card {index} 마우스 퇴장");
                    if (!isCardZoomed)
                    {
                        cardImages[index].transform.DOScale(1f, 0.2f);
                    }
                });
                trigger.triggers.Add(exitEntry);

                // 레이캐스트 타겟 설정
                Image cardImage = cardImages[i];
                cardImage.raycastTarget = true;

                // Canvas와 GraphicRaycaster 확인
                Canvas canvas = cardImage.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = cardImage.gameObject.AddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    cardImage.gameObject.AddComponent<GraphicRaycaster>();
                }

                Debug.Log($"Card {index} 상호작용 설정 완료");
            }
        }
    }

    private void OnCardClicked(int index)
    {
        if (index < 0 || index >= currentSkills.Length || currentSkills[index] == null) return;

        // 확대된 상태라면 먼저 축소
        if (isCardZoomed)
        {
            UnzoomCard(index, GetCardImage(index));
            return; // 확대 상태에서는 선택하지 않음
        }

        // 이미 선택된 카드가 있고 적이 선택되지 않은 경우
        if (selectedCardIndex != -1 && BattleManager.Instance.targetObjects.Count == 0)
        {
            // 이전 선택 카드 투명도 복구
            Image previousCard = GetCardImage(selectedCardIndex);
            if (previousCard != null)
            {
                previousCard.DOFade(1f, 0.3f);
            }
        }

        selectedCardIndex = index;
        Skill selectedSkill = currentSkills[index];
        OnSkillSelected?.Invoke(selectedSkill);

        // 선택된 카드 제외하고 나머지 카드 반투명하게
        UpdateCardOpacity();

        BattleManager.Instance.OnSkillSelected();
    }

    private void OnCardRightClick(int index, Image cardImage)
    {
        if (!isCardZoomed)
        {
            // 카드가 확대되어 있지 않은 경우 확대
            ZoomCard(index, cardImage);
        }
        else if (zoomedCardIndex == index)
        {
            // 현재 확대된 카드를 다시 우클릭한 경우 축소
            UnzoomCard(index, cardImage);
        }
        // 다른 카드가 확대되어 있는 경우는 무시
    }

    private void ZoomCard(int index, Image cardImage)
    {
        // 원래 위치 저장
        Vector3 originalPosition = cardImage.transform.position;
        Vector3 originalScale = cardImage.transform.localScale;

        // 화면 중앙 위치 계산
        Vector3 centerPosition = Camera.main.ScreenToWorldPoint(screenCenter);
        centerPosition.z = cardImage.transform.position.z;

        // 다른 카드들 페이드 아웃 및 상호작용 비활성화
        Image[] cards = { card1Image, card2Image, card3Image };
        Button[] buttons = { card1Button, card2Button, card3Button };
        
        for (int i = 0; i < cards.Length; i++)
        {
            if (i != index && cards[i] != null)
            {
                cards[i].DOFade(0.3f, zoomDuration);
                if (buttons[i] != null)
                {
                    buttons[i].interactable = false;
                }
            }
        }

        // 선택된 카드 확대 및 이동
        Sequence zoomSequence = DOTween.Sequence();
        zoomSequence.Join(cardImage.transform.DOMove(centerPosition, zoomDuration).SetEase(Ease.OutQuad));
        zoomSequence.Join(cardImage.transform.DOScale(cardZoomScale, zoomDuration).SetEase(Ease.OutQuad));

        isCardZoomed = true;
        zoomedCardIndex = index;

        // 정렬 순서 조정
        Canvas cardCanvas = cardImage.GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = cardImage.gameObject.AddComponent<Canvas>();
        }
        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = 100;

        // 선택된 카드의 버튼은 계속 활성화 상태 유지
        if (buttons[index] != null)
        {
            buttons[index].interactable = true;
        }
    }

    private void UnzoomCard(int index, Image cardImage)
    {
        if (!isCardZoomed) return;

        // 원래 위치로 되돌리기
        Vector3 originalPosition = originalPositions[index];
        
        // 모든 카드 페이드 인 및 상호작용 활성화
        Image[] cards = { card1Image, card2Image, card3Image };
        Button[] buttons = { card1Button, card2Button, card3Button };
        
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                // 선택된 카드가 있는 경우 해당 투명도 유지
                float targetAlpha = (selectedCardIndex != -1 && i != selectedCardIndex) ? 0.5f : 1f;
                cards[i].DOFade(targetAlpha, zoomDuration);
                if (buttons[i] != null)
                {
                    buttons[i].interactable = true;
                }
            }
        }

        // 선택된 카드 축소 및 원래 위치로 이동
        Sequence unzoomSequence = DOTween.Sequence();
        unzoomSequence.Join(cardImage.transform.DOMove(originalPosition, zoomDuration)
            .SetEase(Ease.OutQuad));
        unzoomSequence.Join(cardImage.transform.DOScale(Vector3.one, zoomDuration)
            .SetEase(Ease.OutQuad));

        // 정렬 순서 복구
        Canvas cardCanvas = cardImage.GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = 0;
        }

        isCardZoomed = false;
        zoomedCardIndex = -1;
    }

    private Vector3 GetOriginalPosition(int index)
    {
        // 각 카드의 원래 위치 반환
        Transform skillCardsTransform = GameObject.Find("PlayerInfoPanel")?.transform.Find("SkillCards");
        if (skillCardsTransform != null)
        {
            Transform cardTransform = skillCardsTransform.Find($"Card{index + 1}");
            if (cardTransform != null)
            {
                return cardTransform.position;
            }
        }
        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (card1Button != null) card1Button.onClick.RemoveAllListeners();
        if (card2Button != null) card2Button.onClick.RemoveAllListeners();
        if (card3Button != null) card3Button.onClick.RemoveAllListeners();
    }

    // RefreshCards 메서드 수정
    public void RefreshCards(CharacterProfile character)
    {
        if (character == null)
        {
            Debug.LogError("캐릭터가 없습니다.");
            return;
        }

        // 기존 카드들 비활성화
        DeactivateCards();

        // 모든 상태 초기화
        isCardZoomed = false;
        zoomedCardIndex = -1;
        selectedCardIndex = -1;

        // 모든 카드의 투명도를 즉시 1로 설정
        Image[] cards = { card1Image, card2Image, card3Image };
        foreach (var card in cards)
        {
            if (card != null)
            {
                Color color = card.color;
                color.a = 1f;
                card.color = color;
            }
        }

        // 새로운 카드 할당
        AssignRandomSkillSprites(character);
    }

    // 헬퍼 메서드 추가
    private Image GetCardImage(int index)
    {
        switch (index)
        {
            case 0: return card1Image;
            case 1: return card2Image;
            case 2: return card3Image;
            default: return null;
        }
    }

    private void UpdateCardOpacity()
    {
        Image[] cards = { card1Image, card2Image, card3Image };
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                // 확대된 카드가 있는 경우는 투명도 변경하지 않음
                if (!isCardZoomed)
                {
                    float targetAlpha = (i == selectedCardIndex) ? 1f : 0.5f;
                    cards[i].DOFade(targetAlpha, 0.3f);
                }
            }
        }
    }

    private void ResetCardOpacity()
    {
        Image[] cards = { card1Image, card2Image, card3Image };
        foreach (var card in cards)
        {
            if (card != null)
            {
                // 투명도를 즉시 1로 설정
                Color color = card.color;
                color.a = 1f;
                card.color = color;
            }
        }
        selectedCardIndex = -1;
    }

    // 캐릭터 선택 시 호출될 메서드
    public void OnCharacterSelected()
    {
        // 확대된 카드가 있다면 먼저 축소
        if (isCardZoomed && zoomedCardIndex >= 0)
        {
            Image zoomedCard = GetCardImage(zoomedCardIndex);
            if (zoomedCard != null)
            {
                UnzoomCard(zoomedCardIndex, zoomedCard);
            }
        }

        selectedCardIndex = -1;
        ResetCardOpacity();
    }
}
