using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupJuice : UIBase
{
    JuiceManager mng;
    [SerializeField] JuiceController ctrl;

    [Header("Dispenser Button")]
    [SerializeField] Image powerImg;
    [SerializeField] Button powerBtn;

    [Header("Filling Image")]
    [SerializeField] Image fillingImg;

    [SerializeField] GameObject stackedSample;

    private Coroutine blinkCo;
    private bool powerOn;
    private bool _firstOpen = true;
    
    public override void Opened(object[] param)
    {
        mng = (JuiceManager)param[0];
        mng.SetJuiceCtrl(ctrl);
        ctrl.SetManager(mng);

        SetFillingUI(mng.IsFillingOn, powerOn: powerOn);
        //stackedSample.SetActive(mng.JuiceCount.Value > 0);
        ActivePowerBtn(true, powerOn: false);
        if (_firstOpen && !mng.IsFillingOn)
        {
            SetFillingUI(false, powerOn: false);
        }
        _firstOpen = false;
        ctrl.SetDragEnabled(true);

        fillingImg.enabled = ctrl.dragEnabled && ctrl.CurState != JuiceState.None;
        Debug.LogError($"!!!Curstate!!!{ctrl.dragEnabled}");
    }
    public override void CloseDDD()
    {
        base.CloseDDD();
        stackedSample.SetActive(false);
    }
    public void OnJuiceBtnClicked()
    {
        switch (ctrl.CurState)
        {
            case JuiceState.Set:
                powerOn = true;
                ActivePowerBtn(true, powerOn);
                ctrl.CurState = JuiceState.Filling;
                break;

            case JuiceState.Filling:
                powerOn = false;
                ActivePowerBtn(true, powerOn);
                ctrl.CurState = JuiceState.Paused;
                break;

            case JuiceState.Paused:
                powerOn = true;
                ActivePowerBtn(true, powerOn);
                ctrl.CurState = JuiceState.Filling;
                break;

            case JuiceState.Endable:
                powerOn = false;
                ActivePowerBtn(false, powerOn);
                SetFillingUI(false, powerOn);
                ctrl.CurState = JuiceState.End;
                break;

            case JuiceState.Over:
                powerOn = false;
                StopBlink();                                         // 추가: 점멸 중단
                ActivePowerBtn(true, powerOn);
                SetFillingUI(false, powerOn);
                ctrl.PowerToggled(powerOn);                          // 드래그 허용(SetDragEnabled(true))
                break;
        }
    }

    public void ActivePowerBtn(bool isActive, bool powerOn = false)
    {
        powerBtn.interactable = isActive;
        StopBlink();
        this.powerOn = powerOn;
        powerImg.color = powerOn ? mng.GetPowerColor(PowerColorType.Green)
                                 : mng.GetPowerColor(PowerColorType.Default);
        if (isActive && powerOn) StartBlink(true);
    }

    public void SetFillingUI(bool on, bool powerOn = false)
    {
        fillingImg.enabled = on;
        StopBlink();
        this.powerOn = powerOn;
        powerImg.color = on && powerOn ? mng.GetPowerColor(PowerColorType.Green)
                                       : mng.GetPowerColor(PowerColorType.Default);
        if (on && powerOn) StartBlink(true);
    }

    public void ActiveOverflowAlert()
    {
        StopBlink();
        StartBlink(false); // 빨강 점멸
    }

    public void ActiveWaitingCup(NotifyValue<int> juiceCount)
    {
        stackedSample.SetActive(juiceCount.Value > 0);
    }

    private void StartBlink(bool isGreen, float targetTime = float.MaxValue)
    {
        if (blinkCo != null) StopCoroutine(blinkCo);
        blinkCo = StartCoroutine(BlinkBtn(isGreen, targetTime));
    }

    private void StopBlink()
    {
        if (blinkCo != null) { StopCoroutine(blinkCo); blinkCo = null; }
    }

    private IEnumerator BlinkBtn(bool isGreen, float targetTime = float.MaxValue)
    {
        Color curColor = isGreen ? mng.GetPowerColor(PowerColorType.Green) : mng.GetPowerColor(PowerColorType.Red);
        float curTime = 0f; bool baseOn = true;

        while (curTime < targetTime)
        {
            powerImg.color = baseOn ? curColor : Color.white;
            baseOn = !baseOn;
            yield return new WaitForSeconds(0.2f);
            curTime += 0.2f;
        }
        powerImg.color = mng.GetPowerColor(PowerColorType.Default);
    }
}
