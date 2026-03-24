using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Initialize(int damage, FloatingTextSettings settings)
    {
        // 데미지에 따른 텍스트 설정
        textMesh.text = damage > 0 ? "-" + damage.ToString() : damage.ToString();

        // 색상 보간
        float t = Mathf.Clamp01(damage / settings.maxDamage);
        textMesh.color = Color.Lerp(settings.lowDamageColor, settings.highDamageColor, t);

        // 애니메이션
        Vector3 randomOffset = new Vector3(Random.Range(-settings.randomXOffset, settings.randomXOffset), 0, 0);
        transform.localScale = Vector3.zero;

        transform.DOMove(transform.position + Vector3.up * settings.moveUpDistance + randomOffset, settings.moveDuration)
                 .SetEase(Ease.OutCubic);
        textMesh.DOFade(0, settings.moveDuration).SetEase(Ease.InQuad);
        transform.DOScale(Vector3.one * settings.startScale, settings.scaleUpTime)
                 .SetEase(Ease.OutBack);

        Destroy(gameObject, settings.moveDuration);
    }
}
