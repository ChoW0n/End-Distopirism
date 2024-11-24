using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    public void OnGameStartButtonClick()
    {
        SceneManager.LoadScene("StageSelect");
    }

    public void OnStage1ButtonClick()
    {
        DeckData.currentStage = 1;
        DeckData.selectedCharacterPrefabs.Clear(); // 이전 선택 초기화
        SceneManager.LoadScene("DeckBuilding");
    }

    public void OnDeckBuildingComplete()
    {
        // Resources 폴더에서 현재 스테이지의 StageData를 로드
        StageData stageData = Resources.Load<StageData>($"StageData/Stage{DeckData.currentStage}");
        if (stageData != null)
        {
            // StageData에 설정된 씬 이름으로 이동
            SceneManager.LoadScene(stageData.sceneName);
        }
        else
        {
            // StageData를 찾지 못한 경우 기본 이름 사용
            SceneManager.LoadScene($"Stage{DeckData.currentStage}");
            Debug.LogWarning($"Stage{DeckData.currentStage}의 StageData를 찾을 수 없습니다. 기본 씬 이름을 사용합니다.");
        }
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
