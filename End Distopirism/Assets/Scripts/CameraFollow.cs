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

    [Header("CurrentTarget")]
    public Transform currentTarget;
    public bool isFollowing = false;
    public bool isBattle = false;

    [Header("Cursor Follow Settings")]
    public float cursorFollowIntensity = 0.1f;    // 마우스 움직임에 따른 카메라 이동 강도
    public float cursorFollowSmoothness = 5f;     // 카메라 움직임 부드러움
    private Vector3 initialPosition;               // 카메라 초기 위치
    private Vector3 targetPosition;                // 목표 위치

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 40f;
    public float maxZoom = 80f;
    private float targetZoom;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalFOV = mainCamera.fieldOfView;
        targetZoom = originalFOV;  // 초기 줌 값 설정
        
        battleLine = FindObjectOfType<BattleLine>();
        if (battleLine == null)
        {
            Debug.LogWarning("BattleLine을 찾을 수 없습니다.");
        }

        initialPosition = transform.position;
        targetPosition = initialPosition;
    }

    private void LateUpdate()
    {
        if (isBattle)
        {
            HandleBattleCamera();
        }
        else
        {
            HandleCursorFollow();
            HandleZoom();
        }
    }

    private void HandleBattleCamera()
    {
        if (isFollowing && currentTarget != null)
        {
            // 기존 전투 카메라 로직
            Vector3 targetPosition = currentTarget.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }

    private void HandleCursorFollow()
    {
        // 마우스 위치를 -1 ~ 1 범위로 정규화
        Vector2 mousePos = new Vector2(
            (Input.mousePosition.x / Screen.width) * 2 - 1,
            (Input.mousePosition.y / Screen.height) * 2 - 1
        );

        // 마우스 위치에 따른 오프셋 계산
        Vector3 offset = new Vector3(
            mousePos.x * cursorFollowIntensity,
            mousePos.y * cursorFollowIntensity,
            0
        );

        // 목표 위치 계산 (초기 위치 + 오프셋)
        targetPosition = initialPosition + offset;

        // 부드러운 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * cursorFollowSmoothness
        );
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetZoom = Mathf.Clamp(targetZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
        }
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetZoom, Time.deltaTime * cursorFollowSmoothness);
    }

    public void ZoomInOnTarget(Transform target)
    {
        if (target == null) return;

        isFollowing = true;

        currentTarget = target;

        // 기존 트윈 취소
        transform.DOKill();
        mainCamera.DOKill();

        // 전투 중인 두 캐릭터 찾기
        Transform otherCharacter = FindOpponentCharacter(target);
        if (otherCharacter == null) return;

        // 중앙점 계산
        Vector3 centerPoint = (target.position + otherCharacter.position) / 2.5f;
        float distance = Vector3.Distance(target.position, otherCharacter.position);

        // 카메라 방향 전환 (이전 방향의 반대로)
        isLeftSideView = !isLeftSideView;
        float currentSideAngle = isLeftSideView ? -sideViewAngle : sideViewAngle;
        
        // 비스듬한 앵글로 전환
        Vector3 sidePosition = centerPoint + 
            Quaternion.Euler(0, currentSideAngle, 0) * (Vector3.back * (distance + distanceOffset)) + 
            Vector3.up * heightOffset;
        
        Quaternion sideRotation = Quaternion.LookRotation(centerPoint - sidePosition);

        // DOTween을 사용하여 부드러운 카메라 이동
        transform.DOMove(sidePosition, transitionDuration).SetEase(Ease.InOutQuad);
        transform.DORotateQuaternion(sideRotation, transitionDuration).SetEase(Ease.InOutQuad);
        mainCamera.DOFieldOfView(zoomInFOV, transitionDuration).SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                // 카메라 흔들림 효과
                Vector3 originalSidePosition = sidePosition;
                float shakeTime = 0.3f;
                float shakeIntensity = 0.1f;
                
                DOTween.Sequence()
                    .Append(transform.DOShakePosition(shakeTime, shakeIntensity))
                    .OnComplete(() => {
                        // 원래 위치로 복귀
                        transform.position = originalSidePosition;
                    });
            });
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
        currentTarget = null;
        isFollowing = false;
        isBattle = false;
        targetZoom = originalFOV;  // 줌 초기화

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
        isBattle = true;
        targetZoom = normalFOV;  // 전투 시작 시 줌 초기화
        mainCamera.fieldOfView = normalFOV;

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

        // 중앙점 계산 (캐릭터 높이의 절반을 더해서 실제 중앙점 계산)
        Vector3 attackerCenter = attacker.position + Vector3.up * 1f; // 캐릭터 높이의 절반인 1을 더함
        Vector3 defenderCenter = defender.position + Vector3.up * 1f;
        Vector3 centerPoint = (attackerCenter + defenderCenter) / 2f;
        float distance = Vector3.Distance(attackerCenter, defenderCenter);
        
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