using System.Collections;
using UnityEngine;

public class FloatingY : MonoBehaviour
{
    [SerializeField] private RectTransform target; // 움직일 이미지의 RectTransform
    [SerializeField] private float amplitude = 20f; // 위아래 이동 크기 (픽셀 단위)
    [SerializeField] private float speed = 1f;     // 1초에 몇 번 왕복할지(진동 속도)

    private Vector2 startPos;
    private Coroutine floatCo;

    private void OnEnable()
    {
        if (target == null) target = GetComponent<RectTransform>();
        startPos = target.anchoredPosition;

        floatCo = StartCoroutine(FloatRoutine());
    }

    private void OnDisable()
    {
        if (floatCo != null) StopCoroutine(floatCo);
        // 위치를 원래대로 복귀
        if (target != null) target.anchoredPosition = startPos;
    }

    private IEnumerator FloatRoutine()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * speed;
            // Sin 파형으로 위아래 이동
            float offsetY = Mathf.Sin(t) * amplitude;
            target.anchoredPosition = new Vector2(startPos.x, startPos.y + offsetY);
            yield return null;
        }
    }
}
