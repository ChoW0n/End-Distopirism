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
    public float drawDuration = 0.5f; // 화살표를 그리는 데 걸리는 시간

    private LineRenderer lineRenderer;
    private List<(Transform player, Transform target)> connections = new List<(Transform, Transform)>();
    private Coroutine drawCoroutine;

    private void Start()
    {
        CreateLineRenderer();
    }

    private void CreateLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 0.8f, 0.2f); // 밝은 노란색
        lineRenderer.endColor = new Color(1f, 0.4f, 0f); // 주황색
    }

    public void AddConnection(Transform player, Transform target)
    {
        connections.Clear(); // 기존 연결 제거
        connections.Add((player, target));
        if (drawCoroutine != null)
        {
            StopCoroutine(drawCoroutine);
        }
        drawCoroutine = StartCoroutine(DrawArrowCoroutine());
    }

    public void ClearConnections()
    {
        connections.Clear();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
        if (drawCoroutine != null)
        {
            StopCoroutine(drawCoroutine);
            drawCoroutine = null;
        }
    }

    private IEnumerator DrawArrowCoroutine()
    {
        if (connections.Count == 0 || lineRenderer == null)
        {
            yield break;
        }

        Transform start = connections[0].player;
        Transform end = connections[0].target;

        Vector3 startPoint = start.position + Vector3.up * 120f;
        Vector3 endPoint = end.position + Vector3.up * 120f;
        Vector3 controlPoint = (startPoint + endPoint) / 2f + Vector3.up * curveHeight;

        float elapsedTime = 0f;
        while (elapsedTime < drawDuration)
        {
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

        DrawArrowHead(endPoint);
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 start, Vector3 control, Vector3 end)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return uu * start + 2 * u * t * control + tt * end;
    }

    private void DrawArrowHead(Vector3 tipPoint)
    {
        Vector3 direction = (tipPoint - lineRenderer.GetPosition(lineRenderer.positionCount - 2)).normalized;
        Vector3 right = Vector3.Cross(direction, Vector3.forward).normalized;

        Vector3 arrowTip = tipPoint;
        Vector3 arrowLeft = arrowTip - direction * arrowHeadLength + right * arrowHeadLength * Mathf.Tan(arrowHeadAngle * Mathf.Deg2Rad);
        Vector3 arrowRight = arrowTip - direction * arrowHeadLength - right * arrowHeadLength * Mathf.Tan(arrowHeadAngle * Mathf.Deg2Rad);

        lineRenderer.positionCount += 3;
        lineRenderer.SetPosition(lineRenderer.positionCount - 3, arrowLeft);
        lineRenderer.SetPosition(lineRenderer.positionCount - 2, arrowTip);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, arrowRight);
    }
}
