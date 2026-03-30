using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JuiceController : AdvancedDragObj
{
    public JuiceManager juiceMng { get; private set; }

    [SerializeField] Image selfImg;
    [SerializeField] Sprite[] stateSprites; // none, empty, complete
    [SerializeField] GameObject overFlowObj;
    [SerializeField] RectTransform juiceFillMask;
    [SerializeField] GameObject emptyObj;

    private Coroutine fillingCo;
    private bool isFilling;

    private JuiceState _curState;
    public JuiceState CurState
    {
        get => _curState;
        set {
            
            if (_curState == value) return;
            _curState = value;
            ApplyState();
        }
    }

    public float filledAmount { get; private set; }

    private readonly float successTime = 6f;
    private readonly float successY = 0.7f;
    private readonly float overflowTime = 15f;
    private readonly float overflowY = 1f;

    public void SetManager(JuiceManager mng) => juiceMng = mng;
    public override void Execute(DragObj obj) => base.Execute(obj);

    public bool CheckOrder(JuiceState state)
    {
        if (state == CurState + 1) { CurState = CurState + 1; return true; }
        return false;
    }

    public void PowerToggled(bool powerOn)
    {
        // Over 상태에서 버튼을 끄면 드래그 허용
        if (CurState == JuiceState.Over) SetDragEnabled(!powerOn);
    }

    private void ApplyState()
    {
        StageTrigger.Instance.CupPut(CurState);
        SetDragEnabled(true); // 기본 허용

        switch (CurState)
        {
            case JuiceState.None:
                SafeStop();
                overFlowObj.SetActive(false);
                emptyObj.SetActive(false);
                selfImg.enabled = true;
                selfImg.sprite = stateSprites[0];
                break;

            case JuiceState.Set:
                SafeStop();
                selfImg.sprite = stateSprites[1];
                emptyObj.SetActive(true);
                FillJuice(0f);
                UIManager.Get<UIPopupJuice>().ActivePowerBtn(true, powerOn: false);
                break;

            case JuiceState.Filling:
                SetDragEnabled(false); // 채우는 중엔 금지
                UIManager.Get<UIPopupJuice>().SetFillingUI(true, powerOn: true);
                StartFilling(successTime, successY);
                Debug.LogWarning($"!!!Curstate!!!{dragEnabled}");
                break;

            case JuiceState.Paused:
                UIManager.Get<UIPopupJuice>().SetFillingUI(false, powerOn: false);
                SafeStop();
                break;

            case JuiceState.Endable:
                UIManager.Get<UIPopupJuice>().SetFillingUI(true, powerOn: true);
                StartFilling(overflowTime, overflowY); // 독립 구간으로 재보간
                break;

            case JuiceState.End:
                UIManager.Get<UIPopupJuice>().SetFillingUI(false, powerOn: false);
                SafeStop();
                break;

            case JuiceState.Pilled:
                SafeStop();
                selfImg.sprite = stateSprites[2];
                emptyObj.SetActive(false);
                break;

            case JuiceState.Over:
                SafeStop();
                UIManager.Get<UIPopupJuice>().SetFillingUI(false, powerOn: true);
                SetDragEnabled(false); // 전원 켜진 상태에서는 금지
                selfImg.sprite = stateSprites[0];
                emptyObj.SetActive(false);
                overFlowObj.SetActive(true);
                UIManager.Get<UIPopupJuice>().ActiveOverflowAlert();
                break;

            case JuiceState.Submit:
                SafeStop();
                break;
        }
    }

    private void StartFilling(float duration, float targetY)
    {
        if (isFilling) return;
        fillingCo = StartCoroutine(FillingCoroutine(duration, targetY));
        isFilling = true;
    }

    private void SafeStop()
    {
        if (fillingCo != null) { StopCoroutine(fillingCo); fillingCo = null; }
        isFilling = false;
    }

    private void FillJuice(float amount01)
    {
        var s = juiceFillMask.localScale;
        var y = Mathf.Clamp01(amount01);
        juiceFillMask.localScale = new Vector3(s.x, y, s.z);
        filledAmount = y;
    }

    private IEnumerator Defer(System.Action a) { yield return null; a?.Invoke(); }

    private IEnumerator FillingCoroutine(float duration, float targetY)
    {
        float startY = juiceFillMask.localScale.y;

        if (targetY <= startY || duration <= 0f)
        {
            FillJuice(targetY);
            isFilling = false; fillingCo = null;
            UIManager.Get<UIPopupJuice>().SetFillingUI(false, powerOn: false);
            StartCoroutine(Defer(() => CurState = (juiceFillMask.localScale.y >= 1f) ? JuiceState.Over : JuiceState.Endable));
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float y = Mathf.Lerp(startY, targetY, t);
            FillJuice(y);
            yield return null;
        }

        FillJuice(targetY);
        isFilling = false; fillingCo = null;
        UIManager.Get<UIPopupJuice>().SetFillingUI(false, powerOn: false);

        StartCoroutine(Defer(() => CurState = (juiceFillMask.localScale.y >= 1f) ? JuiceState.Over : JuiceState.Endable));
    }
}
