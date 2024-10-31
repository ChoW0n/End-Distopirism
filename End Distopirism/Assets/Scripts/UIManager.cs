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

    public GameObject playerProfilePanel;
    public GameObject enemyProfilePanel;

    public GameObject healthBarPrefab;  // 체력바 프리팹
    public GameObject mentalityBarPrefab;  // 정신력바 프리팹


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
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 디버그: 마우스 위치 출력
        //Debug.Log("마우스 월드 좌표: " + pos);

        // 레이캐스트로 오브젝트를 탐지
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.zero);
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

    // 캐릭터 정보 표시 함수
    public void ShowCharacterInfo(CharacterProfile character)
    {
        if (BattleManager.Instance.state == GameState.playerTurn || BattleManager.Instance.state == GameState.enemyTurn)
        {
            // 전투 중에는 캐릭터 정보를 표시하되, 스킬 카드는 숨김
            if (character.CompareTag("Player"))
            {
                playerProfilePanel.SetActive(true);
                UpdatePlayerInfoPanel(character);
                // 스킬 카드 숨기기
                HideSkillCards(playerProfilePanel);
            }
            else if (character.CompareTag("Enemy"))
            {
                enemyProfilePanel.SetActive(true);
                UpdateEnemyInfoPanel(character);
                // 스킬 카드 숨기기
                HideSkillCards(enemyProfilePanel);
            }
        }
        else
        {
            // 전투가 아닐 때 기존 동작 유지
            if (character.CompareTag("Player"))
            {
                playerProfilePanel.SetActive(true);
                UpdatePlayerInfoPanel(character);
                ShowSkillCards(playerProfilePanel); // 스킬 카드 표시
            }
            else if (character.CompareTag("Enemy"))
            {
                enemyProfilePanel.SetActive(true);
                UpdateEnemyInfoPanel(character);
                ShowSkillCards(enemyProfilePanel); // 스킬 카드 표시
            }
        }
    }

    // 스킬 카드를 숨기는 메서드 추가
    public void HideSkillCards(GameObject profilePanel)
    {
        Transform skillCards = profilePanel.transform.Find("SkillCards");
        if (skillCards != null)
        {
            foreach (Transform card in skillCards)
            {
                card.gameObject.SetActive(false);
            }
        }
    }

    // 스킬 카드를 표시하는 메서드 추가
    public void ShowSkillCards(GameObject profilePanel)
    {
        Transform skillCards = profilePanel.transform.Find("SkillCards");
        if (skillCards != null)
        {
            foreach (Transform card in skillCards)
            {
                card.gameObject.SetActive(true);
            }
        }
    }

    //캐릭터 정보 패널 업데이트 함수
    private void UpdatePlayerInfoPanel(CharacterProfile player)
    {
        //기본 캐릭터 정보
        Text playerDmgLevelText = playerProfilePanel.transform.Find("DmgLevelText").GetComponent<Text>();
        Text playerDefLevelText = playerProfilePanel.transform.Find("DefLevelText").GetComponent<Text>();
        
        //스킬 정보
        Text playerMaxDmgText = playerProfilePanel.transform.Find("MaxDmgText").GetComponent<Text>();
        Text playerMinDmgText = playerProfilePanel.transform.Find("MinDmgText").GetComponent<Text>();
        Text playerDmgUpText = playerProfilePanel.transform.Find("DmgUpText").GetComponent<Text>();

        Image playerSkillIcon = playerProfilePanel.transform.Find("SkillIcon").GetComponent<Image>();
        Text playerSkillName = playerProfilePanel.transform.Find("SkillName").GetComponent<Text>();

        Text playerCoinText = playerProfilePanel.transform.Find("CoinText").GetComponent<Text>();


        playerDmgLevelText.text = "" + player.GetPlayer.dmgLevel;  
        playerDefLevelText.text = "" + player.GetPlayer.defLevel;  

        playerMaxDmgText.text = "" + player.GetPlayer.maxDmg;
        playerMinDmgText.text = "" + player.GetPlayer.minDmg;
        playerDmgUpText.text = "+" + player.GetPlayer.dmgUp;

        playerSkillIcon.sprite = player.GetPlayer.skills[0].sprite;
        playerSkillName.text = player.GetPlayer.skills[0].skillName;

        playerCoinText.text = "" + player.GetPlayer.coin; // 코인 정보 표시
    }

    private void UpdateEnemyInfoPanel(CharacterProfile enemy)
    {
        Text enemyDmgLevelText = enemyProfilePanel.transform.Find("DmgLevelText").GetComponent<Text>();
        Text enemyDefLevelText = enemyProfilePanel.transform.Find("DefLevelText").GetComponent<Text>();

        Text enemyMaxDmgText = enemyProfilePanel.transform.Find("MaxDmgText").GetComponent<Text>();
        Text enemyMinDmgText = enemyProfilePanel.transform.Find("MinDmgText").GetComponent<Text>();
        Text enemyDmgUpText = enemyProfilePanel.transform.Find("DmgUpText").GetComponent<Text>();

        Image enemySkillIcon = enemyProfilePanel.transform.Find("SkillIcon").GetComponent<Image>();
        Text enemySkillName = enemyProfilePanel.transform.Find("SkillName").GetComponent<Text>();

        Text enemyCoinText = enemyProfilePanel.transform.Find("CoinText").GetComponent<Text>();

        enemyDmgLevelText.text = "" + enemy.GetPlayer.dmgLevel;  
        enemyDefLevelText.text = "" + enemy.GetPlayer.defLevel; 

        enemyMaxDmgText.text = "" + enemy.GetPlayer.maxDmg;
        enemyMinDmgText.text = "" + enemy.GetPlayer.minDmg;
        enemyDmgUpText.text = "+" + enemy.GetPlayer.dmgUp;

        enemySkillIcon.sprite = enemy.GetPlayer.skills[0].sprite;
        enemySkillName.text = enemy.GetPlayer.skills[0].skillName;

        enemyCoinText.text = "" + enemy.GetPlayer.coin;
    }

    public void ShowBattleResultText(string message, Vector3 position)
    {
        GameObject damageText = Instantiate(damageTextPrefab, position, Quaternion.identity, canvas.transform);
        Text textComponent = damageText.GetComponent<Text>();
        textComponent.text = message;

        // 승리 문구는 초록색, 패배 문구는 빨간색으로 설정
        if (message == "승리")
        {
            textComponent.color = Color.green; // 승리 문구는 초록색
        }
        else if (message == "패배")
        {
            textComponent.color = Color.red; // 패배 문구는 빨간색
        }

        // 텍스트 애니메이션
        StartCoroutine(AnimateDamageText(damageText));
    }

    public void ShowDamageTextNearCharacter(int damage, Transform characterTransform)
    {
        Vector3 randomOffset = UnityEngine.Random.insideUnitCircle * 50f; // 랜덤한 오프셋 생성
        Vector3 spawnPosition = characterTransform.position + new Vector3(randomOffset.x, 100f + randomOffset.y, 0);

        GameObject damageText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, canvas.transform);
        Text textComponent = damageText.GetComponent<Text>();
        textComponent.text = "-" + damage.ToString();
        textComponent.color = Color.red; // 데미지 텍스트 색상을 빨간색으로 설정
        
        // 텍스트 애니메이션
        StartCoroutine(AnimateDamageText(damageText));
    }

    private IEnumerator AnimateDamageText(GameObject damageText)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Vector3 startPosition = damageText.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 50f;
        Text textComponent = damageText.GetComponent<Text>();

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            damageText.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1 - t);
            yield return null;
        }

        Destroy(damageText);
    }

    // 전투 시작과 종료에 따라 UI를 업데이트하는 메서드 추가
    public void SetBattleUI(bool isBattle)
    {
        if (isBattle)
        {
            // 전투 시작 시 필요한 UI 요소를 숨기거나 비활성화
            HideSkillCards(playerProfilePanel);
            HideSkillCards(enemyProfilePanel);
        }
        else
        {
            // 전투 종료 시 필요한 UI 요소를 다시 표시
            ShowSkillCards(playerProfilePanel);
            ShowSkillCards(enemyProfilePanel);
        }
    }

}
