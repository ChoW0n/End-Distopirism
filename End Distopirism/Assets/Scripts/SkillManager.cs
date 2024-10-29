using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 네임스페이스 추가

public class SkillManager : MonoBehaviour
{
    // 스킬 카드의 기본 스프라이트와 성공 시 사용할 스프라이트를 관리
    public Sprite defaultSprite;
    public Sprite successSprite;

    // Card1, Card2, Card3의 Image 컴포넌트를 참조하기 위한 변수 추가
    [SerializeField]
    private Image card1Image;
    [SerializeField]
    private Image card2Image;
    [SerializeField]
    private Image card3Image;

    // Start is called before the first frame update
    void Start()
    {
        // 시작 시 카드들을 비활성화
        DeactivateCards();
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
        float delay = 0.1f; // 카드 간 딜레이

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

            // 카드 회전 및 축소 애니메이션
            cards[i].transform
                .DOScale(Vector3.zero, 0.3f)
                .SetDelay(delay * i)
                .SetEase(Ease.InBack);

            cards[i].transform
                .DORotate(new Vector3(0, 180f, 0), 0.3f)
                .SetDelay(delay * i)
                .SetEase(Ease.InQuad);

            // 페이드 아웃 효과
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, 0.3f)
                .SetDelay(delay * i)
                .SetEase(Ease.InQuad)
                .OnStart(() => Debug.Log($"카드{index + 1} 페이드 아웃 시작"))
                .OnComplete(() => 
                {
                    Debug.Log($"카드{index + 1} 페이드 아웃 완료");
                    cards[index].gameObject.SetActive(false);
                });
        }
    }

    /// <summary>
    /// 플레이어의 스킬 목록에서 랜덤하게 3개를 선택하여 카드에 스프라이트를 할당합니다.
    /// </summary>
    /// <param name="playerSkills">선택된 플레이어의 스킬 리스트</param>
    public void AssignRandomSkillSprites(List<Skill> playerSkills)
    {
        if (playerSkills == null || playerSkills.Count == 0)
        {
            Debug.LogWarning("플레이어의 스킬 목록이 비어 있습니다.");
            return;
        }

        // 랜덤으로 3개의 스킬 선택 (중복 허용)
        Skill[] selectedSkills = new Skill[3];
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, playerSkills.Count);
            selectedSkills[i] = playerSkills[randomIndex];
            Debug.Log($"선택된 스킬 {i + 1}: {selectedSkills[i].skillName}");
        }

        // 카드 초기 상태 설정 및 스프라이트 할당
        Image[] cards = { card1Image, card2Image, card3Image };
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null)
            {
                Debug.LogWarning($"카드{i + 1}가 null입니다.");
                continue; // 카드가 null인지 확인
            }

            cards[i].gameObject.SetActive(true);
            cards[i].sprite = selectedSkills[i].nomalSprite;
            Debug.Log($"카드{i + 1} 스프라이트 할당: {selectedSkills[i].nomalSprite.name}");
            cards[i].transform.localScale = Vector3.zero;
            cards[i].transform.rotation = Quaternion.Euler(0, 180f, 0); // 뒤집힌 상태로 시작

            // CanvasGroup 초기화
            CanvasGroup canvasGroup = cards[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = cards[i].gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;

            // 애니메이션 적용
            float delay = 0.2f; // 카드 간 딜레이
            int index = i; // 클로저를 위한 인덱스 복사

            cards[i].transform
                .DOScale(1f, 0.5f) // 크기 애니메이션
                .SetDelay(delay * index) // 카드마다 딜레이
                .SetEase(Ease.OutBack) // 튕기는 효과
                .OnComplete(() => Debug.Log($"카드{index + 1} 애니메이션 완료"));

            cards[i].transform
                .DORotate(Vector3.zero, 0.5f) // 회전 애니메이션
                .SetDelay(delay * index)
                .SetEase(Ease.OutQuad);

            // 페이드 인 효과
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, 0.3f) // 0.3초 동안 페이드 인
                .SetDelay(delay * index)
                .OnStart(() => Debug.Log($"카드{index + 1} 페이드 인 시작"))
                .OnComplete(() => Debug.Log($"카드{index + 1} 페이드 인 완료"));
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

    private void OnDestroy()
    {
        // 씬 전환 시 DOTween 애니메이션 정리
        if (card1Image != null && card1Image.transform != null)
            DOTween.Kill(card1Image.transform);
        if (card2Image != null && card2Image.transform != null)
            DOTween.Kill(card2Image.transform);
        if (card3Image != null && card3Image.transform != null)
            DOTween.Kill(card3Image.transform);
    }
}
