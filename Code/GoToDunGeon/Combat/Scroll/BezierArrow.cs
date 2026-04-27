using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// UI용 슬더스 스타일 뼈 기반 화살표
/// - start → end 사이를 segments 개수로 나눠 RectTransform 배치
/// - 각 Segment는 다음 Segment 방향 바라봄
/// - 마지막 Segment는 화살촉 Prefab으로
/// - 화살촉은 마우스 방향을 바라봄
/// </summary>
public class BezierArrow : MonoBehaviour
{
    [Header("Targets")]
    [field: SerializeField] public RectTransform baizerHead { get; private set; }
    RectTransform start;
    RectTransform end;
    bool isStart;
    bool inputEnabled = true; // 입력 활성화 상태

    [Header("Prefabs")]
    public GameObject segmentPrefab;
    public GameObject arrowHeadPrefab;

    [Header("Curve")]
    public int segments = 12;
    public float curvature = 50f;
    public float wobbleAmplitude = 0f;
    public float wobbleSpeed = 2f;

    [Header("Style")]
    public Gradient colorOverLife;
    private Color baseColor = Color.red;

    private RectTransform[] bones;
    private RectTransform arrowHeadRT;
    private Image arrowHeadImage;
    private Image[] arrowHeadSprite;

    private Vector3 p0, p1, p2, p3;

    private RectTransform container;   
    private Canvas canvas;             
    [SerializeField] private float spriteUpAngle = 90f; 

    void Awake()
    {
        if (segmentPrefab == null || arrowHeadPrefab == null)
        {
            Debug.LogError("SegmentPrefab과 ArrowHeadPrefab을 할당하세요!");
            return;
        }

        bones = new RectTransform[segments];
        arrowHeadSprite = new Image[segments];
        for (int i = 0; i < segments; i++)
        {
            GameObject go = Instantiate(segmentPrefab, transform);
            go.name = "Segment_" + i;
            bones[i] = go.GetComponent<RectTransform>();
            arrowHeadSprite[i] = go.GetComponent<Image>();
            go.SetActive(false);
        }

        GameObject headGO = Instantiate(arrowHeadPrefab, transform);
        headGO.name = "ArrowHead";
        arrowHeadRT = headGO.GetComponent<RectTransform>();
        arrowHeadRT.gameObject.SetActive(false);
        arrowHeadImage = arrowHeadRT.GetComponent<Image>();

        container = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
    }

    public void CanFindTarget(bool isTrue)
    {
        baseColor = isTrue ? Color.green : Color.red;
        //UpdateBones();
        //UpdateArrowHead();
    }

    public void StartBazier(RectTransform start, RectTransform end)
    {
        isStart = true;
        this.start = start;
        this.end = end;

        arrowHeadRT.gameObject.SetActive(true);
        foreach (RectTransform rt in bones)
            rt.gameObject.SetActive(true);
    }

    public void StopBazier()
    {
        isStart = false;
        this.start = null;
        this.end = null;

        arrowHeadRT.gameObject.SetActive(false);
        foreach (RectTransform rt in bones)
            rt.gameObject.SetActive(false);
    }

    //void Update()
    //{
    //    if (!isStart || start == null || end == null || !inputEnabled) return;

    //    Vector3 startPos = start.position;
    //    Vector3 endPos = end.position;

    //    Vector3 dir = endPos - startPos;
    //    float dist = dir.magnitude;
    //    Vector3 tangent = dir.normalized;
    //    Vector3 normal = Vector3.Cross(Vector3.forward, tangent).normalized;
    //    float bend = curvature + (wobbleAmplitude > 0f ? Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmplitude : 0f);

    //    p0 = startPos;
    //    p3 = endPos;
    //    p1 = startPos + tangent * (dist * 0.5f) + normal * bend;
    //    p2 = endPos - tangent * (dist * 0.5f) + normal * bend;

    //    UpdateBones();
    //    UpdateArrowHead();
    //}

    void Update()
    {
        float lastAngle = 0f; 
        if (!isStart || start == null || end == null || !inputEnabled) return;

        Vector2 p0 = ToLocal(start);
        Vector2 p2 = ToLocal(end);

        Vector2 d = p2 - p0;
        if (d.sqrMagnitude < 0.0001f) return;

        Vector2 tng = d.normalized;
        Vector2 nrm = new Vector2(-tng.y, tng.x); 

        float wobble = (wobbleAmplitude > 0f) ? Mathf.Sin(Time.unscaledTime * wobbleSpeed) * wobbleAmplitude : 0f;
        Vector2 p1 = (p0 + p2) * 0.5f + (curvature + wobble) * nrm;

        for (int i = 0; i < segments; i++)
        {
            float t = (segments > 1) ? i / (float)(segments - 1) : 1f;
            Vector2 pos = BezierPointQuad(p0, p1, p2, t);
            bones[i].anchoredPosition = pos;

            if (i < segments - 1)
            {
                float tNext = Mathf.Min(1f, (i + 1) / (float)(segments - 1));
                Vector2 next = BezierPointQuad(p0, p1, p2, tNext);
                Vector2 dir = (next - pos).normalized;
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                bones[i].localRotation = Quaternion.Euler(0, 0, ang + spriteUpAngle + 180f);
            }

            if (arrowHeadSprite[i] != null)
            {
                Color grad = colorOverLife.Evaluate(1f - t);
                arrowHeadSprite[i].color = baseColor * grad;
            }
        }

        Vector2 tan = BezierTangentQuad(p0, p1, p2, 1f);  
        float headAng = Mathf.Atan2(tan.y, tan.x) * Mathf.Rad2Deg;
        arrowHeadRT.anchoredPosition = p2;
        arrowHeadRT.localRotation = Quaternion.Euler(0, 0, headAng + spriteUpAngle - 90f);
        if (arrowHeadImage != null) arrowHeadImage.color = baseColor;
    }


    Vector2 ToLocal(RectTransform target)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(canvas ? canvas.worldCamera : null, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screen, canvas ? canvas.worldCamera : null, out var local);
        return local;
    }

    static Vector2 BezierPointQuad(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
    static Vector2 BezierTangentQuad(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }
}
