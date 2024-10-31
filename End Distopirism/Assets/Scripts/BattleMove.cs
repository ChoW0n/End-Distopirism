using UnityEngine;
using System.Collections;

public class BattleMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float spacingDistance = 2f;
    public float advanceDistance = 1f;
    public float retreatDistance = 1f;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    public Animator animator;

    private void Start()
    {
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
    }

    public void MoveTowardsCenter(Vector3 centerPoint, bool isPlayerSide)
    {
        Vector3 directionToCenter = (centerPoint - transform.position).normalized;

        float additionalSpacing = 50f; //추가 간격

        if (isPlayerSide)
        {
            targetPosition = transform.position + directionToCenter * (Vector3.Distance(transform.position, centerPoint) - spacingDistance - additionalSpacing);
        }
        else
        {
            targetPosition = transform.position + directionToCenter * (Vector3.Distance(transform.position, centerPoint) - spacingDistance - additionalSpacing);
        }

        // Attack 애니메이션 재생
        if (animator != null)
        {
            animator.SetBool("Attack", true); // Attack 애니메이션 재생
        }

        StartCoroutine(MoveCoroutine());
    }

    public void ReturnToInitialPosition()
    {
        targetPosition = initialPosition;

        if (animator != null)
        {
            animator.SetTrigger("Return"); // Return 애니메이션 재생
        }

        StartCoroutine(MoveCoroutine());
    }

    public void Advance()
    {
        Vector3 direction = (targetPosition - initialPosition).normalized;
        targetPosition = transform.position + direction * advanceDistance;

        if (animator != null)
        {
            animator.SetBool("Attack", true); // Attack 애니메이션 재생
        }

        StartCoroutine(MoveCoroutine());
    }

    public void Retreat()
    {
        Vector3 direction = (initialPosition - targetPosition).normalized;
        targetPosition = transform.position + direction * retreatDistance;

        if (animator != null)
        {
            animator.SetTrigger("Return"); 
        }

        StartCoroutine(RetreatCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;

        // 이동이 완료되면 Idle 애니메이션으로 전환
        if (animator != null)
        {
            animator.SetBool("Attack", false); // Attack 애니메이션 중지
            animator.SetBool("Idle", true); // Idle 애니메이션 재생
        }

        isMoving = false;
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit"); // Hit 애니메이션 재생  
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }
    private IEnumerator RetreatCoroutine()
    {
        // Return 애니메이션을 1초 동안 재생
        yield return new WaitForSeconds(0.0f);

        // Return 애니메이션 중지
        if (animator != null)
        {
            animator.SetBool("Idle", true); // Idle 애니메이션 재생
        }

        StartCoroutine(MoveCoroutine());
    }

}
