using Ami.BroAudio;
using UnityEngine;

public class StarPlanet : MonoBehaviour
{
    [HideInInspector]
    public EatSO currentSO;
    [HideInInspector]
    public float currentScale;
    [HideInInspector]
    public int score;

    [Header("블랙홀 설정")]
    public Transform blackholeCenter;     // 중심점 (블랙홀)
    public float rotateSpeed = 180f;      // 도/초
    public float shrinkSpeed = 0.5f;      // 축소 속도
    public float pullRate = 0.97f;        // 감기 속도 (0.95~0.99)
    public float pullDuration = 0.97f;        // 감기 속도 (0.95~0.99)
    public float magnetisRotateSpeed = 180f;      // 도/초

    [Header("파괴 조건")]
    public float minScaleThreshold = 0.1f;   // 평균 스케일 기준
    public float minRadiusThreshold = 0.05f; // 반지름 기준

    private bool isAbsorbing = false;

    private float currentRadius;
    private float currentAngle;
    [Header("오디오")]
    [SerializeField]
    private SoundID eatSFX;
    private void Start()
    {
        pullDuration = Random.Range(70,200);
    }

    public void StartBlackhole()
    {
        if (blackholeCenter == null) return;

        isAbsorbing = true;

        // 현재 위치에서 블랙홀 중심까지의 거리 및 각도 계산
        Vector3 offset = transform.position - blackholeCenter.position;
        currentRadius = offset.magnitude;
        currentAngle = Mathf.Atan2(offset.y, offset.x);
    }

    void Update()
    {
        if (!isAbsorbing && blackholeCenter == null) return;

        // 1. 회전 (라디안 단위로 누적)
        float angleDelta = rotateSpeed * Mathf.Deg2Rad * Time.deltaTime;
        currentAngle += angleDelta;
        transform.Rotate(0, 0, magnetisRotateSpeed * Time.deltaTime);

        // 2. 중심으로 감기 (점점 반지름 축소)
        currentRadius *= Mathf.Pow(pullRate, Time.deltaTime * pullDuration); // 프레임 보정

        // 3. 새로운 위치 계산
        Vector3 offset = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0f) * currentRadius;
        transform.position = blackholeCenter.position + offset;

        // 4. 점점 작아지기
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);

        // 5. 크기 기준 or 거리 기준으로 제거
        float avgScale = (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
        if (avgScale < minScaleThreshold || currentRadius < minRadiusThreshold)
        {
            BroAudio.Play(eatSFX);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isAbsorbing)
        {
            blackholeCenter = collision.gameObject.transform;
            GameManager.Instance.Score(score);
            StartBlackhole();
        }
    }

}
