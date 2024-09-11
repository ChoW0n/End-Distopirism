using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //CharacterManager Character = GameObject.Find("CharacterManager").GetComponent<CharacterManager>();
    public enum GameState
    {
        start, playerTurn, enemyTurn, win, pause
    }

    public GameState state;
    public bool isLive; //적 생존 시
    public bool EnemySelect; //적 선택 여부

    void Awake()
    {
        state = GameState.start;    //전투 시작 알림
        BattleStart();
    }

    void BattleStart()
    {
        //전투 시작시 캐릭터 등장 애니메이션 등 효과를 넣고 싶으면 여기 아래에

        //플레이어나 적에게 턴 넘기기

        state = GameState.playerTurn;
        isLive = true;
    }

    //공격 버튼
    public void PlayerAttackButton()
    {
                //플레이어 턴이 아닐 때 방지

        if(state != GameState.playerTurn)
        {
            return;
        }
        StartCoroutine(PlayerAttack());
    }

    IEnumerator selectTarget()  //내 캐릭터&타겟 선택
    {
        while(true)
        {
            if (state != GameState.enemyTurn && Input.GetMouseButtonDown(0))
            {
                //GameObject clickObject =
                
            }
        }
    }
    IEnumerator PlayerAttack()  //플레이어 공격턴
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("플레이어 공격");
        //공격 스킬, 데미지 등 코드 작성

        if(!isLive)
        {
            state = GameState.win;
            EndBattle();
        }
        else//적 공격 턴으로 전환
        {
            state = GameState.enemyTurn;
            StartCoroutine(EnemyTurn());
        }
    }

    void EndBattle()    //전투 종료
    {
        Debug.Log("전투 종료");
    }

    IEnumerator EnemyTurn() //적 공격턴
    {
        yield return new WaitForSeconds(1f);
        //적 공격 코드
        Debug.Log("적 공격");

        //적 공격 끝났으면 플레이어에게 턴 넘기기
        state = GameState.playerTurn;
    }
}
