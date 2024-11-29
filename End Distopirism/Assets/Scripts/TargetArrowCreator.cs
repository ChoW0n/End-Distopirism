using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetArrowCreator : MonoBehaviour
{
    public float curveHeight = 200f;
    public int resolution = 50;
    public float arrowHeadLength = 40f;
    public float lineWidth = 5f;
    public float arrowHeadAngle = 20f;
    public float drawDuration = 0.5f;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private List<(Transform player, Transform target)> connections = new List<(Transform, Transform)>();
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    private void Start()
    {
        // 시작 시 초기화
        ClearConnections();
    }

    private LineRenderer CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("Arrow Line");
        lineObj.transform.SetParent(transform);
        LineRenderer newLineRenderer = lineObj.AddComponent<LineRenderer>();
        
        newLineRenderer.startWidth = lineWidth;
        newLineRenderer.endWidth = lineWidth;
        newLineRenderer.positionCount = 0;
        newLineRenderer.useWorldSpace = true;

        newLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        newLineRenderer.startColor = new Color(1f, 0.8f, 0.2f);
        newLineRenderer.endColor = new Color(1f, 0.4f, 0f);

        return newLineRenderer;
    }

    public void AddConnection(Transform player, Transform target)
    {
        // 이미 존재하는 연결인지 확인
        bool connectionExists = false;
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].player == player && connections[i].target == target)
            {
                connectionExists = true;
                break;
            }
        }

        if (!connectionExists)
        {
            LineRenderer newLineRenderer = CreateLineRenderer();
            lineRenderers.Add(newLineRenderer);
            connections.Add((player, target));

            var newCoroutine = StartCoroutine(DrawArrowCoroutine(lineRenderers.Count - 1));
            activeCoroutines.Add(newCoroutine);
        }
    }

    public void ClearConnections()
    {
        // 활성화된 모든 코루틴 중지
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();

        // 모든 LineRenderer 제거
        foreach (var renderer in lineRenderers)
        {
            if (renderer != null)
            {
                Destroy(renderer.gameObject);
            }
        }
        lineRenderers.Clear();
        connections.Clear();
    }

    private IEnumerator DrawArrowCoroutine(int index)
    {
        if (index >= connections.Count || index >= lineRenderers.Count)
        {
            yield break;
        }

        var lineRenderer = lineRenderers[index];
        var connection = connections[index];
        Transform start = connection.player;
        Transform end = connection.target;

        Vector3 startPoint = start.position + Vector3.up * 120f;
        Vector3 endPoint = end.position + Vector3.up * 120f;
        Vector3 controlPoint = (startPoint + endPoint) / 2f + Vector3.up * curveHeight;

        float elapsedTime = 0f;
        while (elapsedTime < drawDuration)
        {
            if (lineRenderer == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / drawDuration);

            int currentResolution = Mathf.CeilToInt(t * resolution);
            lineRenderer.positionCount = currentResolution + 1;

            for (int i = 0; i <= currentResolution; i++)
            {
                float segmentT = i / (float)resolution;
                Vector3 point = CalculateBezierPoint(segmentT, startPoint, controlPoint, endPoint);
                lineRenderer.SetPosition(i, point);
            }

            yield return null;
        }

        if (lineRenderer != null)
        {
            DrawArrowHead(endPoint, lineRenderer);
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 start, Vector3 control, Vector3 end)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return uu * start + 2 * u * t * control + tt * end;
    }

    private void DrawArrowHead(Vector3 tipPoint, LineRenderer lineRenderer)
    {
        if (lineRenderer.positionCount < 2) return;

        Vector3 direction = (tipPoint - lineRenderer.GetPosition(lineRenderer.positionCount - 2)).normalized;
        Vector3 right = Vector3.Cross(direction, Vector3.forward).normalized;

        Vector3 arrowTip = tipPoint;
        Vector3 arrowLeft = arrowTip - direction * arrowHeadLength + right * arrowHeadLength * Mathf.Tan(arrowHeadAngle * Mathf.Deg2Rad);
        Vector3 arrowRight = arrowTip - direction * arrowHeadLength - right * arrowHeadLength * Mathf.Tan(arrowHeadAngle * Mathf.Deg2Rad);

        int originalPositionCount = lineRenderer.positionCount;
        lineRenderer.positionCount = originalPositionCount + 3;
        lineRenderer.SetPosition(originalPositionCount, arrowLeft);
        lineRenderer.SetPosition(originalPositionCount + 1, arrowTip);
        lineRenderer.SetPosition(originalPositionCount + 2, arrowRight);
    }

    public void RemoveConnection(Transform player, Transform target)
    {
        for (int i = connections.Count - 1; i >= 0; i--)
        {
            if (connections[i].player == player && connections[i].target == target)
            {
                // 해당 연결의 코루틴 중지
                if (i < activeCoroutines.Count && activeCoroutines[i] != null)
                {
                    StopCoroutine(activeCoroutines[i]);
                    activeCoroutines.RemoveAt(i);
                }

                // LineRenderer 제거
                if (i < lineRenderers.Count && lineRenderers[i] != null)
                {
                    Destroy(lineRenderers[i].gameObject);
                    lineRenderers.RemoveAt(i);
                }

                // 연결 정보 제거
                connections.RemoveAt(i);
                break;
            }
        }
    }
}
