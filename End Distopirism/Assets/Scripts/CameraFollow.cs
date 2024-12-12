using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float originalFOV;
    private Camera mainCamera;
    private BattleLine battleLine;

    [Header("Camera Settings")]
    public float smoothSpeed = 5f;
    public float zoomInFOV = 45f;
    public float normalFOV = 60f;
    public float combatHeight = 3f;
    public float combatAngle = 15f;

    [Header("Combat Camera")]
    public float sideViewAngle = 25f; // 측면 뷰 각도
    public float heightOffset = 2f; // 높이 오프셋
    public float distanceOffset = 5f; // 거리 오프셋
    public float transitionDuration = 0.8f; // 전환 시간
    private bool isLeftSideView = true; // 현재 카메라가 왼쪽에서 보고 있는지

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalFOV = mainCamera.fieldOfView;
        
        battleLine = FindObjectOfType<BattleLine>();
        if (battleLine == null)
        {
            Debug.LogWarning("BattleLine을 찾을 수 없습니다.");
        }
    }

    public void ZoomInOnTarget(Transform target)
    {
        if (target == null) return;

        // 기존 트윈 취소
        transform.DOKill();
        mainCamera.DOKill();

        StartCoroutine(CombatCameraSequence(target));
    }

    private IEnumerator CombatCameraSequence(Transform target)
    {
        if (target == null) yield break;

        // 전투 중인 두 캐릭터 찾기
        Transform otherCharacter = FindOpponentCharacter(target);
        if (otherCharacter == null) yield break;

        // 중앙점 계산
        Vector3 centerPoint = (target.position + otherCharacter.position) / 2.5f;
        //중앙점의 높이를 캐릭터에 발에서 더 위로
        // y축 높이를 조정
        //float heightOffset = -2f; // 캐릭터 높이에 따라 조정할 값
        //centerPoint.y += heightOffset;
        float distance = Vector3.Distance(target.position, otherCharacter.position);

        // 카메라 방향 전환 (이전 방향의 반대로)
        isLeftSideView = !isLeftSideView;
        float currentSideAngle = isLeftSideView ? -sideViewAngle : sideViewAngle;
        
        // 1. 비스듬한 앵글로 전환
        Vector3 sidePosition = centerPoint + 
            Quaternion.Euler(0, currentSideAngle, 0) * (Vector3.back * (distance + distanceOffset)) + 
            Vector3.up * heightOffset;
        
        Quaternion sideRotation = Quaternion.LookRotation(centerPoint - sidePosition);

        // DOTween을 사용하여 부드러운 카메라 이동
        transform.DOMove(sidePosition, transitionDuration).SetEase(Ease.InOutQuad);
        transform.DORotateQuaternion(sideRotation, transitionDuration).SetEase(Ease.InOutQuad);
        mainCamera.DOFieldOfView(zoomInFOV, transitionDuration).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(transitionDuration);

        // 2. 약간의 카메라 흔들림 효과
        Vector3 originalSidePosition = sidePosition;
        float shakeTime = 0.3f;
        float shakeIntensity = 0.1f;
        
        float elapsed = 0;
        while (elapsed < shakeTime)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            
            transform.position = originalSidePosition + new Vector3(offsetX, offsetY, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 원앙점으로 카메라 복귀
        //transform.position = originalSidePosition;
    }

    // 상대 캐릭터를 찾는 메서드
    private Transform FindOpponentCharacter(Transform currentCharacter)
    {
        CharacterProfile currentProfile = currentCharacter.GetComponent<CharacterProfile>();
        if (currentProfile == null) return null;

        // 현재 캐릭터가 플레이어인 경우
        if (currentProfile.CompareTag("Player"))
        {
            // BattleManager에서 현재 선택된 적 찾기
            int playerIndex = BattleManager.Instance.playerObjects.IndexOf(currentProfile);
            if (playerIndex >= 0 && playerIndex < BattleManager.Instance.targetObjects.Count)
            {
                return BattleManager.Instance.targetObjects[playerIndex]?.transform;
            }
        }
        // 현재 캐릭터가 적인 경우
        else if (currentProfile.CompareTag("Enemy"))
        {
            // BattleManager에서 현재 선택된 플레이어 찾기
            int enemyIndex = BattleManager.Instance.targetObjects.IndexOf(currentProfile);
            if (enemyIndex >= 0 && enemyIndex < BattleManager.Instance.playerObjects.Count)
            {
                return BattleManager.Instance.playerObjects[enemyIndex]?.transform;
            }
        }

        return null;
    }

    public void ResetCamera()
    {
        // 기존 트윈 취소
        transform.DOKill();
        mainCamera.DOKill();

        // BattleLine 비활성화
        if (battleLine != null)
        {
            battleLine.SetLinesActive(false);
        }

        // 카메라를 원래 위치로 부드럽게 되돌리기
        transform.DOMove(originalPosition, transitionDuration).SetEase(Ease.InOutQuad);
        transform.DORotateQuaternion(originalRotation, transitionDuration).SetEase(Ease.InOutQuad);
        mainCamera.DOFieldOfView(originalFOV, transitionDuration).SetEase(Ease.InOutQuad);
    }

    // 전투 시작 시 호출될 메서드
    public void SetupCombatView()
    {
        // 기존 트윈 취소
        transform.DOKill();
        mainCamera.DOKill();

        Vector3 combatPosition = originalPosition + Vector3.up * combatHeight;
        Quaternion combatRotation = Quaternion.Euler(combatAngle, originalRotation.eulerAngles.y, 0);

        transform.DOMove(combatPosition, transitionDuration).SetEase(Ease.InOutQuad);
        transform.DORotateQuaternion(combatRotation, transitionDuration).SetEase(Ease.InOutQuad);
    }

    // 공격 장면에서 두 캐릭터를 모두 보여주는 메서드
    public void FocusOnDuel(Transform attacker, Transform defender)
    {
        if (attacker == null || defender == null) return;
        
        // 기존 트윈 취소
        transform.DOKill();
        mainCamera.DOKill();

        // 중앙점 계산
        Vector3 centerPoint = (attacker.position + defender.position) / 2f;
        float distance = Vector3.Distance(attacker.position, defender.position);
        
        // 공격자의 위치에 따라 카메라 각도 결정
        bool isAttackerPlayer = attacker.CompareTag("Player");
        float currentSideAngle = isAttackerPlayer ? -sideViewAngle : sideViewAngle;
        
        Vector3 duelPosition = centerPoint + 
            Quaternion.Euler(0, currentSideAngle, 0) * (Vector3.back * (distance + distanceOffset)) + 
            Vector3.up * heightOffset;
        
        Quaternion duelRotation = Quaternion.LookRotation(centerPoint - duelPosition);

        transform.DOMove(duelPosition, transitionDuration).SetEase(Ease.InOutQuad);
        transform.DORotateQuaternion(duelRotation, transitionDuration).SetEase(Ease.InOutQuad);
        mainCamera.DOFieldOfView(zoomInFOV + 5f, transitionDuration).SetEase(Ease.InOutQuad);
    }
}