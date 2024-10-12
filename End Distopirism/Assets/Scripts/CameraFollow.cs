using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 타겟
    public float smoothSpeed = 0.125f; // 부드러운 이동 속도
    public Vector3 offset; // 카메라 오프셋
    private Vector3 initialPosition = new Vector3(0, 0, -10); // 초기 카메라 위치 고정

    // 줌 인 강도 설정을 위한 변수
    public float zoomAmount = 5f; // 기본 줌 인 강도

    // Start is called before the first frame update
    void Start()
    {
        transform.position = initialPosition; // 카메라를 초기 위치로 설정
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (target != null) // 타겟이 설정되어 있는지 확인
        {
            // 타겟의 위치에 오프셋을 더하여 원하는 카메라 위치 계산
            Vector3 desiredPosition = (Vector3)target.position + offset;
            // 현재 위치와 원하는 위치 사이를 부드럽게 보간하여 이동
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, initialPosition.z); // 카메라 위치 업데이트
        }
    }

    public void ZoomInOnTarget(Transform newTarget)
    {
        if (newTarget != null) // 새로운 타겟이 null이 아닌지 확인
        {
            target = newTarget; // 새로운 타겟 설정
            StartCoroutine(ZoomIn(zoomAmount)); // 줌 인 코루틴 시작
        }
        else
        {
            Debug.LogWarning("타겟이 null입니다."); // 경고 메시지 출력
        }
    }

    private IEnumerator ZoomIn(float zoomAmount)
    {
        Vector3 originalPosition = transform.position; // 원래 카메라 위치 저장
        Vector3 targetPosition = (Vector3)target.position + new Vector3(0, 0, -zoomAmount); // 타겟 위치로 이동 (Z축으로 -zoomAmount)

        float duration = 1f; // 줌 인 시간
        float elapsedTime = 0f; // 경과 시간 초기화

        // 지정된 시간 동안 카메라를 부드럽게 이동
        while (elapsedTime < duration)
        {
            transform.position = new Vector3(
                Mathf.Lerp(originalPosition.x, targetPosition.x, elapsedTime / duration),
                Mathf.Lerp(originalPosition.y, targetPosition.y, elapsedTime / duration),
                initialPosition.z // Z축은 고정
            );
            elapsedTime += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }

        transform.position = new Vector3(targetPosition.x, targetPosition.y, initialPosition.z); // 최종 위치 설정
    }

    public void ResetCamera()
    {
        StartCoroutine(ReturnToInitialPosition());
    }

    private IEnumerator ReturnToInitialPosition()
    {
        float duration = 0.5f; // 초기 위치로 돌아가는 시간
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = new Vector3(
                Mathf.Lerp(originalPosition.x, initialPosition.x, elapsedTime / duration),
                Mathf.Lerp(originalPosition.y, initialPosition.y, elapsedTime / duration),
                initialPosition.z // Z축은 고정
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = initialPosition; // 최종 위치 설정
    }
}
