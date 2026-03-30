using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class KettleAnim : MonoBehaviour
{
    [SerializeField] private float moveDistance = 30f;   // 위로 얼마나 올릴지
    [SerializeField] private float duration = 1.5f;      // 올라가는 시간
    [SerializeField] private Ease easeType = Ease.OutSine;

    private RectTransform rt;
    private Vector2 startPos;

    private void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;

        // Sequence 생성
        Sequence seq = DOTween.Sequence();

        // 위치 이동 Tween
        seq.Append(rt.DOAnchorPosY(startPos.y + moveDistance, duration).SetEase(easeType));

        // Fade Tween
        seq.Append(GetComponentInChildren<Image>().DOFade(0f, 0.2f).SetEase(Ease.Linear));

        // Tween 종료 시 오브젝트 삭제
        seq.OnComplete(() => Destroy(gameObject));
    }

    private void OnDisable()
    {
        rt.DOKill();
        rt.anchoredPosition = startPos;
    }
}
