using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Start, 
    PlayerTurn, 
    EnemyTurn, 
    Win, 
    Lose, 
    Pause
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public GameState state;
    public bool enemySelect; //적 선택 여부
    public bool playerSelect; //내 캐릭터 선택 여부
    public int draw = 0; //교착 횟수
    public int playerCheck = 0;
    public int enemyCheck = 0;
    public bool allTargetSelected = false; //모든 타겟을 설정했는가
    public bool attacking = false;
    public bool selecting = false;  //적을 선택해야하는 상태일 때

    //다수의 적과 플레이어를 선택할 수 있도록 List 사용
    public List<CharacterProfile> targetObjects = new List<CharacterProfile>();
    public List<CharacterProfile> playerObjects = new List<CharacterProfile>();

    void Awake()
    {
        state = GameState.Start; // 전투 시작 알림
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        playerCheck = players.Length;
        enemyCheck = enemies.Length;

        // 게임 시작시 전투 시작
        BattleStart();
    }

    public void Update()
    {
        if (state == GameState.PlayerTurn && Input.GetMouseButtonDown(0))
        {
            SelectTarget();
            Debug.Log("선택 실행 중");
        }
    }

    void BattleStart()
    {
        // 전투 시작 시 캐릭터 등장 애니메이션 등 효과를 넣고 싶으면 여기 아래에

        // 플레이어나 적에게 턴 넘기기
        // 추후 랜덤 가능성 있음.
        state = GameState.PlayerTurn;
    }

    //공격&턴 종료 버튼
    public void PlayerAttackButton()
    {
        //플레이어 턴이 아닐 때 방지
        if (state != GameState.PlayerTurn || !allTargetSelected)
        {
            return;
        }
        if (attacking)
        {
            return;
        }
        StartCoroutine(PlayerAttack());
    }

    void SelectTarget()  //내 캐릭터&타겟 선택
    {
        //클릭된 오브젝트 가져오기
        GameObject clickObject = UIManager.Instance.MouseGetObject();

        if (clickObject == null)
            return;

        if (clickObject.CompareTag("Enemy"))
        {
            CharacterProfile selectedEnemy = clickObject.GetComponent<CharacterProfile>();

            //동일한 플레이어 클릭 시 선택 취소
            if (targetObjects.Contains(selectedEnemy))
            {
                targetObjects.Remove(selectedEnemy);
                Debug.Log("적 캐릭터 선택 취소됨");
                selecting = true;
            }

            if (selecting)
            {
                //새로운 적 선택
                targetObjects.Add(selectedEnemy);
                Debug.Log("적 캐릭터 선택됨");
                selecting = false;
            }
        }

        // 플레이어 캐릭터 선택 또는 재선택
        if (clickObject.CompareTag("Player"))
        {
            if (selecting)
            {
                Debug.Log("적을 선택해주세요.");
                return;
            }

            CharacterProfile selectedPlayer = clickObject.GetComponent<CharacterProfile>();

            //플레이어가 이미 선택된 플레이어 리스트에 있는지 확인
            if (playerObjects.Contains(selectedPlayer))
            {
                //매칭된 적 삭제
                int index = playerObjects.IndexOf(selectedPlayer);
                if (index != -1 && index < targetObjects.Count)
                {
                    targetObjects.RemoveAt(index);
                }

                //동일한 플레이어 클릭 시 선택 취소
                playerObjects.Remove(selectedPlayer);
                Debug.Log("플레이어 캐릭터 선택 취소됨");
            }
            else
            {
                //새로운 플레이어 선택
                playerObjects.Add(selectedPlayer);
                Debug.Log("플레이어 캐릭터 선택됨");
                selecting = true;
            }
        }

        //아군과 적이 모두 선택되었는지 확인
        playerSelect = playerObjects.Count > 0;
        enemySelect = targetObjects.Count > 0;
        if (playerObjects.Count == playerCheck && targetObjects.Count == enemyCheck)
        {
            allTargetSelected = true;
        }
        else
        {
            allTargetSelected = false;
        }
    }

    void CoinRoll(CharacterProfile Object, ref int succesCount)// 정신력에 비례하여 코인 결과 조정
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            float maxMenTality = 100f; // 최대 정신력
            float maxProbability = 0.6f; // 최대 확률 (60%)

            // 정신력에 따른 확률 계산
            float currentProbability = Mathf.Max(0f, maxProbability * (Object.GetPlayer.menTality / maxMenTality));

            for (int j = 0; j < Object.GetPlayer.coin - 1; j++)
            {
                // 코인 던지기: 현재 확률에 따라 성공 여부 결정
                if (Random.value < currentProbability)
                {
                    Object.successCount++;
                }
            }
            Object.coinBonus = succesCount * Object.GetPlayer.dmgUp;
            Debug.Log($"{Object.GetPlayer.charName}의 코인 던지기 성공 횟수: {succesCount} / {Object.GetPlayer.coin} ");
            Debug.Log($"{Object.GetPlayer.charName}의 남은 코인: {Object.GetPlayer.coin} / {Object.GetPlayer.maxCoin}");
        }
    }

    void DiffCheck()// 공격 레벨과 방어 레벨을 비교하여 보너스 및 패널티 적용
    {
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterProfile playerObject = playerObjects[i];
            CharacterProfile targetObject = targetObjects[i];


            if (playerObject.GetPlayer.dmgLevel > (targetObject.GetPlayer.DefLevel + 4))
            {
                playerObject.bonusDmg = ((playerObject.GetPlayer.dmgLevel - playerObject.GetPlayer.DefLevel) / 4) * 1;
            }
            if (targetObject.GetPlayer.dmgLevel > (playerObject.GetPlayer.DefLevel + 4))
            {
                targetObject.bonusDmg = ((targetObject.GetPlayer.dmgLevel - playerObject.GetPlayer.DefLevel) / 4) * 1;
            }
        }
    }

    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        attacking = true;
        yield return new WaitForSeconds(1f);

        Debug.Log("플레이어 공격");
        DiffCheck();
        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterProfile playerObject = playerObjects[i];
            CharacterProfile targetObject = targetObjects[i];

            playerObject.totalDamageDealt = 0; // 턴 시작 시 총 데미지 초기화

            while (playerObject.GetPlayer.coin > 0)
            {
                CalculateDamage(playerObject, targetObject);
                ApplyDamage(playerObject, targetObject);
                playerObject.GetPlayer.coin--;
            }

            // 플레이어의 총 데미지 표시 (적의 머리 위에)
            DisplayTotalDamage(playerObject, targetObject);
        }

        attacking = false;
        yield return StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()     //적 공격턴
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("적 공격");

        int matchCount = Mathf.Min(playerObjects.Count, targetObjects.Count);
        for (int i = 0; i < matchCount; i++)
        {
            CharacterProfile enemyObject = targetObjects[i];
            CharacterProfile playerObject = playerObjects[i];

            enemyObject.totalDamageDealt = 0; // 턴 시작 시 총 데미지 초기화

            while (enemyObject.GetPlayer.coin > 0)
            {
                CalculateDamage(enemyObject, playerObject);
                ApplyDamage(enemyObject, playerObject);
                enemyObject.GetPlayer.coin--;
            }

            // 적의 총 데미지 표시 (플레이어의 머리 위에)
            DisplayTotalDamage(enemyObject, playerObject);
        }

        yield return new WaitForSeconds(2f); // 데미지 표시를 위한 대기 시간

        state = GameState.PlayerTurn;
        allTargetSelected = false;
    }

    // 데미지 계산 함수 수정
    void CalculateDamage(CharacterProfile attacker, CharacterProfile defender)
    {
        int damage = Random.Range(attacker.GetPlayer.maxDmg, attacker.GetPlayer.minDmg) + attacker.coinBonus + attacker.bonusDmg;
        attacker.GetPlayer.dmg = damage;
        attacker.totalDamageDealt += damage; // 총 데미지에 추가
        Debug.Log($"{attacker.GetPlayer.charName}의 공격 데미지: {damage} (총 데미지: {attacker.totalDamageDealt})");
    }

    // 데미지 적용 함수
    void ApplyDamage(CharacterProfile attacker, CharacterProfile victim)
    {
        int actualDamage = Mathf.Max(0, attacker.GetPlayer.dmg - victim.GetPlayer.DefLevel);
        victim.GetPlayer.hp -= actualDamage;

        if (victim.GetPlayer.hp <= 0)
        {
            victim.OnDeath();
        }

        victim.GetPlayer.menTality -= 2;            //정신력 감소
        if (attacker.GetPlayer.menTality < 100)
        {
            attacker.GetPlayer.menTality += 1;      //정신력 증가
        }
    }

    // 총 데미지 표시 함수 수정 (Vector3 사용)
    void DisplayTotalDamage(CharacterProfile attacker, CharacterProfile defender)
    {
        // 피해를 받는 대상의 위치를 Vector3로 사용하고 약간 위로 올림
        Vector3 damagePosition = defender.transform.position + Vector3.up * 250f;

        UIManager.Instance.ShowDamageText(attacker.totalDamageDealt, damagePosition);
        Debug.Log($"{attacker.GetPlayer.charName}가 {defender.GetPlayer.charName}에게 입힌 총 데미지: {attacker.totalDamageDealt}");
    }

    void CheckBattleEnd()
    {
        if (enemyCheck == 0)
        {
            state = GameState.Win;
            Debug.Log("승리");
            EndBattle();
        }
        else if (playerCheck == 0)
        {
            state = GameState.Lose;
            Debug.Log("패배");
            EndBattle();
        }
    }

    void EndBattle()    //전투 종료
    {
        Debug.Log("전투 종료");
    }
}

