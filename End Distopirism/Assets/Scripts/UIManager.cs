using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton 인스턴스
    public static UIManager Instance
    {
        get
        {
            if (uim_instance == null)
            {
                uim_instance = FindObjectOfType<UIManager>();

                // 만약 UIManager 인스턴스가 존재하지 않으면 오류 출력
                if (uim_instance == null)
                {
                    Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다.");
                }
                else
                {
                    // 싱글톤 인스턴스를 파괴되지 않도록 설정
                    DontDestroyOnLoad(uim_instance.gameObject);
                }
            }
            return uim_instance;
        }
    }
    private static UIManager uim_instance;

    // 마우스 클릭 위치에서 오브젝트 반환
    public GameObject MouseGetObject()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 디버그: 마우스 위치 출력
        //Debug.Log("마우스 월드 좌표: " + pos);

        // 레이캐스트로 오브젝트를 탐지
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        GameObject clickObject = null;

        if (hit.collider != null)
        {
            clickObject = hit.transform.gameObject;

            // 디버그: 선택된 오브젝트 이름 출력
            Debug.Log("클릭한 오브젝트: " + clickObject.name);

            return clickObject;
        }

        // 디버그: 맞춘 오브젝트가 없을 때
        //Debug.LogWarning("레이캐스트가 오브젝트를 맞추지 못했습니다.");
        return null;
    }


    //작업 진행중
    public GameObject damageTextPrefab;
    public Canvas canvas;

    public void ShowDamageText(int damageAmount, Vector2 worldPosition)
    {
        //데미지 텍스트 생성
        GameObject damageText = Instantiate(damageTextPrefab, canvas.transform);

        //텍스트 내용 변경
        Text textComponent = damageText.GetComponentInChildren<Text>();
        textComponent.text = damageAmount.ToString();

        //월드 좌표를 스크린 좌표로 바꿔 캔버스에 위치 설정
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        //텍스트 위치를 조정
        RectTransform recttransform = damageText.GetComponentInChildren<RectTransform>();
        recttransform.position = screenPosition;

        //데미지 텍스트를 1초 후에 사라지게 설정
        Destroy(damageText, 2f);
    }
}
