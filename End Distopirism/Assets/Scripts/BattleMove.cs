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

    private void Start()
    {
        initialPosition = transform.position;
    }

    public void MoveTowardsCenter(Vector3 centerPoint, bool isPlayerSide)
    {
        Vector3 directionToCenter = (centerPoint - transform.position).normalized;
        
        if (isPlayerSide)
        {
            targetPosition = transform.position + directionToCenter * (Vector3.Distance(transform.position, centerPoint) - spacingDistance);
        }
        else
        {
            targetPosition = transform.position + directionToCenter * (Vector3.Distance(transform.position, centerPoint) - spacingDistance);
        }
        
        StartCoroutine(MoveCoroutine());
    }

    public void ReturnToInitialPosition()
    {
        targetPosition = initialPosition;
        StartCoroutine(MoveCoroutine());
    }

    public void Advance()
    {
        Vector3 direction = (targetPosition - initialPosition).normalized;
        targetPosition = transform.position + direction * advanceDistance;
        StartCoroutine(MoveCoroutine());
    }

    public void Retreat()
    {
        Vector3 direction = (initialPosition - targetPosition).normalized;
        targetPosition = transform.position + direction * retreatDistance;
        StartCoroutine(MoveCoroutine());
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
        isMoving = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}
