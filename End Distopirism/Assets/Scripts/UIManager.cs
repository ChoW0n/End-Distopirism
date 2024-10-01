using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public Canvas canvas;
    private static UIManager uimInstance;

    public GameObject playerProfilePrefab;
    public GameObject enemyProfilePrefab;
    private GameObject currentPlayerInfoPanel;
    private GameObject currentEnemyInfoPanel;

    // Singleton 인스턴스
    public static UIManager Instance
    {
        get
        {
            if (uimInstance == null)
            {
                uimInstance = FindObjectOfType<UIManager>();

                // 만약 UIManager 인스턴스가 존재하지 않으면 오류 출력
                if (uimInstance == null)
                {
                    Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다.");
                }
                else
                {
                    // 싱글톤 인스턴스를 파괴되지 않도록 설정
                    DontDestroyOnLoad(uimInstance.gameObject);
                }
            }
            return uimInstance;
        }
    }

    // 마우스 클릭 위치에서 오브젝트 반환
    public GameObject MouseGetObject()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 디버그: 마우스 위치 출력
        //Debug.Log("마우스 월드 좌표: " + pos);

        // 레이캐스트로 오브젝트를 탐지
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        GameObject clickObject = null;

        if (hit.collider != null)
        {
            clickObject = hit.transform.gameObject;

            // 디버그: 선택된 오브젝트 이름 출력
            Debug.Log("클릭한 오브젝트: " + clickObject.name);

            return clickObject;
        }

        // 디버그: 맞춘 오브젝트가 없을 때
        //Debug.LogWarning("레이캐스트가 오브젝트를 맞추지 못했습니다.");
        return null;
    }

    //데미지 표시 함수
    public void ShowDamageText(int damageAmount, Vector3 worldPosition)
    {
        GameObject damageText = Instantiate(damageTextPrefab, worldPosition, Quaternion.identity, canvas.transform);

        Text textComponent = damageText.GetComponentInChildren<Text>();
        textComponent.text = "-" + damageAmount.ToString();

        // 월드 공간에서의 위치 설정
        RectTransform rectTransform = damageText.GetComponent<RectTransform>();
        rectTransform.position = worldPosition;

        // 텍스트가 항상 카메라를 향하도록 설정
        damageText.transform.forward = Camera.main.transform.forward;

        Destroy(damageText, 2f);
    }

    // 캐릭터 정보 표시 함수
    public void ShowCharacterInfo(CharacterProfile character)
    {
        if (character.CompareTag("Player"))
        {
            if (currentPlayerInfoPanel == null)
            {
                currentPlayerInfoPanel = Instantiate(playerProfilePrefab, canvas.transform);
            }

            UpdatePlayerInfoPanel(character);
        }
        else if (character.CompareTag("Enemy"))
        {
            if (currentEnemyInfoPanel == null)
            {
                currentEnemyInfoPanel = Instantiate(enemyProfilePrefab, canvas.transform);
            }
            UpdateEnemyInfoPanel(character);
        }
    }

    // 캐릭터 정보 패널 업데이트 함수
    private void UpdatePlayerInfoPanel(CharacterProfile player)
    {
        Text playerNameText = currentPlayerInfoPanel.transform.Find("PlayerNameText").GetComponent<Text>();
        Text playerDmgLevelText = currentPlayerInfoPanel.transform.Find("PlayerDmgLevelText").GetComponent<Text>();
        Text playerDefLevelText = currentPlayerInfoPanel.transform.Find("PlayerDefLevelText").GetComponent<Text>();

        playerNameText.text = player.GetPlayer.charName; 
        playerDmgLevelText.text = "" + player.GetPlayer.dmgLevel;  
        playerDefLevelText.text = "" + player.GetPlayer.defLevel;  
    }

    private void UpdateEnemyInfoPanel(CharacterProfile enemy)
    {
        Text enemyNameText = currentEnemyInfoPanel.transform.Find("EnemyNameText").GetComponent<Text>();
        Text enemyDmgLevelText = currentEnemyInfoPanel.transform.Find("EnemyDmgLevelText").GetComponent<Text>();
        Text enemyDefLevelText = currentEnemyInfoPanel.transform.Find("EnemyDefLevelText").GetComponent<Text>();

        enemyNameText.text = enemy.GetPlayer.charName; 
        enemyDmgLevelText.text = "" + enemy.GetPlayer.dmgLevel;  
        enemyDefLevelText.text = "" + enemy.GetPlayer.defLevel; 
    }

}
