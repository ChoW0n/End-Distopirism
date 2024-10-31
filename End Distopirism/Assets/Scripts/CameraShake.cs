using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    private bool isShaking = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 현재 카메라 위치를 기준으로 랜덤한 오프셋 추가
            Vector3 currentPos = transform.position;
            float x = currentPos.x + Random.Range(-1f, 1f) * magnitude;
            float y = currentPos.y + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, currentPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isShaking = false;
    }

    public bool IsShaking()
    {
        return isShaking;
    }
}