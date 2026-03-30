using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public enum CursorState
{
    Default,
    Hold,
    Hover
}

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [SerializeField] private RectTransform canvasRoot;   // UI 캔버스 (Screen Space - Overlay 권장)
    [SerializeField] private GameObject floatingTextPrefab; // TextMeshProUGUI 프리팹
    [SerializeField] private Texture2D[] cursor;
    [SerializeField] Texture2D[] cursorMac; 

    private void Awake()
    {
        Instance = this;
    }

    public void SetCursor(CursorState state)
    {
        Texture2D texture;
        switch (state)
        {
            case CursorState.Default:
                texture = cursor[0];
                break;
            case CursorState.Hold:
                texture = cursor[1];
                break;
            case CursorState.Hover:
                texture = cursor[2];
                break;
            default:
                texture = cursor[0];
                break;
        }

        Vector2 hotspot = new Vector2(45f, 55f);

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        var texMac = SelectByState(cursorMac, state) ?? texture;

        // 크기 큰 커서는 소프트웨어 모드로 강제
        bool big = texMac != null && (texMac.width > 64 || texMac.height > 64);

        // 텍스처별로 유효한 핫스팟 보정(비율 유지)
        Vector2 hs = ClampHotspot(hotspot, texMac);

        Cursor.SetCursor(texMac, hs, big ? CursorMode.ForceSoftware : CursorMode.Auto);
#else
        Vector2 hs = ClampHotspot(hotspot, texture);
        Cursor.SetCursor(texture, hs, CursorMode.Auto);
#endif
    }

    static Texture2D SelectByState(Texture2D[] arr, CursorState s)
    {
        if (arr == null || arr.Length == 0) return null;
        int i = s switch { CursorState.Default => 0, CursorState.Hold => 1, CursorState.Hover => 2, _ => 0 };
        return i < arr.Length ? arr[i] : arr[0];
    }

    static Vector2 ClampHotspot(Vector2 hsPx, Texture2D tex)
    {
        if (tex == null) return hsPx;
        float x = Mathf.Clamp(hsPx.x, 0f, tex.width - 1f);
        float y = Mathf.Clamp(hsPx.y, 0f, tex.height - 1f);
        return new Vector2(x, y);
    }

    /// <summary>
    /// 문자열을 받아서 마우스 위치에 플로팅 텍스트 생성
    /// </summary>
    public void ShowFloatingText(string message,Color color)
    {
        if (canvasRoot == null || floatingTextPrefab == null) return;

        // 마우스 스크린 좌표 → UI 로컬 좌표 변환
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRoot,
            Input.mousePosition,
            canvasRoot.GetComponent<Canvas>().worldCamera,
            out pos);

        // 프리팹 생성
        GameObject go = Instantiate(floatingTextPrefab, canvasRoot);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;

        // 텍스트 설정
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.color = color;

        // 쌈뽕한 애니메이션 (위로 떠오르면서 커졌다가 사라짐)
        rt.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        seq.Join(rt.DOAnchorPosY(rt.anchoredPosition.y + 80f, 1f).SetEase(Ease.Linear));
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 120f, 1f).SetEase(Ease.OutCubic));
        seq.Join(tmp.DOFade(0f, 0.4f));
        seq.OnComplete(() => Destroy(go));
    }
}
