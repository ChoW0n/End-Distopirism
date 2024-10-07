using System.Collections.Generic;
using UnityEngine;

public class TargetSelector
{
    internal bool enemySelect; // 적 선택 여부
    internal bool playerSelect; // 내 캐릭터 선택 여부
    internal bool allTargetSelected = false; // 모든 타겟을 설정했는가
    internal bool selecting = false; // 적을 선택해야하는 상태일 때

    internal List<CharacterProfile> targetObjects = new List<CharacterProfile>();
    internal List<CharacterProfile> playerObjects = new List<CharacterProfile>();

    private int playerCheck = 0;
    private int enemyCheck = 0;

    internal void InitializeSelector(int playerCount, int enemyCount)
    {
        playerCheck = playerCount;
        enemyCheck = enemyCount;
    }

    internal void SelectTarget()
    {
        GameObject clickObject = UIManager.Instance.MouseGetObject();

        if (clickObject == null)
            return;

        if (clickObject.CompareTag("Enemy"))
        {
            HandleEnemySelection(clickObject);
        }
        else if (clickObject.CompareTag("Player"))
        {
            HandlePlayerSelection(clickObject);
        }

        UpdateSelectionStatus();
    }

    private void HandleEnemySelection(GameObject clickObject)
    {
        CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();

        if (targetObjects.Contains(selectedEnemy))
        {
            targetObjects.Remove(selectedEnemy);
            Debug.Log("적 캐릭터 선택 취소됨");
            selecting = true;
        }

        if (selecting)
        {
            targetObjects.Add(selectedEnemy);
            Debug.Log("적 캐릭터 선택됨");
            selecting = false;

            UIManager.Instance.ShowCharacterInfo(targetObjects[0]);
        }
    }

    private void HandlePlayerSelection(GameObject clickObject)
    {
        if (selecting)
        {
            Debug.Log("적을 선택해주세요.");
            return;
        }

        CharacterProfile selectedPlayer = clickObject.GetComponent<CharacterProfile>();

        if (playerObjects.Contains(selectedPlayer))
        {
            int index = playerObjects.IndexOf(selectedPlayer);
            if (index != -1 && index < targetObjects.Count)
            {
                targetObjects.RemoveAt(index);
            }

            playerObjects.Remove(selectedPlayer);
            Debug.Log("플레이어 캐릭터 선택 취소됨");
        }
        else
        {
            playerObjects.Add(selectedPlayer);
            Debug.Log("플레이어 캐릭터 선택됨");
            selecting = true;

            UIManager.Instance.ShowCharacterInfo(playerObjects[0]);
        }
    }

    private void UpdateSelectionStatus()
    {
        playerSelect = playerObjects.Count > 0;
        enemySelect = targetObjects.Count > 0;
        allTargetSelected = (playerObjects.Count == playerCheck && targetObjects.Count == enemyCheck);
    }

    internal void ResetSelection()
    {
        targetObjects.Clear();
        playerObjects.Clear();
        allTargetSelected = false;
        selecting = false;
    }
}