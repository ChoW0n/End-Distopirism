using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FakeLoading : MonoBehaviour
{
    [SerializeField] private Image fillImage;        // Loading->Fill 이미지
    [SerializeField] private TextMeshProUGUI loadingText;  // Loading->Text
    [SerializeField] private CanvasGroup canvasGroup;  // 로딩 UI의 CanvasGroup
    [SerializeField] private float fadeTime = 0.5f;    // 페이드 시간
    
    private void Start()
    {
        string nextSceneName = SceneButtonManager.GetNextSceneName();
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("다음 씬 이름이 설정되지 않았습니다.");
            return;
        }

        // 시작 시 UI를 투명하게 설정
        canvasGroup.alpha = 0f;
        
        StartCoroutine(LoadingSequence(nextSceneName));
    }

    private IEnumerator LoadingSequence(string nextSceneName)
    {
        // 페이드 인
        yield return StartCoroutine(FadeLoadingUI(true));
        
        // 로딩 진행
        yield return StartCoroutine(LoadingCoroutine(nextSceneName));
        
        // 페이드 아웃
        yield return StartCoroutine(FadeLoadingUI(false));
        
        // 씬 전환
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator LoadingCoroutine(string nextSceneName)
    {
        float randomTime = Random.Range(5f, 7f);
        float currentTime = 0f;
        int lastProgress = 0;
        
        while (currentTime < randomTime)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / randomTime;
            
            fillImage.fillAmount = progress;
            
            int currentProgress = Mathf.FloorToInt(progress * 100f);
            
            if (currentProgress != lastProgress)
            {
                loadingText.text = $"{currentProgress}%";
                lastProgress = currentProgress;
            }
            
            yield return null;
        }
        
        loadingText.text = "100%";
    }

    private IEnumerator FadeLoadingUI(bool fadeIn)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
