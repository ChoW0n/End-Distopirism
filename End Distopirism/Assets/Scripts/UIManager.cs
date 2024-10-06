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

    //캐릭터 정보 패널 업데이트 함수
    private void UpdatePlayerInfoPanel(CharacterProfile player)
    {
        //기본 캐릭터 정보
        Text playerNameText = currentPlayerInfoPanel.transform.Find("NameText").GetComponent<Text>();
        Text playerDmgLevelText = currentPlayerInfoPanel.transform.Find("DmgLevelText").GetComponent<Text>();
        Text playerDefLevelText = currentPlayerInfoPanel.transform.Find("DefLevelText").GetComponent<Text>();
        
        //스킬 정보
        Text playerMaxDmgText = currentPlayerInfoPanel.transform.Find("MaxDmgText").GetComponent<Text>();
        Text playerMinDmgText = currentPlayerInfoPanel.transform.Find("MinDmgText").GetComponent<Text>();
        Text playerDmgUpText = currentPlayerInfoPanel.transform.Find("DmgUpText").GetComponent<Text>();

        Image playerSkillIcon = currentPlayerInfoPanel.transform.Find("SkillIcon").GetComponent<Image>();
        Text playerSkillName = currentPlayerInfoPanel.transform.Find("SkillName").GetComponent<Text>();


        playerNameText.text = player.GetPlayer.charName; 
        playerDmgLevelText.text = "" + player.GetPlayer.dmgLevel;  
        playerDefLevelText.text = "" + player.GetPlayer.defLevel;  

        playerMaxDmgText.text = "" + player.GetPlayer.maxDmg;
        playerMinDmgText.text = "" + player.GetPlayer.minDmg;
        playerDmgUpText.text = "+" + player.GetPlayer.dmgUp;

        playerSkillIcon.sprite = player.GetPlayer.skills[0].sprite;
        playerSkillName.text = player.GetPlayer.skills[0].skillName;
    }

    private void UpdateEnemyInfoPanel(CharacterProfile enemy)
    {
        Text enemyNameText = currentEnemyInfoPanel.transform.Find("NameText").GetComponent<Text>();
        Text enemyDmgLevelText = currentEnemyInfoPanel.transform.Find("DmgLevelText").GetComponent<Text>();
        Text enemyDefLevelText = currentEnemyInfoPanel.transform.Find("DefLevelText").GetComponent<Text>();

        Text enemyMaxDmgText = currentEnemyInfoPanel.transform.Find("MaxDmgText").GetComponent<Text>();
        Text enemyMinDmgText = currentEnemyInfoPanel.transform.Find("MinDmgText").GetComponent<Text>();
        Text enemyDmgUpText = currentEnemyInfoPanel.transform.Find("DmgUpText").GetComponent<Text>();

        Image enemySkillIcon = currentEnemyInfoPanel.transform.Find("SkillIcon").GetComponent<Image>();
        Text enemySkillName = currentEnemyInfoPanel.transform.Find("SkillName").GetComponent<Text>();
        enemyNameText.text = enemy.GetPlayer.charName; 
        enemyDmgLevelText.text = "" + enemy.GetPlayer.dmgLevel;  
        enemyDefLevelText.text = "" + enemy.GetPlayer.defLevel; 

        enemyMaxDmgText.text = "" + enemy.GetPlayer.maxDmg;
        enemyMinDmgText.text = "" + enemy.GetPlayer.minDmg;
        enemyDmgUpText.text = "+" + enemy.GetPlayer.dmgUp;

        enemySkillIcon.sprite = enemy.GetPlayer.skills[0].sprite;
        enemySkillName.text = enemy.GetPlayer.skills[0].skillName;
    }

}
