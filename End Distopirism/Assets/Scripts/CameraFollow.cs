using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 카메라가 따라갈 타겟
    public float smoothSpeed = 0.125f; // 카메라 이동 속도
    public GameObject floorBackground;  //전투 시 깔아둘 바닥 오브젝트
    public Vector3 offset; // 카메라와 타겟 간의 오프셋
    public Camera mainCamera; // 메인 카메라
    public float zoomedSize = 460f; // 공격 시 카메라 사이즈
    private float initialSize; // 초기 카메라 사이즈
    private Vector3 initialPosition; // 초기 카메라 위치
    private Quaternion initialRotation; // 초기 카메라 회전

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 메인 카메라 설정
        }
        initialSize = mainCamera.orthographicSize; // 초기 카메라 사이즈 저장
        initialPosition = transform.position; // 초기 카메라 위치 저장
        initialRotation = transform.rotation; // 초기 카메라 회전 저장
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            // 타겟의 위치에 오프셋 추가
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y + offset.y+110, target.position.z + offset.z-100);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // 부드러운 이동
            transform.position = smoothedPosition; // 카메라 위치 업데이트
        }
    }

    // 공격 시 카메라를 타겟으로 이동하고 사이즈를 줄이는 메서드
    public void ZoomInOnTarget(Transform attacker)
    {
        target = attacker; // 카메라가 따라갈 타겟 설정
        StartCoroutine(SmoothZoom(zoomedSize)); // 부드럽게 줌 인
        ChangeCameraAngle(); // 카메라 각도 변경
    }

    // 카메라 각도를 공격 시 변경하는 메서드
    private void ChangeCameraAngle()
    {
        StartCoroutine(SmoothRotate(new Vector3(22f, 0f, 0f), 1f)); // X 40으로 부드럽게 회전
        //StartCoroutine(SmoothPosition(new Vector3(0f, 0f, 0f), 1f)); // Z -10으로 부드럽게 이동
    }

    private IEnumerator SmoothPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position; // 현재 카메라 위치
        Vector3 endPosition = targetPosition; // 목표 위치

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration); // 위치 보간
            elapsedTime += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }

        transform.position = endPosition; // 최종 위치 설정
    }

    // 카메라를 부드럽게 회전시키는 코루틴
    private IEnumerator SmoothRotate(Vector3 targetAngle, float duration)
    {
        Quaternion startRotation = transform.rotation; // 현재 카메라 회전
        Quaternion endRotation = Quaternion.Euler(targetAngle); // 목표 회전

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration); // 회전 보간
            elapsedTime += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }

        transform.rotation = endRotation; // 최종 회전 설정
    }

    // 카메라 사이즈를 부드럽게 줄이는 코루틴
    private IEnumerator SmoothZoom(float targetSize)
    {
        float startSize = mainCamera.orthographicSize; // 현재 카메라 사이즈
        float duration = 1f; // 줌 인에 걸리는 시간
        float elapsedTime = 0f;
        floorBackground.SetActive(true);
        while (elapsedTime < duration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsedTime / duration); // 사이즈 보간
            elapsedTime += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }

        mainCamera.orthographicSize = targetSize; // 최종 사이즈 설정
    }

    // 카메라를 초기 위치와 사이즈로 되돌리는 메서드
    public void ResetCamera()
    {
        target = null; // 타겟 초기화
        floorBackground.SetActive(false);
        StartCoroutine(SmoothZoom(initialSize)); // 부드럽게 초기 사이즈로 되돌리기
        StartCoroutine(MoveToInitialPosition()); // 초기 위치로 부드럽게 이동
        StartCoroutine(SmoothRotate(initialRotation.eulerAngles, 1f)); // 초기 회전으로 부드럽게 돌아가기
    }

    // 카메라를 초기 위치로 부드럽게 이동하는 코루틴
    private IEnumerator MoveToInitialPosition()
    {
        Vector3 startPosition = transform.position; // 현재 카메라 위치
        float duration = 1f; // 이동에 걸리는 시간
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, initialPosition, elapsedTime / duration); // 위치 보간
            elapsedTime += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }

        transform.position = initialPosition; // 최종 위치 설정
    }
}