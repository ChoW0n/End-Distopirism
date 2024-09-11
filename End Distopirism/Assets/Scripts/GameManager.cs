using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State
    {
        start, playerTurn, enemyTurn, win
    }

    public State state;
    public bool isLive; //적 생존 시

    void Awake()
    {
        state = State.start;    //전투 시작 알림
        BattleStart();
    }

    void BattleStart()
    {
        //전투 시작시 캐릭터 등장 애니메이션 등 효과를 넣고 싶으면 여기 아래에

        //플레이어나 적에게 턴 넘기기

        state = State.playerTurn;
        isLive = true;
    }

    //공격 버튼
    public void PlayerAttackButton()
    {
                //플레이어 턴이 아닐 때 방지
        if(state != State.playerTurn)
        {
            return;
        }
        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("플레이어 공격");
        //공격 스킬, 데미지 등 코드 작성

        if(!isLive)
        {
            state = State.win;
            EndBattle();
        }
        else//적 공격 턴
        {
            state = State.enemyTurn;
            StartCoroutine(EnemyTurn());
        }
    }

    void EndBattle()
    {
        Debug.Log("전투 종료");
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        //적 공격 코드
        Debug.Log("적 공격");

        //적 공격 끝났으면 플레이어에게 턴 넘기기
        state = State.playerTurn;
    }
}
