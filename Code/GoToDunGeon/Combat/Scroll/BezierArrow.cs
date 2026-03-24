using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class BezierArrow : MonoBehaviour
{
    [Header("Targets")]
    [field: SerializeField] public RectTransform BezierHead { get; private set; }

    private RectTransform _startTarget;
    private RectTransform _endTarget;
    private bool _isActive;
    private bool _inputEnabled = true;

    [Header("Prefabs")]
    [SerializeField] private GameObject _segmentPrefab;
    [SerializeField] private GameObject _arrowHeadPrefab;

    [Header("Curve Settings")]
    [Range(2, 50)][SerializeField] private int _segments = 12;
    [SerializeField] private float _curvature = 50f;
    [SerializeField] private float _wobbleAmplitude = 0f;
    [SerializeField] private float _wobbleSpeed = 2f;

    [Header("Style")]
    [SerializeField] private Gradient _colorOverLife;
    [SerializeField] private float _spriteUpAngle = 90f;
    private Color _baseColor = Color.red;

    // 캐싱된 컴포넌트들
    private RectTransform[] _bones;
    private Image[] _boneImages;
    private RectTransform _arrowHeadRT;
    private Image _arrowHeadImage;
    private RectTransform _container;

    [Header("UI Camera Settings")]
    [SerializeField] private Camera _uiCamera;

    private void Awake()
    {
        if (_segmentPrefab == null || _arrowHeadPrefab == null)
        {
            Debug.LogError("할당되지 않았습니다.");
            return;
        }

        _container = (RectTransform)transform;
        InitializePool();
    }

    private void InitializePool()
    {
        _bones = new RectTransform[_segments];
        _boneImages = new Image[_segments];

        for (int i = 0; i < _segments; i++)
        {
            GameObject go = Instantiate(_segmentPrefab, _container);
            go.SetActive(false);

            _bones[i] = go.GetComponent<RectTransform>();
            _boneImages[i] = go.GetComponent<Image>();
        }

        GameObject headGO = Instantiate(_arrowHeadPrefab, _container);
        headGO.SetActive(false);

        _arrowHeadRT = headGO.GetComponent<RectTransform>();
        _arrowHeadImage = _arrowHeadRT.GetComponent<Image>();
        BezierHead = _arrowHeadRT;
    }

    public void SetTargetColor(bool isTargetValid)
    {
        _baseColor = isTargetValid ? Color.green : Color.red;
    }

    public void StartBezier(RectTransform start, RectTransform end)
    {
        _isActive = true;
        _startTarget = start;
        _endTarget = end;

        _arrowHeadRT.gameObject.SetActive(true);
        foreach (var rt in _bones)
        {
            rt.gameObject.SetActive(true);
        }
    }

    public void StopBezier()
    {
        _isActive = false;
        _startTarget = null;
        _endTarget = null;

        _arrowHeadRT.gameObject.SetActive(false);
        foreach (var rt in _bones)
        {
            rt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!_isActive || _startTarget == null || _endTarget == null || !_inputEnabled)
            return;

        DrawCurve();
    }

    private void DrawCurve()
    {
        Vector2 p0 = ToLocal(_startTarget);
        Vector2 p2 = ToLocal(_endTarget);
        Vector2 direction = p2 - p0;

        if (direction.sqrMagnitude < 0.0001f) return;

        Vector2 tangent = direction.normalized;
        Vector2 normal = new Vector2(-tangent.y, tangent.x);
        float wobble = (_wobbleAmplitude > 0f) ? Mathf.Sin(Time.unscaledTime * _wobbleSpeed) * _wobbleAmplitude : 0f;

        Vector2 p1 = (p0 + p2) * 0.5f + (_curvature + wobble) * normal;

        for (int i = 0; i < _segments; i++)
        {
            float t = (_segments > 1) ? (float)i / (_segments - 1) : 1f;
            Vector2 pos = CalculateQuadraticBezierPoint(p0, p1, p2, t);
            _bones[i].anchoredPosition = pos;

            if (i < _segments - 1)
            {
                float tNext = Mathf.Min(1f, (float)(i + 1) / (_segments - 1));
                Vector2 nextPos = CalculateQuadraticBezierPoint(p0, p1, p2, tNext);
                Vector2 dirToNext = (nextPos - pos).normalized;

                float angle = Mathf.Atan2(dirToNext.y, dirToNext.x) * Mathf.Rad2Deg;
                _bones[i].localRotation = Quaternion.Euler(0, 0, angle + _spriteUpAngle + 180f);
            }

            if (_boneImages[i] != null)
            {
                Color gradientColor = _colorOverLife.Evaluate(1f - t);
                _boneImages[i].color = _baseColor * gradientColor;
            }
        }

        // 화살촉(Head) 배치 및 회전
        Vector2 headTangent = CalculateQuadraticBezierTangent(p0, p1, p2, 1f);
        float headAngle = Mathf.Atan2(headTangent.y, headTangent.x) * Mathf.Rad2Deg;

        _arrowHeadRT.anchoredPosition = p2;
        _arrowHeadRT.localRotation = Quaternion.Euler(0, 0, headAngle + _spriteUpAngle - 90f);

        if (_arrowHeadImage != null)
            _arrowHeadImage.color = _baseColor;
    }

    private Vector2 ToLocal(RectTransform target)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_uiCamera, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, screenPoint, _uiCamera, out Vector2 localPoint);
        return localPoint;
    }

    #region Bezier Math

    private static Vector2 CalculateQuadraticBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }

    private static Vector2 CalculateQuadraticBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }

    #endregion
}