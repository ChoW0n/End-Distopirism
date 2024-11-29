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
    private bool isPlayer;

    public Animator animator;
    private CharacterProfile characterProfile;

    [Header("Screen Shake")]
    private float hitShakeDuration = 0.2f;  // 타격 시 흔들림 지속 시간
    private float hitShakeIntensity = 2f;   // 타격 시 흔들림 강도
    private float dashShakeDuration = 0.1f;  // 대시 시 흔들림 지속 시간
    private float dashShakeIntensity = 1f;   // 대시 시 흔들림 강도

    private void Start()
    {
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
        isPlayer = CompareTag("Player");
        characterProfile = GetComponent<CharacterProfile>();
    }

    public void PlayAttackSound()
    {
        if (characterProfile != null)
        {
            characterProfile.PlayHitSound();
        }
    }

    public void PlayDashSoundEvent()
    {
        if (characterProfile != null)
        {
            characterProfile.PlayDashSound();
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        targetPosition = position;

        if (animator != null)
        {
            animator.SetBool("Attack", true);
        }

        StartCoroutine(MoveCoroutine());
    }

    public void ReturnToInitialPosition()
    {
        targetPosition = initialPosition;

        if (animator != null)
        {
            animator.SetTrigger("Return");
        }

        StartCoroutine(MoveCoroutine());
    }

    public void Advance()
    {
        float direction = isPlayer ? 1f : -1f;
        float fixedY = transform.position.y;
        
        targetPosition = new Vector3(
            transform.position.x + (direction * advanceDistance),
            fixedY,
            transform.position.z
        );

        if (animator != null)
        {
            animator.SetBool("Attack", true);
        }

        StartCoroutine(MoveCoroutine());
    }

    public void Retreat()
    {
        float direction = isPlayer ? -1f : 1f;
        float fixedY = transform.position.y;

        targetPosition = new Vector3(
            transform.position.x + (direction * retreatDistance),
            fixedY,
            transform.position.z
        );

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

        if (animator != null)
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Idle", true);
        }

        isMoving = false;
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    private IEnumerator RetreatCoroutine()
    {
        yield return new WaitForSeconds(0.0f);

        if (animator != null)
        {
            animator.SetBool("Idle", true);
        }

        StartCoroutine(MoveCoroutine());
    }

    public void OnAttackHit()
    {
        if (characterProfile != null)
        {
            characterProfile.PlayHitSound();
            // 타격 시 화면 흔들림
            StartCoroutine(CameraShake.Instance.Shake(hitShakeDuration, hitShakeIntensity));
        }
    }

    public void OnMoveStart()
    {
        if (characterProfile != null)
        {
            characterProfile.PlayDashSound();
            // 대시 시 화면 흔들림
            StartCoroutine(CameraShake.Instance.Shake(dashShakeDuration, dashShakeIntensity));
        }
    }
}
