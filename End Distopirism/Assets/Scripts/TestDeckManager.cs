using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestDeckManager : MonoBehaviour
{
    [Header("캐릭터 선택 버튼")]
    public Button[] characterButtons;
    
    [Header("선택된 캐릭터 UI")]
    public Transform selectedCharacterPanel;
    public GameObject characterPrefab;
    public float characterSpacing = 10f;
    public float characterSize = 100f;
    public int maxCharactersPerRow = 5;
    public float characterPanelHeight = 120f;
    public Vector2 characterImageSize = new Vector2(80f, 80f);
    
    [Header("카드 선택 UI")]
    public GameObject cardSelectPanel;
    public Button[] cardButtons;
    
    [Header("선택된 카드 UI")]
    public Transform selectedCardPanel;
    public GameObject cardPrefab;
    public float cardSpacing = 10f;
    public float cardSize = 100f;
    public int maxCardsPerRow = 5;
    public float cardPanelHeight = 120f;
    public Vector2 cardImageSize = new Vector2(80f, 80f);
    
    private List<int> selectedCharacterIndices = new List<int>();
    private List<GameObject> selectedCharacterObjects = new List<GameObject>();
    private List<GameObject> selectedCards = new List<GameObject>();
    private int currentCharacterIndex = -1;
    
    private void Start()
    {
        cardSelectPanel.SetActive(false);
        SetupSelectedCharacterPanel();
        SetupSelectedCardPanel();
        InitializeButtons();
    }

    private void SetupSelectedCharacterPanel()
    {
        RectTransform panelRect = selectedCharacterPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, characterPanelHeight);
        }

        GridLayoutGroup gridLayout = selectedCharacterPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = selectedCharacterPanel.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.cellSize = new Vector2(characterSize, characterSize);
        gridLayout.spacing = new Vector2(characterSpacing, characterSpacing);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = maxCharactersPerRow;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

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
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = maxCardsPerRow;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter sizeFitter = selectedCardPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            Destroy(sizeFitter);
        }
    }
    
    private void InitializeButtons()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => OnCharacterSelected(index));
        }
        
        for (int i = 0; i < cardButtons.Length; i++)
        {
            int index = i;
            cardButtons[i].onClick.AddListener(() => OnCardSelected(index));
        }
    }
    
    private void OnCharacterSelected(int characterIndex)
    {
        if (selectedCharacterIndices.Contains(characterIndex))
        {
            // 이미 선택된 캐릭터면 선택 해제
            UnselectCharacter(characterIndex);
            return;
        }

        // 새로운 캐릭터 선택
        selectedCharacterIndices.Add(characterIndex);
        characterButtons[characterIndex].image.color = Color.gray;
        
        // 선택된 캐릭터 패널에 캐릭터 추가
        CreateSelectedCharacterIcon(characterIndex);
        
        // 현재 선택된 캐릭터 설정
        currentCharacterIndex = characterIndex;
        
        // 카드 선택 패널 표시
        cardSelectPanel.SetActive(true);
        
        // 기존에 선택된 카드들 제거
        ClearSelectedCards();
    }

    private void CreateSelectedCharacterIcon(int characterIndex)
    {
        GameObject selectedChar = Instantiate(characterPrefab, selectedCharacterPanel);
        selectedCharacterObjects.Add(selectedChar);
        
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
            charButton.onClick.AddListener(() => UnselectCharacter(characterIndex));
        }
    }

    private void UnselectCharacter(int characterIndex)
    {
        int index = selectedCharacterIndices.IndexOf(characterIndex);
        if (index != -1)
        {
            selectedCharacterIndices.RemoveAt(index);
            
            GameObject charObject = selectedCharacterObjects[index];
            selectedCharacterObjects.RemoveAt(index);
            Destroy(charObject);
            
            characterButtons[characterIndex].image.color = Color.white;
            
            if (currentCharacterIndex == characterIndex)
            {
                currentCharacterIndex = -1;
                cardSelectPanel.SetActive(false);
                ClearSelectedCards();
            }
        }
    }
    
    private void OnCardSelected(int cardIndex)
    {
        if (currentCharacterIndex == -1) return;
        
        GameObject newCard = Instantiate(cardPrefab, selectedCardPanel);
        selectedCards.Add(newCard);
        
        RectTransform rectTransform = newCard.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = cardImageSize;
        }
        
        Image cardImage = newCard.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = cardButtons[cardIndex].image.sprite;
            cardImage.preserveAspect = true;
        }
        
        Button cardButton = newCard.GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(() => UnselectCard(newCard));
        }
    }
    
    private void UnselectCard(GameObject card)
    {
        selectedCards.Remove(card);
        Destroy(card);
    }
    
    private void ClearSelectedCards()
    {
        foreach (GameObject card in selectedCards)
        {
            Destroy(card);
        }
        selectedCards.Clear();
    }
}