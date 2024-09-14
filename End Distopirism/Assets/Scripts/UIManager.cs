using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton �ν��Ͻ�
    public static UIManager Instance
    {
        get
        {
            if (uim_instance == null)
            {
                uim_instance = FindObjectOfType<UIManager>();

                // ���� UIManager �ν��Ͻ��� �������� ������ ���� ���
                if (uim_instance == null)
                {
                    Debug.LogError("UIManager �ν��Ͻ��� ã�� �� �����ϴ�.");
                }
                else
                {
                    // �̱��� �ν��Ͻ��� �ı����� �ʵ��� ����
                    DontDestroyOnLoad(uim_instance.gameObject);
                }
            }
            return uim_instance;
        }
    }
    private static UIManager uim_instance;

    // ���콺 Ŭ�� ��ġ���� ������Ʈ ��ȯ
    public GameObject MouseGetObject()
    {
        // ���콺 ��ġ�� ���� ��ǥ�� ��ȯ
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // �����: ���콺 ��ġ ���
        //Debug.Log("���콺 ���� ��ǥ: " + pos);

        // ����ĳ��Ʈ�� ������Ʈ�� Ž��
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        GameObject clickObject = null;

        if (hit.collider != null)
        {
            clickObject = hit.transform.gameObject;

            // �����: ���õ� ������Ʈ �̸� ���
            Debug.Log("Ŭ���� ������Ʈ: " + clickObject.name);

            return clickObject;
        }

        // �����: ���� ������Ʈ�� ���� ��
        //Debug.LogWarning("����ĳ��Ʈ�� ������Ʈ�� ������ ���߽��ϴ�.");
        return null;
    }


    //�۾� ������
    public GameObject damageTextPrefab;
    public Canvas canvas;

    public void ShowDamageText(int damageAmount, Vector2 worldPosition)
    {
        //������ �ؽ�Ʈ ����
        GameObject damageText = Instantiate(damageTextPrefab, canvas.transform);

        //�ؽ�Ʈ ���� ����
        Text textComponent = damageText.GetComponentInChildren<Text>();
        textComponent.text = damageAmount.ToString();

        //���� ��ǥ�� ��ũ�� ��ǥ�� �ٲ� ĵ������ ��ġ ����
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        //�ؽ�Ʈ ��ġ�� ����
        RectTransform recttransform = damageText.GetComponentInChildren<RectTransform>();
        recttransform.position = screenPosition;

        //������ �ؽ�Ʈ�� 1�� �Ŀ� ������� ����
        Destroy(damageText, 2f);
    }
}
