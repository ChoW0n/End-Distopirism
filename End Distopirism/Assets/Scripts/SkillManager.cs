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
    // 스킬 카드의 기본 스프라이트와 성공 시 사용할 스프라이트를 관리
    public Sprite defaultSprite;
    public Sprite successSprite;

    private Image card1Image;
    private Image card2Image;
    private Image card3Image;

    private Button card1Button;
    private Button card2Button;
    private Button card3Button;

    private Skill[] currentSkills = new Skill[3];

    // 스킬 선택 콜백을 public으로 선언
    public System.Action<Skill> OnSkillSelected;

    [SerializeField] private float cardHoverScale = 1.2f; // 마우스 오버 시 확대 크기
    [SerializeField] private float cardAnimationDuration = 0.3f; // 애니메이션 지속 시간

    private Vector3[] originalPositions = new Vector3[3]; // 카드의 원래 위치 저장

    private bool isCardCentered = false; // 카드가 중앙에 있는지 여부
    private int centeredCardIndex = -1; // 현재 중앙에 있는 카드의 인덱스

    private TextMeshProUGUI card1Text;
    private TextMeshProUGUI card2Text;
    private TextMeshProUGUI card3Text;

    void Start()
    {
        // DOTween 용량 설정을 더 크게 수정
        DOTween.SetTweensCapacity(3000, 300);  // tweens와 sequences 모두 크게 증가

        // SkillCards 찾기 (모든 가능한 경로 시도)
        Transform skillCardsTransform = null;
        GameObject[] possiblePaths = {
            GameObject.Find("Canvas/PlayerInfoPanel/SkillCards"),
            GameObject.Find("PlayerInfoPanel/SkillCards"),
            GameObject.Find("SkillCards")
        };

        // PlayerInfoPanel을 직접 찾아서 시도
        GameObject playerInfoPanel = GameObject.Find("PlayerInfoPanel");
        if (playerInfoPanel != null)
        {
            Transform skillCards = playerInfoPanel.transform.Find("SkillCards");
            if (skillCards != null)
            {
                skillCardsTransform = skillCards;
                Debug.Log($"PlayerInfoPanel에서 SkillCards를 찾았습니다: {GetFullPath(skillCards)}");
            }
        }

        // 이전 방법으로도 시도
        if (skillCardsTransform == null)
        {
            foreach (var obj in possiblePaths)
            {
                if (obj != null)
                {
                    skillCardsTransform = obj.transform;
                    Debug.Log($"경로에서 SkillCards를 찾았습니다: {GetFullPath(obj.transform)}");
                    break;
                }
            }
        }

        // UIManager를 통해 시도
        if (skillCardsTransform == null && UIManager.Instance != null && UIManager.Instance.playerProfilePanel != null)
        {
            Transform skillCards = UIManager.Instance.playerProfilePanel.transform.Find("SkillCards");
            if (skillCards != null)
            {
                skillCardsTransform = skillCards;
                Debug.Log($"UIManager를 통해 SkillCards를 찾았습니다: {GetFullPath(skillCards)}");
            }
        }

        if (skillCardsTransform == null)
        {
            Debug.LogError("어떤 방법으로도 SkillCards를 찾을 수 없습니다!");
            return;
        }

        // 카드 이미지와 버튼 찾기
        Transform card1Transform = skillCardsTransform.Find("Card1");
        Transform card2Transform = skillCardsTransform.Find("Card2");
        Transform card3Transform = skillCardsTransform.Find("Card3");

        Debug.Log($"Card1 Transform: {(card1Transform != null ? "찾음" : "못찾음")}");
        Debug.Log($"Card2 Transform: {(card2Transform != null ? "찾음" : "못찾음")}");
        Debug.Log($"Card3 Transform: {(card3Transform != null ? "찾음" : "못찾음")}");

        if (card1Transform != null) 
        {
            card1Image = card1Transform.GetComponent<Image>();
            Debug.Log($"Card1 Image: {(card1Image != null ? "있음" : "없음")}");
        }
        if (card2Transform != null) 
        {
            card2Image = card2Transform.GetComponent<Image>();
            Debug.Log($"Card2 Image: {(card2Image != null ? "있음" : "없음")}");
        }
        if (card3Transform != null) 
        {
            card3Image = card3Transform.GetComponent<Image>();
            Debug.Log($"Card3 Image: {(card3Image != null ? "있음" : "없음")}");
        }

        if (card1Image != null) card1Button = card1Image.GetComponent<Button>();
        if (card2Image != null) card2Button = card2Image.GetComponent<Button>();
        if (card3Image != null) card3Button = card3Image.GetComponent<Button>();

        // 디버그 로그
        Debug.Log($"Card1 상태: Image={card1Image != null}, Button={card1Button != null}");
        Debug.Log($"Card2 상태: Image={card2Image != null}, Button={card2Button != null}");
        Debug.Log($"Card3 상태: Image={card3Image != null}, Button={card3Button != null}");

        // 버튼 리스너 설정
        if (card1Button) card1Button.onClick.AddListener(() => OnCardClicked(0));
        if (card2Button) card2Button.onClick.AddListener(() => OnCardClicked(1));
        if (card3Button) card3Button.onClick.AddListener(() => OnCardClicked(2));

        DeactivateCards();

        // EventSystem이 없다면 추가
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // 카드 텍스트 컴포넌트 찾기
        if (skillCardsTransform != null)
        {
            // Text(TMP) 컴포넌트 찾기
            if (card1Image != null) 
            {
                Transform textTransform = null;
                string childNames = "";
                foreach (Transform child in card1Image.transform)
                {
                    childNames += child.name + ", ";
                    if (child.name.Equals("SkillInfo", System.StringComparison.OrdinalIgnoreCase))
                    {
                        textTransform = child;
                        break;
                    }
                }

                if (textTransform != null)
                {
                    card1Text = textTransform.GetComponent<TextMeshProUGUI>();
                    Debug.Log($"Card1 Text Transform 경로: {GetFullPath(textTransform)}");
                }
                else
                {
                    Debug.LogError($"Card1의 SkillInfo 오브젝트를 찾을 수 없습니다. 현재 자식 오브젝트들: {childNames}");
                }
            }

            if (card2Image != null)
            {
                Transform textTransform = null;
                string childNames = "";
                foreach (Transform child in card2Image.transform)
                {
                    childNames += child.name + ", ";
                    if (child.name.Equals("SkillInfo", System.StringComparison.OrdinalIgnoreCase))
                    {
                        textTransform = child;
                        break;
                    }
                }

                if (textTransform != null)
                {
                    card2Text = textTransform.GetComponent<TextMeshProUGUI>();
                    Debug.Log($"Card2 Text Transform 경로: {GetFullPath(textTransform)}");
                }
                else
                {
                    Debug.LogError($"Card2의 SkillInfo 오브젝트를 찾을 수 없습니다. 현재 자식 오브젝트들: {childNames}");
                }
            }

            if (card3Image != null)
            {
                Transform textTransform = null;
                string childNames = "";
                foreach (Transform child in card3Image.transform)
                {
                    childNames += child.name + ", ";
                    if (child.name.Equals("SkillInfo", System.StringComparison.OrdinalIgnoreCase))
                    {
                        textTransform = child;
                        break;
                    }
                }

                if (textTransform != null)
                {
                    card3Text = textTransform.GetComponent<TextMeshProUGUI>();
                    Debug.Log($"Card3 Text Transform 경로: {GetFullPath(textTransform)}");
                }
                else
                {
                    Debug.LogError($"Card3의 SkillInfo 오브젝트를 찾을 수 없습니다. 현재 자식 오브젝트들: {childNames}");
                }
            }

            Debug.Log($"Card1 Text: {(card1Text != null ? "찾음" : "못찾음")}");
            Debug.Log($"Card2 Text: {(card2Text != null ? "찾음" : "못찾음")}");
            Debug.Log($"Card3 Text: {(card3Text != null ? "찾음" : "못찾음")}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 모든 카드 이미지를 비활성화합니다.
    /// </summary>
    public void DeactivateCards()
    {
        Image[] cards = { card1Image, card2Image, card3Image };
        float delay = 0.1f;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null || !cards[i].gameObject.activeSelf)
            {
                continue;
            }

            int index = i;
            CanvasGroup canvasGroup = cards[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = cards[i].gameObject.AddComponent<CanvasGroup>();
            }

            try
            {
                // 기존 Tween 제거
                DOTween.Kill(cards[i].transform, true);
                DOTween.Kill(canvasGroup, true);

                // 새로운 시퀀스 생성
                Sequence cardSequence = DOTween.Sequence()
                    .SetAutoKill(true)  // 자동 정리 활성화
                    .OnComplete(() => {
                        if (cards[index] != null && cards[index].gameObject != null)
                        {
                            cards[index].gameObject.SetActive(false);
                        }
                    });

                // 애니메이션 추가
                cardSequence.Join(cards[i].transform
                    .DOScale(Vector3.zero, 0.3f)
                    .SetDelay(delay * i)
                    .SetEase(Ease.InBack));

                cardSequence.Join(cards[i].transform
                    .DORotate(new Vector3(0, 180f, 0), 0.3f)
                    .SetDelay(delay * i)
                    .SetEase(Ease.InQuad));

                cardSequence.Join(canvasGroup
                    .DOFade(0f, 0.3f)
                    .SetDelay(delay * i)
                    .SetEase(Ease.InQuad));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DeactivateCards 에러 (카드 {index}): {e.Message}");
            }
        }
    }

    /// <summary>
    /// 플레이어의 스킬 목록에서 랜덤하게 3개를 선택하여 카드에 스프라이트를 할당합니다.
    /// </summary>
    /// <param name="playerSkills">선택된 플레이어의 스킬 리스트</param>
    public void AssignRandomSkillSprites(List<Skill> playerSkills)
    {
        if (playerSkills == null || playerSkills.Count == 0 || 
            card1Image == null || card2Image == null || card3Image == null)
        {
            Debug.LogError("필요한 참조가 누락되었습니다.");
            return;
        }

        // 스킬 선택 및 할당
        currentSkills = new Skill[3];
        Image[] cards = { card1Image, card2Image, card3Image };
        TextMeshProUGUI[] cardTexts = { card1Text, card2Text, card3Text };

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, playerSkills.Count);
            currentSkills[i] = playerSkills[randomIndex];
            
            if (currentSkills[i] == null) 
            {
                Debug.LogError($"Card {i+1}의 스킬이 null입니다.");
                continue;
            }

            int index = i;
            cards[i].gameObject.SetActive(true);
            cards[i].sprite = currentSkills[i].nomalSprite;
            
            // 스킬 이펙트 텍스트 설정 및 디버그
            if (cardTexts[i] != null)
            {
                // SkillInfo 텍스트 설정
                cardTexts[i].text = currentSkills[i].skillEffect;
                cardTexts[i].gameObject.SetActive(true);
                
                // SkillName 텍스트 찾기 및 설정
                Transform skillNameTransform = cards[i].transform.Find("SkillName");
                if (skillNameTransform != null)
                {
                    TextMeshProUGUI skillNameText = skillNameTransform.GetComponent<TextMeshProUGUI>();
                    if (skillNameText != null)
                    {
                        skillNameText.text = currentSkills[i].skillName;
                        skillNameText.gameObject.SetActive(true);
                        Debug.Log($"Card {i+1} 스킬 이름 설정: {currentSkills[i].skillName}");
                    }
                    else
                    {
                        Debug.LogError($"Card {i+1}의 SkillName TMP 컴포넌트를 찾을 수 없습니다.");
                    }
                }
                else
                {
                    Debug.LogError($"Card {i+1}의 SkillName 오브젝트를 찾을 수 없습니다.");
                }
                
                Debug.Log($"Card {i+1} 스킬 효과 설정: {currentSkills[i].skillEffect}");
            }
            else
            {
                Debug.LogError($"Card {i+1}의 SkillInfo Text 컴포넌트를 찾을 수 없습니다.");
            }
            
            // 초기 위치 저장
            originalPositions[i] = cards[i].transform.position;
            
            // 초기 설정
            cards[i].transform.localScale = Vector3.zero;

            // 카드 생성 애니메이션
            float delay = 0.2f * i;
            try
            {
                cards[i].transform.DOScale(Vector3.one, cardAnimationDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"카드 {i+1} 생성 애니메이션 에러: {e.Message}");
            }

            // 마우스 이벤트 설정
            EventTrigger trigger = cards[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = cards[i].gameObject.AddComponent<EventTrigger>();
            }
            trigger.triggers.Clear();

            // 마우스 진입 벤트 (마우스 오버 시 확대만)
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => {
                if (!isCardCentered) // 카드가 중앙에 없을 때만 확대
                {
                    try
                    {
                        DOTween.Kill(cards[index].transform);
                        cards[index].transform.DOScale(cardHoverScale, 0.3f).SetEase(Ease.OutQuad);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"카드 {index+1} 마우스 오버 에러: {e.Message}");
                    }
                }
            });
            trigger.triggers.Add(enterEntry);

            // 마우스 퇴장 이벤트
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => {
                if (!isCardCentered) // 카드가 중앙에 없을 때만 축소
                {
                    try
                    {
                        DOTween.Kill(cards[index].transform);
                        cards[index].transform.DOScale(1f, 0.3f).SetEase(Ease.OutQuad);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"카드 {index+1} 마우스 아웃 에러: {e.Message}");
                    }
                }
            });
            trigger.triggers.Add(exitEntry);

            // 우클릭 이벤트
            EventTrigger.Entry rightClickEntry = new EventTrigger.Entry();
            rightClickEntry.eventID = EventTriggerType.PointerClick;
            rightClickEntry.callback.AddListener((data) => {
                PointerEventData pData = (PointerEventData)data;
                if (pData.button == PointerEventData.InputButton.Right)
                {
                    HandleRightClick(index, cards[index]);
                }
            });
            trigger.triggers.Add(rightClickEntry);

            // 좌클릭 이벤트 (카드 선택)
            Button button = cards[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    // 중앙에 있거나 기본 상태일 때 선택 가능
                    if (!isCardCentered || (isCardCentered && centeredCardIndex == index))
                    {
                        SelectCard(index);
                    }
                });
            }
        }
    }

    /// <summary>
    /// 시전 성공 시 스프라이트를 변경하는 메서드
    /// </summary>
    /// <param name="skill">변경할 스킬</param>
    /// <param name="isSuccess">시전 성공 여부</param>
    public void UpdateSkillSprite(Skill skill, bool isSuccess)
    {
        // 예시: 스킬 카드 UI 요소의 Image 컴포넌트를 가져와서 스프라이트를 변경
        // Image skillImage = /* 스킬 카드의 Image 컴포넌트 참조 */;
        // skillImage.sprite = isSuccess ? skill.successSprite : skill.nomalSprite;
    }

    private void OnCardClicked(int index)
    {
        if (index < 0 || index >= currentSkills.Length || currentSkills[index] == null) return;

        Skill selectedSkill = currentSkills[index];
        OnSkillSelected?.Invoke(selectedSkill);
        
        Image[] cards = { card1Image, card2Image, card3Image };
        
        // 선택되지 않은 카드들 페이드 아웃
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;
            
            if (i != index)
            {
                try
                {
                    cards[i].transform.DOScale(0f, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => {
                            if (cards[i] != null && cards[i].gameObject != null)
                            {
                                cards[i].gameObject.SetActive(false);
                            }
                        });
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"카드 {i+1} 페이드 아웃 에러: {e.Message}");
                }
            }
            else
            {
                try
                {
                    // 선택된 카드도 페이드 아웃
                    cards[i].transform.DOScale(0f, 0.3f)
                        .SetEase(Ease.InBack)
                        .SetDelay(0.2f) // 약간의 딜레이 후 페이드 아웃
                        .OnComplete(() => {
                            if (cards[i] != null && cards[i].gameObject != null)
                            {
                                cards[i].gameObject.SetActive(false);
                            }
                        });
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"선택된 카드 {i+1} 페이드 아웃 에러: {e.Message}");
                }
            }
        }

        // BattleManager에 스킬 선택 완료 알림
        BattleManager.Instance.OnSkillSelected();
    }

    private void HandleRightClick(int index, Image card)
    {
        if (!isCardCentered || (isCardCentered && centeredCardIndex != index))
        {
            // 다른 카드가 중앙에 있다면 원위치로
            if (isCardCentered && centeredCardIndex != index)
            {
                Image[] cards = { card1Image, card2Image, card3Image };
                ReturnCardToOriginalPosition(centeredCardIndex, cards[centeredCardIndex]);
            }

            // 선택한 카드를 중앙으로
            MoveCardToCenter(index, card);
            isCardCentered = true;
            centeredCardIndex = index;
        }
        else
        {
            // 이미 중앙에 있는 카드를 다시 우클릭하면 원위치로
            ReturnCardToOriginalPosition(index, card);
            isCardCentered = false;
            centeredCardIndex = -1;
        }
    }

    private void MoveCardToCenter(int index, Image card)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 centerPosition = Camera.main.ScreenToWorldPoint(screenCenter);
        centerPosition.z = card.transform.position.z;

        DOTween.Kill(card.transform);
        
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Join(card.transform.DOMove(centerPosition, 0.5f).SetEase(Ease.OutCubic));
        cardSequence.Join(card.transform.DOScale(cardHoverScale * 1.2f, 0.5f).SetEase(Ease.OutCubic));

        Canvas cardCanvas = card.GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = 10;
        }
    }

    private void ReturnCardToOriginalPosition(int index, Image card)
    {
        if (index >= 0 && index < originalPositions.Length)
        {
            DOTween.Kill(card.transform);
            
            Sequence returnSequence = DOTween.Sequence();
            returnSequence.Join(card.transform.DOMove(originalPositions[index], 0.5f).SetEase(Ease.OutCubic));
            returnSequence.Join(card.transform.DOScale(1f, 0.5f).SetEase(Ease.OutCubic));

            Canvas cardCanvas = card.GetComponent<Canvas>();
            if (cardCanvas != null)
            {
                cardCanvas.sortingOrder = 1;
            }
        }
    }

    private void SelectCard(int index)
    {
        if (index < 0 || index >= currentSkills.Length || currentSkills[index] == null) return;

        Skill selectedSkill = currentSkills[index];
        OnSkillSelected?.Invoke(selectedSkill);
        
        Image[] cards = { card1Image, card2Image, card3Image };
        
        // 선택되지 않은 카드들 페이드 아웃
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;
            
            if (i != index)
            {
                cards[i].transform.DOScale(0f, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => cards[i].gameObject.SetActive(false));
            }
            else
            {
                // 선택된 카드도 페이드 아웃
                cards[i].transform.DOScale(0f, 0.3f)
                    .SetEase(Ease.InBack)
                    .SetDelay(0.2f) // 약간의 딜레이 후 페이드 아웃
                    .OnComplete(() => cards[i].gameObject.SetActive(false));
            }
        }

        // BattleManager에 스킬 선택 완료 알림
        BattleManager.Instance.OnSkillSelected();
    }

    private void OnDestroy()
    {
        try
        {
            // 모든 Tween 정리
            DOTween.KillAll();

            // 각 카드별 Tween 정리
            CleanupCardTweens(card1Image);
            CleanupCardTweens(card2Image);
            CleanupCardTweens(card3Image);

            // 이벤트 리스너 제거
            RemoveAllCardListeners();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OnDestroy 에러: {e.Message}");
        }
    }

    private void CleanupCardTweens(Image cardImage)
    {
        if (cardImage != null && cardImage.gameObject != null)
        {
            try
            {
                // Transform Tween 정리
                DOTween.Kill(cardImage.transform, true);

                // CanvasGroup Tween 정리
                var canvasGroup = cardImage.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    DOTween.Kill(canvasGroup, true);
                }

                // EventTrigger 제거
                var trigger = cardImage.GetComponent<EventTrigger>();
                if (trigger != null)
                {
                    trigger.triggers.Clear();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CleanupCardTweens 에러: {e.Message}");
            }
        }
    }

    private void RemoveAllCardListeners()
    {
        try
        {
            if (card1Button != null) card1Button.onClick.RemoveAllListeners();
            if (card2Button != null) card2Button.onClick.RemoveAllListeners();
            if (card3Button != null) card3Button.onClick.RemoveAllListeners();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"RemoveAllCardListeners 에러: {e.Message}");
        }
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
}
