using Ami.BroAudio;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [HideInInspector] public EatSO currentSO;
    [HideInInspector] public float currentScale;
    [HideInInspector] public float currentLevel;
    [HideInInspector] public int score;

    [Header("블랙홀 설정")]
    public float rotateSpeed = 180f;            // 도/초
    public float shrinkSpeed = 0.5f;            // 축소 애니메이션 속도
    public float magneticForceSizeSpeed = 0.1f; // 자력 축소 속도
    public float magnetisRotateSpeed = 180f;    // 도/초
    public float magnetismSpeed = 0.3f;         // 자력이 끌어당기는 속도
    public float pullRate = 0.97f;              // 감기 속도 (0.95~0.99)

    [Header("파괴 조건")]
    public float minScaleThreshold = 0.1f;      // 평균 스케일 기준
    public float minRadiusThreshold = 0.05f;    // 반지름 기준

    [Header("현재 지름")]
    [SerializeField] private float diameter;

    [Header("오디오")]
    [SerializeField] private SoundID eatSFX;

    private bool _isAbsorbing = false;
    private bool _isTry = false;
    private bool _isAte = false;
    private float _currentRadius;
    private float _currentAngle;

    private CircleCollider2D _col;
    private BlackHole _playerBlackHole;
    private Spawner _gameSpawner;

    private void Start()
    {
        _col = GetComponent<CircleCollider2D>();

        _playerBlackHole = FindAnyObjectByType<BlackHole>();
        _gameSpawner = FindAnyObjectByType<Spawner>();
    }

    public void StartBlackhole()
    {
        if (_playerBlackHole == null) return;

        _isAbsorbing = true;

        Vector3 offset = transform.position - _playerBlackHole.transform.position;
        _currentRadius = offset.magnitude;
        _currentAngle = Mathf.Atan2(offset.y, offset.x);
    }

    private void Update()
    {
        diameter = _col.radius * 2f * transform.lossyScale.x;

        if (_isTry && !_isAbsorbing)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, _playerBlackHole.transform.position, magnetismSpeed * Time.deltaTime);
        }

        if (_isAbsorbing && _playerBlackHole != null)
        {
            ProcessAbsorption();
        }
    }

    private void ProcessAbsorption()
    {
        float angleDelta = rotateSpeed * Mathf.Deg2Rad * Time.deltaTime;
        _currentAngle += angleDelta;
        transform.Rotate(0, 0, magnetisRotateSpeed * Time.deltaTime);

        _currentRadius *= Mathf.Pow(pullRate, Time.deltaTime * 60f);

        Vector3 offset = new Vector3(Mathf.Cos(_currentAngle), Mathf.Sin(_currentAngle), 0f) * _currentRadius;
        transform.position = _playerBlackHole.transform.position + offset;

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);

        float avgScale = (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;

        if (avgScale < minScaleThreshold || _currentRadius < minRadiusThreshold)
        {
            ConsumePlanet();
        }
    }

    private void ConsumePlanet()
    {
        RosenBridge.Instance.EatPlanet(currentSO.planetType);

        if (_gameSpawner != null)
        {
            _gameSpawner.planetList.Remove(gameObject);
        }

        BroAudio.Play(eatSFX);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _playerBlackHole != null)
        {
            if (_playerBlackHole.Diameter >= diameter && !_isAte)
            {
                GameManager.Instance.Score(score);
                StartBlackhole();
                _isAte = true;
                _isTry = false;
            }
            else if (!_isAte)
            {
                _isTry = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !_isAte)
        {
            _isTry = false;
            transform.localScale = new Vector2(currentScale, currentScale);
        }
    }
}