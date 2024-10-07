using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 2f;  // 움직임 속도
    public float moveAmount = 0.1f; // 위아래로 움직이는 양

    private BattleManager battleManager; // BattleManager 인스턴스

    void Start()
    {
        // BattleManager를 찾습니다.
        battleManager = FindObjectOfType<BattleManager>();
    }

    void Update()
    {
        // 모든 플레이어와 적 객체를 가져옵니다.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 플레이어 처리
        foreach (GameObject player in players)
        {
            if (battleManager != null && !battleManager.isAttacking) // 공격 중이 아닐 때만 움직임
            {
                Vector3 position = player.transform.position;
                position.y += Mathf.Sin(Time.time * moveSpeed) * moveAmount; // 위아래로 움직임
                player.transform.position = position;
            }
        }

        // 적 처리
        foreach (GameObject enemy in enemies)
        {
            if (battleManager != null && !battleManager.isAttacking) // 공격 중이 아닐 때만 움직임
            {
                Vector3 position = enemy.transform.position;
                position.y += Mathf.Sin(Time.time * moveSpeed) * moveAmount; // 위아래로 움직임
                enemy.transform.position = position;
            }
        }
    }
}
