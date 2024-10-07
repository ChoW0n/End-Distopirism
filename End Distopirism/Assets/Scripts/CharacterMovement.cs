using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 2f;  // ������ �ӵ�
    public float moveAmount = 0.1f; // ���Ʒ��� �����̴� ��

    private BattleManager battleManager; // BattleManager �ν��Ͻ�

    void Start()
    {
        // BattleManager�� ã���ϴ�.
        battleManager = FindObjectOfType<BattleManager>();
    }

    void Update()
    {
        // ��� �÷��̾�� �� ��ü�� �����ɴϴ�.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // �÷��̾� ó��
        foreach (GameObject player in players)
        {
            if (battleManager != null && !battleManager.isAttacking) // ���� ���� �ƴ� ���� ������
            {
                Vector3 position = player.transform.position;
                position.y += Mathf.Sin(Time.time * moveSpeed) * moveAmount; // ���Ʒ��� ������
                player.transform.position = position;
            }
        }

        // �� ó��
        foreach (GameObject enemy in enemies)
        {
            if (battleManager != null && !battleManager.isAttacking) // ���� ���� �ƴ� ���� ������
            {
                Vector3 position = enemy.transform.position;
                position.y += Mathf.Sin(Time.time * moveSpeed) * moveAmount; // ���Ʒ��� ������
                enemy.transform.position = position;
            }
        }
    }
}
