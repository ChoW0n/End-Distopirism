using System.Collections.Generic;
using UnityEngine;

public class Silhouette : MonoBehaviour
{
    public bool Active = false;

    [Header("실루엣 설정")]
    public int SlideEA = 5;  // 잔상 개수
    public float SlideTime = 0.05f;  // 잔상 생성 간격

    [Header("RGB 범위 설정")]
    public float RedValue = 200;     // 고정된 잔상 색상 값
    public float GreenValue = 200;
    public float BlueValue = 200;

    private GameObject bank;
    private List<GameObject> silhouetteList = new List<GameObject>();
    private int limit = 0;
    private float delta = 0;
    private bool errorDebug = false;
    private Camera mainCamera;  // 메인 카메라 참조 추가

    private void Awake()
    {
        // 메인 카메라 참조 가져오기
        mainCamera = Camera.main;

        // SpriteRenderer가 없으면 작동 중지
        if (!GetComponent<SpriteRenderer>())
        {
            Debug.LogError("SpriteRenderer가 필요합니다. 오브젝트: " + gameObject.name);
            errorDebug = true;
        }
        
        // 초기 슬루엣 오브젝트 생성
        InitializeSilhouettes();
    }

    private void InitializeSilhouettes()
    {
        bank = new GameObject($"{gameObject.name}_SilhouetteBank");

        for (int i = 0; i < SlideEA; i++)
        {
            GameObject silhouette = new GameObject($"{gameObject.name}_Silhouette_{i}");
            silhouette.transform.parent = bank.transform;
            SpriteRenderer sr = silhouette.AddComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0);  // 초기 투명 상태
            sr.sortingOrder = -3;  // 소팅 레이어를 -3으로 설정
            silhouetteList.Add(silhouette);
        }
    }

    private void ResetSilhouettes()
    {
        delta = 0;
        limit = 0;

        // 모든 실루엣의 투명도를 초기화
        foreach (GameObject silhouette in silhouetteList)
        {
            silhouette.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        }
    }

    private void FadeSilhouettes()
    {
        float fadeAmount = 3f / SlideEA;  // 빠른 페이드아웃 속도로 잔상 제거

        // 리스트를 역순으로 순회하여 파괴된 실루엣 제거
        for (int i = silhouetteList.Count - 1; i >= 0; i--)
        {
            GameObject silhouette = silhouetteList[i];
            if (silhouette == null)
            {
                silhouetteList.RemoveAt(i);
                continue;
            }

            SpriteRenderer sr = silhouette.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = Mathf.Max(0, color.a - fadeAmount * Time.deltaTime);
                sr.color = color;
            }
        }
    }

    private void Update()
    {
        if (errorDebug) return;

        // 잔상 개수가 변경되면 초기화
        if (SlideEA != silhouetteList.Count)
        {
            ResetSilhouettes();
            SlideEA = silhouetteList.Count;
        }

        // 모든 실루엣이 원본 캐릭터의 회전값을 따라가도록 수정
        foreach (GameObject silhouette in silhouetteList)
        {
            if (silhouette != null)
            {
                silhouette.transform.rotation = transform.rotation;
            }
        }

        delta += Time.deltaTime;

        // 잔상 생성 주기
        if (delta > SlideTime && Active)
        {
            delta = 0;

            // 리스트가 비어있거나 인덱스가 범위를 벗어나면 리턴
            if (silhouetteList.Count == 0 || limit >= silhouetteList.Count) return;

            GameObject silhouette = silhouetteList[limit];
            if (silhouette == null) return;

            SpriteRenderer sr = silhouette.GetComponent<SpriteRenderer>();
            SpriteRenderer originalSR = GetComponent<SpriteRenderer>();
            if (sr == null || originalSR == null) return;

            // 현재 위치, 스프라이트, 크기 및 flip 반영
            silhouette.transform.position = transform.position + new Vector3(0, 0, -0.1f);
            sr.sprite = originalSR.sprite;
            sr.flipX = originalSR.flipX;
            sr.flipY = originalSR.flipY;
            silhouette.transform.localScale = transform.localScale;

            // 일정한 잔상 색상 설정
            sr.color = new Color(RedValue / 255f, GreenValue / 255f, BlueValue / 255f, 0.7f);

            limit = (limit + 1) % SlideEA;
        }

        // 잔상 페이드아웃
        FadeSilhouettes();
    }

    private void OnDestroy()
    {
        // 실루엣 뱅크와 모든 실루엣 오브젝트 제거
        if (bank != null)
        {
            foreach (var silhouette in silhouetteList)
            {
                if (silhouette != null)
                {
                    Destroy(silhouette);
                }
            }
            silhouetteList.Clear();
            Destroy(bank);
        }
    }
}
