using UnityEngine;
using UnityEngine.UI;

public class AlphaRaycastImage : Image
{
    [SerializeField, Range(0f, 1f)]
    [Tooltip("이 값보다 낮은 알파값을 가진 픽셀은 Raycast를 통과시킵니다")]
    new private float alphaHitTestMinimumThreshold = 0.1f;

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, eventCamera))
        {
            return false;
        }

        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local))
        {
            return false;
        }

        Rect rect = rectTransform.rect;
        Vector2 normalizedLocal = Rect.PointToNormalized(rect, local);
        
        if (sprite != null && sprite.texture != null)
        {
            // Read/Write Enabled 체크
            if (!sprite.texture.isReadable)
            {
                Debug.LogWarning($"Texture2D {sprite.texture.name}의 Read/Write Enabled가 체크되어 있지 않습니다. 알파 테스트가 작동하지 않을 수 있습니다.");
                return true;
            }

            Vector2 spriteSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Vector2 pixelPos = new Vector2(
                Mathf.Clamp(normalizedLocal.x * spriteSize.x, 0, spriteSize.x - 1),
                Mathf.Clamp(normalizedLocal.y * spriteSize.y, 0, spriteSize.y - 1)
            );

            Color pixelColor = sprite.texture.GetPixel((int)pixelPos.x, (int)pixelPos.y);
            return pixelColor.a >= alphaHitTestMinimumThreshold;
        }

        return true;
    }
} 