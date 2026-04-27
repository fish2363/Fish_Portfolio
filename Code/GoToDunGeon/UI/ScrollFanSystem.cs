using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScrollFanSystem : MonoBehaviour
{
    [Header("조절 가능 값")]
    [Tooltip("원의 반지름")] public float radius = 500f;
    [Tooltip("부채각")] public float angleRange = 60f;
    [Tooltip("카드가 펼쳐지는 간격")] public float horizontalSpacing = 1f;

    [Header("카드 회전 설정")]
    [Tooltip("카드가 펼쳐지는 기울기(작을 수록 덜 기울음)")] public float rotationMultiplier = 1f;
    public bool rotateCards = true;

    public const float CardRefHeight = 1280f;
    public const float DefaultSizeOffset = 0.21f; //손패 카드 사이즈 조절
    
    private static string CardRotateID = "CardRotate";

    /// <summary>
    /// 카드 재배치
    /// exclude: 현재 드래그 중인 카드
    /// </summary>
    public void ArrangeChildrenInFan(Transform exclude = null)
    {
        int count = transform.childCount;
        if (count == 0) return;
        
        RectTransform parentRt = transform as RectTransform;
        
        float t = Mathf.InverseLerp(2f, 6f, count);
        float spread = Mathf.Lerp(0.65f, 1f, t);
        
        float effectiveAngleRange = angleRange * spread;
        float effectiveRadius = radius * spread;
        
        float angleStep = count == 1 ? 0 : effectiveAngleRange / (count - 1);
        float startAngle = -effectiveAngleRange / 2f;
        
        float offsetY = CalculateReferenceOffsetY(parentRt, 0f);

        for (int visibleIndex = 0; visibleIndex < count; visibleIndex++)
        {
            Transform child = transform.GetChild(visibleIndex);

            RectTransform rt = child.GetComponent<RectTransform>();
            if (!rt) continue;
            ApplyCardScale(parentRt, rt);

            float angle = startAngle + visibleIndex * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * effectiveRadius * horizontalSpacing;
            float y = effectiveRadius * (Mathf.Cos(rad) - 1f) * 0.5f + offsetY;

            if (child != exclude)
                rt.DOLocalMove(new Vector3(x, y, 0), 0.2f).SetEase(Ease.OutQuad);

            if (rotateCards)
            {
                float rotationAngle = -angle * rotationMultiplier;

                DOTween.Kill(rt, CardRotateID);
                rt.DOLocalRotate(new Vector3(0, 0, rotationAngle), 0.2f).SetEase(Ease.OutQuad).SetId(CardRotateID);
                
            }
            else
            {
                rt.localRotation = Quaternion.identity;
            }

        }
    }
    
    public static float ComputeCardScale(RectTransform parentRt, float sizeOffset = DefaultSizeOffset)
    {
        if (!parentRt) return 1f;
        float parentH = parentRt.rect.height;
        float targetH = Mathf.Clamp(parentH * sizeOffset, 64f, 8192f);
        float scale = targetH / CardRefHeight;
        return scale;
    }
    
    public static void ApplyCardScale(RectTransform parentRt, RectTransform rt, float sizeOffset = DefaultSizeOffset)
    {
        float scale = ComputeCardScale(parentRt, sizeOffset);
        rt.localScale = new Vector3(scale, scale, 1f);
    }
    
    private static float CalculateReferenceOffsetY(RectTransform parentRt, float referencePx)
    {
        CanvasScaler scaler = parentRt ? parentRt.GetComponentInParent<CanvasScaler>() : null;
        if (!scaler) return 0;
        
        Vector2 refRes = scaler.referenceResolution;
        float match = scaler.matchWidthOrHeight;
        float scaleX = Screen.width / refRes.x;
        float scaleY = Screen.height / refRes.y;
        float scale = Mathf.Lerp(scaleX, scaleY, match);

        float units = referencePx / Mathf.Max(0.0001f, scale);
        float bottomInsetUnits = Screen.safeArea.yMin / Mathf.Max(0.0001f, scale);
        return units + bottomInsetUnits;
    }
}
