using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleLine : MonoBehaviour
{
    [Header("Line Settings")]
    public List<RectTransform> leftLines = new List<RectTransform>();
    public List<RectTransform> rightLines = new List<RectTransform>();
    public float scrollSpeed = 100f;
    
    [Header("Layout Settings")]
    public float resetPosition = 1920f;
    private float[] leftPositions;
    private float[] rightPositions;
    private float[] lineWidths; // 각 라인의 너비 저장
    private bool initialized = false;

    [Header("Line Objects")]
    public List<GameObject> leftLineObjects = new List<GameObject>();
    public List<GameObject> rightLineObjects = new List<GameObject>();

    private void Start()
    {
        InitializeLines();
        
        // 라인 오브젝트 리스트가 비어있다면 자동으로 채우기
        if (leftLineObjects.Count == 0 || rightLineObjects.Count == 0)
        {
            // 왼쪽 라인들을 GameObject 리스트에 추가
            foreach (var line in leftLines)
            {
                if (line != null)
                {
                    leftLineObjects.Add(line.gameObject);
                }
            }

            // 오른쪽 라인들을 GameObject 리스트에 추가
            foreach (var line in rightLines)
            {
                if (line != null)
                {
                    rightLineObjects.Add(line.gameObject);
                }
            }
        }

        // 시작 시 모든 라인 비활성화
        SetLinesActive(false);
    }

    private void InitializeLines()
    {
        // 위치 배열 초기화
        leftPositions = new float[leftLines.Count];
        rightPositions = new float[rightLines.Count];
        lineWidths = new float[Mathf.Max(leftLines.Count, rightLines.Count)];

        // 라인 너비 저장
        for (int i = 0; i < leftLines.Count; i++)
        {
            if (leftLines[i] != null)
            {
                lineWidths[i] = leftLines[i].rect.width;
            }
        }

        // 왼쪽 라인들의 초기 위치 설정 (연속적으로 배치)
        float currentLeftPos = 0;
        for (int i = 0; i < leftLines.Count; i++)
        {
            if (leftLines[i] != null)
            {
                leftPositions[i] = currentLeftPos;
                Vector2 anchoredPosition = leftLines[i].anchoredPosition;
                anchoredPosition.x = currentLeftPos;
                leftLines[i].anchoredPosition = anchoredPosition;
                currentLeftPos += lineWidths[i]; // 다음 라인은 현재 라인 바로 뒤에 배치
            }
        }

        // 오른쪽 라인들의 초기 위치 설정 (연속적으로 배치)
        float currentRightPos = 0;
        for (int i = 0; i < rightLines.Count; i++)
        {
            if (rightLines[i] != null)
            {
                rightPositions[i] = currentRightPos;
                Vector2 anchoredPosition = rightLines[i].anchoredPosition;
                anchoredPosition.x = currentRightPos;
                rightLines[i].anchoredPosition = anchoredPosition;
                currentRightPos += lineWidths[i]; // 다음 라인은 현재 라인 바로 뒤에 배치
            }
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        // 왼쪽 이동 라인 업데이트
        for (int i = 0; i < leftLines.Count; i++)
        {
            if (leftLines[i] != null)
            {
                // 왼쪽으로 이동
                leftPositions[i] -= scrollSpeed * Time.deltaTime;
                Vector2 anchoredPosition = leftLines[i].anchoredPosition;
                anchoredPosition.x = leftPositions[i];
                leftLines[i].anchoredPosition = anchoredPosition;

                // 화면 밖으로 나가면 마지막 라인의 뒤에 배치
                if (leftPositions[i] < -resetPosition)
                {
                    // 마지막 라인의 위치 찾기
                    float lastLinePosition = -float.MaxValue;
                    for (int j = 0; j < leftLines.Count; j++)
                    {
                        if (j != i && leftPositions[j] > lastLinePosition)
                        {
                            lastLinePosition = leftPositions[j];
                        }
                    }
                    // 마지막 라인의 뒤에 배치
                    leftPositions[i] = lastLinePosition + lineWidths[i] - 10;
                }
            }
        }

        // 오른쪽 이동 라인 업데이트
        for (int i = 0; i < rightLines.Count; i++)
        {
            if (rightLines[i] != null)
            {
                // 오른쪽으로 이동
                rightPositions[i] += scrollSpeed * Time.deltaTime;
                Vector2 anchoredPosition = rightLines[i].anchoredPosition;
                anchoredPosition.x = rightPositions[i];
                rightLines[i].anchoredPosition = anchoredPosition;

                // 화면 밖으로 나가면 마지막 라인의 뒤에 배치
                if (rightPositions[i] > resetPosition)
                {
                    // 마지막 라인의 위치 찾기
                    float lastLinePosition = float.MaxValue;
                    for (int j = 0; j < rightLines.Count; j++)
                    {
                        if (j != i && rightPositions[j] < lastLinePosition)
                        {
                            lastLinePosition = rightPositions[j];
                        }
                    }
                    // 마지막 라인의 뒤에 배치
                    rightPositions[i] = lastLinePosition - lineWidths[i] + 10;
                }
            }
        }
    }

    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    public void ToggleScroll(bool enable)
    {
        enabled = enable;
    }

    // 라인 활성화/비활성화 메서드 추가
    public void SetLinesActive(bool active)
    {
        Debug.Log($"라인 {(active ? "활성화" : "비활성화")} 시도");
        
        // 왼쪽 라인 활성화/비활성화
        foreach (var lineObj in leftLineObjects)
        {
            if (lineObj != null)
            {
                lineObj.SetActive(active);
                Debug.Log($"왼쪽 라인 {lineObj.name} {(active ? "활성화됨" : "비활성화됨")}");
            }
        }
        
        // 오른쪽 라인 활성화/비활성화
        foreach (var lineObj in rightLineObjects)
        {
            if (lineObj != null)
            {
                lineObj.SetActive(active);
                Debug.Log($"오른쪽 라인 {lineObj.name} {(active ? "활성화됨" : "비활성화됨")}");
            }
        }

        // 스크롤 활성화/비활성화
        enabled = active;
    }
} 