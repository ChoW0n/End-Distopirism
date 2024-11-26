using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingBackGroundEffect : MonoBehaviour
{
    [Header("움직임 설정")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float moveAmount = 0.5f;
    
    [Header("랜덤 방향 설정")]
    [SerializeField] private float directionChangeTime = 3f;
    [SerializeField] private float transitionDuration = 1f; // 방향 전환 시 부드러운 전환 시간
    
    private Vector2 startPosition;
    private float timeElapsed;
    private float prevXOffset;
    private float prevYOffset;
    private float targetXOffset;
    private float targetYOffset;
    private float transitionTime;
    private bool isTransitioning;

    private void Start()
    {
        startPosition = transform.position;
        SetNewRandomDirection();
        prevXOffset = targetXOffset;
        prevYOffset = targetYOffset;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        
        if (timeElapsed >= directionChangeTime)
        {
            StartTransition();
            timeElapsed = 0f;
        }

        float currentXOffset, currentYOffset;

        if (isTransitioning)
        {
            transitionTime += Time.deltaTime;
            float t = transitionTime / transitionDuration;
            t = Mathf.SmoothStep(0, 1, t); // 더 부드러운 전환을 위해 SmoothStep 사용

            currentXOffset = Mathf.Lerp(prevXOffset, targetXOffset, t);
            currentYOffset = Mathf.Lerp(prevYOffset, targetYOffset, t);

            if (transitionTime >= transitionDuration)
            {
                isTransitioning = false;
            }
        }
        else
        {
            currentXOffset = targetXOffset;
            currentYOffset = targetYOffset;
        }

        float xMovement = Mathf.Sin(Time.time * moveSpeed + currentXOffset) * moveAmount;
        float yMovement = Mathf.Cos(Time.time * moveSpeed + currentYOffset) * moveAmount;

        transform.position = startPosition + new Vector2(xMovement, yMovement);
    }

    private void StartTransition()
    {
        isTransitioning = true;
        transitionTime = 0f;
        prevXOffset = targetXOffset;
        prevYOffset = targetYOffset;
        SetNewRandomDirection();
    }

    private void SetNewRandomDirection()
    {
        targetXOffset = Random.Range(0f, 2f * Mathf.PI);
        targetYOffset = Random.Range(0f, 2f * Mathf.PI);
    }
}
