using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShadowEffect : MonoBehaviour
{
    public Vector3 Offset = new Vector3(-0.1f, -0.1f);
    public Material material;

    // ������ ������ ���� (ȸ���� Vector3�� �� �ະ�� ����)
    public Vector3 shadowRotation = new Vector3(0f, 0f, 0f);  // X, Y, Z ������ ����
    public Vector3 shadowScale = new Vector3(1f, 1f, 1f);  // �׸����� ������ (�⺻���� ������ ����)

    GameObject _shadow;
    SpriteRenderer _shadowRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _shadow = new GameObject("Shadow");
        _shadow.transform.parent = transform;

        // �׸����� ��ġ, ȸ��, ������ �ʱ�ȭ
        _shadow.transform.localPosition = Offset;
        _shadow.transform.localRotation = Quaternion.Euler(shadowRotation);  // XYZ ���� ����
        _shadow.transform.localScale = shadowScale;  // ������ ����

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        _shadowRenderer = _shadow.AddComponent<SpriteRenderer>();
        _shadowRenderer.material = material;

        _shadowRenderer.sortingLayerName = renderer.sortingLayerName;
        _shadowRenderer.sortingOrder = renderer.sortingOrder - 1;
    }

    void LateUpdate()
    {
        // ��ü ��������Ʈ�� ���� �׸����� ��������Ʈ�� ����
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        _shadowRenderer.sprite = renderer.sprite;  // ��ü�� ��������Ʈ�� �׸��ڿ� �ǽð����� �ݿ�

        // �׸����� ��ġ, ȸ��, ������ ������Ʈ
        _shadow.transform.localPosition = Offset;
        _shadow.transform.localRotation = Quaternion.Euler(shadowRotation);  // �� �����Ӹ��� XYZ ȸ�� ����
        _shadow.transform.localScale = shadowScale;  // �� �����Ӹ��� ������ ����
    }
}
