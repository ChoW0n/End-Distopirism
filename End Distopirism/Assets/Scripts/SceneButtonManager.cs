using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    private static string nextSceneName; // 다음에 로드할 씬 이름을 저장

    public void OnGameStartButtonClick()
    {
        nextSceneName = "StageSelect";
        SceneManager.LoadScene("Loading");
    }

    public void OnStage1ButtonClick()
    {
        DeckData.currentStage = 1;
        DeckData.selectedCharacterPrefabs.Clear(); // 이전 선택 초기화
        nextSceneName = "DeckBuilding";
        SceneManager.LoadScene("Loading");
    }

    public void OnDeckBuildingComplete()
    {
        // Resources 폴더에서 현재 스테이지의 StageData를 로드
        StageData stageData = Resources.Load<StageData>($"StageData/Stage{DeckData.currentStage}");
        if (stageData != null)
        {
            // StageData에 설정된 씬 이름으로 이동
            nextSceneName = stageData.sceneName;
        }
        else
        {
            // StageData를 찾지 못한 경우 기본 이름 사용
            nextSceneName = $"Stage{DeckData.currentStage}";
            Debug.LogWarning($"Stage{DeckData.currentStage}의 StageData를 찾을 수 없습니다. 기본 씬 이름을 사용합니다.");
        }
        SceneManager.LoadScene("Loading");
    }

    // 다음 씬 이름을 가져오는 정적 메서드
    public static string GetNextSceneName()
    {
        return nextSceneName;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
