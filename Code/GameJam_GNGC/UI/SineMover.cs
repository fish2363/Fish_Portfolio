using UnityEngine;

public class SineMover : MonoBehaviour
{
    [Header("Sine Movement Settings")]
    [SerializeField] private float amplitude = 0.1f;       // 진폭
    [SerializeField] private float frequency = 1f;         // 주파수
    [SerializeField] private bool startOnEnable = true;

    private Vector3 initialLocalPosition;
    private float timeOffset;
    private bool isMoving;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        timeOffset = Random.Range(0f, Mathf.PI * 2f); // 개별 오브젝트 랜덤 오프셋
    }

    private void OnEnable()
    {
        if (startOnEnable)
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
        transform.localPosition = initialLocalPosition + Vector3.up * offset;
    }
}
