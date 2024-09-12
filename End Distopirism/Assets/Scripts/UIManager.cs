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
}
