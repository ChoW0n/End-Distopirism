using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  // TextMeshPro 추가

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;  // TextMeshPro - Text를 포함한 프리팹으로 교체 필요
    public Canvas canvas;
    private static UIManager uimInstance;

    public GameObject playerProfilePanel;
    public GameObject enemyProfilePanel;

    public GameObject healthBarPrefab;
    public GameObject mentalityBarPrefab;

    public GameObject playerSkillEffectPrefab;
    public GameObject enemySkillEffectPrefab;

    public static UIManager Instance
    {
        get
        {
            if (uimInstance == null)
            {
                uimInstance = FindObjectOfType<UIManager>();
                if (uimInstance == null)
                {
                    Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다.");
                }
                else
                {
                    DontDestroyOnLoad(uimInstance.gameObject);
                }
            }
            return uimInstance;
        }
    }

    public GameObject MouseGetObject()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.zero);
        GameObject clickObject = null;

        if (hit.collider != null)
        {
            clickObject = hit.transform.gameObject;
            Debug.Log("클릭한 오브젝트: " + clickObject.name);
            return clickObject;
        }
        return null;
    }

    public void ShowCharacterInfo(CharacterProfile character)
    {
        if (BattleManager.Instance.state == GameState.playerTurn || BattleManager.Instance.state == GameState.enemyTurn)
        {
            if (character.CompareTag("Player"))
            {
                playerProfilePanel.SetActive(true);
                UpdatePlayerInfoPanel(character);
                HideSkillCards(playerProfilePanel);
            }
            else if (character.CompareTag("Enemy"))
            {
                enemyProfilePanel.SetActive(true);
                UpdateEnemyInfoPanel(character);
                HideSkillCards(enemyProfilePanel);
            }
        }
        else
        {
            if (character.CompareTag("Player"))
            {
                playerProfilePanel.SetActive(true);
                UpdatePlayerInfoPanel(character);
                ShowSkillCards(playerProfilePanel);
            }
            else if (character.CompareTag("Enemy"))
            {
                enemyProfilePanel.SetActive(true);
                UpdateEnemyInfoPanel(character);
                ShowSkillCards(enemyProfilePanel);
            }
        }
    }

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

    private void UpdatePlayerInfoPanel(CharacterProfile player)
    {
        TextMeshProUGUI playerDmgLevelText = playerProfilePanel.transform.Find("DmgLevelText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerDefLevelText = playerProfilePanel.transform.Find("DefLevelText").GetComponent<TextMeshProUGUI>();
        
        TextMeshProUGUI playerMaxDmgText = playerProfilePanel.transform.Find("MaxDmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerMinDmgText = playerProfilePanel.transform.Find("MinDmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerDmgUpText = playerProfilePanel.transform.Find("DmgUpText").GetComponent<TextMeshProUGUI>();

        Image playerSkillIcon = playerProfilePanel.transform.Find("SkillIcon").GetComponent<Image>();
        TextMeshProUGUI playerSkillName = playerProfilePanel.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerCoinText = playerProfilePanel.transform.Find("CoinText").GetComponent<TextMeshProUGUI>();

        playerDmgLevelText.text = player.GetPlayer.dmgLevel.ToString();
        playerDefLevelText.text = player.GetPlayer.defLevel.ToString();
        playerMaxDmgText.text = player.GetPlayer.maxDmg.ToString();
        playerMinDmgText.text = player.GetPlayer.minDmg.ToString();
        playerDmgUpText.text = "+" + player.GetPlayer.dmgUp.ToString();

        playerSkillIcon.sprite = player.GetPlayer.skills[0].sprite;
        playerSkillName.text = player.GetPlayer.skills[0].skillName;
        playerCoinText.text = player.GetPlayer.coin.ToString();
    }

    private void UpdateEnemyInfoPanel(CharacterProfile enemy)
    {
        TextMeshProUGUI enemyDmgLevelText = enemyProfilePanel.transform.Find("DmgLevelText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI enemyDefLevelText = enemyProfilePanel.transform.Find("DefLevelText").GetComponent<TextMeshProUGUI>();

        TextMeshProUGUI enemyMaxDmgText = enemyProfilePanel.transform.Find("MaxDmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI enemyMinDmgText = enemyProfilePanel.transform.Find("MinDmgText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI enemyDmgUpText = enemyProfilePanel.transform.Find("DmgUpText").GetComponent<TextMeshProUGUI>();

        Image enemySkillIcon = enemyProfilePanel.transform.Find("SkillIcon").GetComponent<Image>();
        TextMeshProUGUI enemySkillName = enemyProfilePanel.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI enemyCoinText = enemyProfilePanel.transform.Find("CoinText").GetComponent<TextMeshProUGUI>();

        enemyDmgLevelText.text = enemy.GetPlayer.dmgLevel.ToString();
        enemyDefLevelText.text = enemy.GetPlayer.defLevel.ToString();
        enemyMaxDmgText.text = enemy.GetPlayer.maxDmg.ToString();
        enemyMinDmgText.text = enemy.GetPlayer.minDmg.ToString();
        enemyDmgUpText.text = "+" + enemy.GetPlayer.dmgUp.ToString();

        enemySkillIcon.sprite = enemy.GetPlayer.skills[0].sprite;
        enemySkillName.text = enemy.GetPlayer.skills[0].skillName;
        enemyCoinText.text = enemy.GetPlayer.coin.ToString();
    }

    public void ShowBattleResultText(string message, Vector3 position)
    {
        GameObject damageText = Instantiate(damageTextPrefab, position, Quaternion.identity, canvas.transform);
        TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
        textComponent.text = message;

        if (message == "승리")
        {
            textComponent.color = Color.green;
        }
        else if (message == "패배")
        {
            textComponent.color = Color.red;
        }

        StartCoroutine(AnimateDamageText(damageText));
    }

    public void ShowDamageTextNearCharacter(int damage, Transform characterTransform)
    {
        Vector3 randomOffset = Random.insideUnitCircle * 50f;
        Vector3 spawnPosition = characterTransform.position + new Vector3(randomOffset.x, 100f + randomOffset.y, 0);

        GameObject damageText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, canvas.transform);
        TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
        textComponent.text = "-" + damage.ToString();
        textComponent.color = Color.red;
        
        StartCoroutine(AnimateDamageText(damageText));
    }

    private IEnumerator AnimateDamageText(GameObject damageText)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Vector3 startPosition = damageText.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 50f;
        TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();

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

    public void SetBattleUI(bool isBattle)
    {
        if (isBattle)
        {
            HideSkillCards(playerProfilePanel);
            HideSkillCards(enemyProfilePanel);
        }
        else
        {
            ShowSkillCards(playerProfilePanel);
            ShowSkillCards(enemyProfilePanel);
        }
    }
}