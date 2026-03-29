using UnityEngine;

public class CatchTetrisObjUI : MonoBehaviour
{
    private float amplitude = 0.1f;   // 진폭
    [SerializeField] private float frequency = 1f;     // 진동 속도
    [SerializeField] private bool startOnAwake = true;

    private Vector3 initialLocalPosition;
    private float timeOffset;

    private bool isMoving = false;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    private void OnEnable()
    {
        if (startOnAwake)
            StartMoving();
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
        transform.localPosition = initialLocalPosition;
    }

    private void LateUpdate()
    {
        if (!isMoving) return;

        float offset = Mathf.Sin(Time.time * frequency + timeOffset) * amplitude;

        // 부모 기준 로컬 Y축 방향으로 사인 곡선 오프셋 적용
        transform.localPosition = initialLocalPosition + Vector3.up * offset;
    }
}
