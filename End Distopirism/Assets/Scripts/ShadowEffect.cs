using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShadowEffect : MonoBehaviour
{
    public Vector3 Offset = new Vector3(-0.1f, -0.1f);
    public Material material;

    // 각도와 스케일 설정 (회전은 Vector3로 각 축별로 지정)
    public Vector3 shadowRotation = new Vector3(0f, 0f, 0f);  // X, Y, Z 각도를 설정
    public Vector3 shadowScale = new Vector3(1f, 1f, 1f);  // 그림자의 스케일 (기본값은 원본과 동일)

    GameObject _shadow;
    SpriteRenderer _shadowRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _shadow = new GameObject("Shadow");
        _shadow.transform.parent = transform;

        // 그림자의 위치, 회전, 스케일 초기화
        _shadow.transform.localPosition = Offset;
        _shadow.transform.localRotation = Quaternion.Euler(shadowRotation);  // XYZ 각도 적용
        _shadow.transform.localScale = shadowScale;  // 스케일 적용

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        _shadowRenderer = _shadow.AddComponent<SpriteRenderer>();
        _shadowRenderer.material = material;

        _shadowRenderer.sortingLayerName = renderer.sortingLayerName;
        _shadowRenderer.sortingOrder = renderer.sortingOrder - 1;
    }

    void LateUpdate()
    {
        // 본체 스프라이트에 맞춰 그림자의 스프라이트를 변경
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        _shadowRenderer.sprite = renderer.sprite;  // 본체의 스프라이트를 그림자에 실시간으로 반영

        // 그림자의 위치, 회전, 스케일 업데이트
        _shadow.transform.localPosition = Offset;
        _shadow.transform.localRotation = Quaternion.Euler(shadowRotation);  // 매 프레임마다 XYZ 회전 적용
        _shadow.transform.localScale = shadowScale;  // 매 프레임마다 스케일 적용
    }
}
