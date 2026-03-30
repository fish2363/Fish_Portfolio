using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;   // ✅ DOShake 사용

public class UIPopupChillBread : UIBase
{
    [SerializeField] private GameObject _button;
    [SerializeField] private RectTransform _cover;
    [SerializeField] private GameObject _cancelButton;
    [SerializeField] private GameObject smokeObject; // 10초 이후 연기
    [SerializeField] private Image cookGaugeImg;     // ✅ 요리 시간 게이지

    [SerializeField] private FinishBread finishBread;
    [SerializeField] private List<ChillBreadTle> chillBreads = new();
    private List<ChillBreadTle> InChillBreads = new();

    [SerializeField] private float shakeAmount = 5f;
    [SerializeField] private float shakeSpeed = 0.05f;

    private Vector2 originalCoverPos;
    private Coroutine cookCoroutine;
    private bool cancelPressed = false;

    private void OnEnable()
    {
        if (_cover != null)
            originalCoverPos = _cover.anchoredPosition;

        _cover.gameObject.SetActive(false);
        _cancelButton.SetActive(false);
        if (smokeObject != null) smokeObject.SetActive(false);

        // ✅ 게이지 초기화
        if (cookGaugeImg != null)
        {
            cookGaugeImg.fillAmount = 1f;
            cookGaugeImg.color = Color.green;
        }
    }

    private Transform spawn;

    public void AddKitchen(WantFoodEnum foodEnum) { }

    public void AddChillBread(ChillBreadTle chillBread)
    {
        InChillBreads.Add(chillBread);
    }

    public void EnableButton() => _button.SetActive(true);
    public void DisableButton() => _button.SetActive(false);

    public void CoverButton()
    {
        foreach (ChillBreadTle chillBread in chillBreads)
        {
            if (chillBread.MyCurrentBakeTle())
            {
                InGameUIManager.Instance.ShowFloatingText("아직 미완성인 음식이 있습니다.", Color.red);
                return;
            }
        }
        StageTrigger.Instance.CoverPut(true);

        _cover.gameObject.SetActive(true);
        DisableButton();
        cancelPressed = false;

        cookCoroutine = StartCoroutine(CookRoutine());
    }

    public void CancelButtonPressed()
    {
        cancelPressed = true;
        StageTrigger.Instance.CoverPut(false);
    }
    private bool isOneTime;
    private IEnumerator CookRoutine()
    {
        float timer = 0f;
        bool smokeActivated = false;
        bool shakeActivated = false;     // ✅ DOShake 1회만 실행
        float totalTime = 15f;           // 전체 요리시간

        while (timer < totalTime)
        {
            // ✅ 게이지바 채우기 (1→0)
            if (cookGaugeImg != null)
                cookGaugeImg.fillAmount = 1f - (timer / totalTime);

            // ✅ 색상 변화
            if (cookGaugeImg != null)
            {
                if (timer <= 3f)
                {
                    cookGaugeImg.color = Color.green;
                }
                else if (timer <= 10f)
                {
                    // 3~10초: 초록→빨강
                    float t = Mathf.Clamp01((timer - 3f) / (10f - 3f));
                    cookGaugeImg.color = Color.Lerp(Color.green, Color.red, t);
                }
                else
                {
                    // 10~15초: 빨강→검정
                    float t = Mathf.Clamp01((timer - 10f) / (15f - 10f));
                    cookGaugeImg.color = Color.Lerp(Color.red, Color.black, t);
                }
            }

            // 흔들기 (기본 살짝)
            if (_cover != null)
                _cover.anchoredPosition = originalCoverPos + new Vector2(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount)
                );

            timer += shakeSpeed;
            yield return new WaitForSeconds(shakeSpeed);

            // 3초 이내 취소
            if (cancelPressed && timer < 3f)
            {
                if (cookGaugeImg != null)
                {
                    cookGaugeImg.fillAmount = 1f;
                    cookGaugeImg.color = Color.green;
                }
                ResetCover();
                InGameUIManager.Instance.ShowFloatingText("요리 취소됨", Color.yellow);
                yield break;
            }

            if (timer >= 3f && !isOneTime)
            {
                isOneTime = true;
                _cancelButton.SetActive(true);
            }

            // 3초 이후 취소 시 요리 완료
            if (cancelPressed && timer >= 3f)
            {
                if (cookGaugeImg != null)
                {
                    cookGaugeImg.fillAmount = 0f;
                    cookGaugeImg.color = Color.red;
                }
                ResetCover();
                InGameUIManager.Instance.ShowFloatingText("요리 완료!", Color.green);
                CompleteCooking();
                yield break;
            }

            // 10초 이후 연기 + 쌈뽕 DOShake
            if (!smokeActivated && timer >= 10f)
            {
                if (smokeObject != null) smokeObject.SetActive(true);
                StageTrigger.Instance.CoverPut(true, true);
                smokeActivated = true;
            }
            if (!shakeActivated && timer >= 10f)
            {
                shakeActivated = true;
                // ✅ DOShake 로 쌈뽕하게 흔들기
                _cover.DOShakeAnchorPos(
                    duration: 1.5f,
                    strength: new Vector2(30f, 30f),
                    vibrato: 25,
                    randomness: 90f,
                    snapping: false,
                    fadeOut: true
                );
            }
        }

        if (cookGaugeImg != null)
        {
            cookGaugeImg.fillAmount = 0f;
            cookGaugeImg.color = Color.black;   // 최종 까맣게
        }

        ResetCover();
        StageTrigger.Instance.CoverPut(false);
        StageTrigger.Instance.Burn();
        Burned();
    }

    private void ResetCover()
    {
        DisableButton();
        _cover.anchoredPosition = originalCoverPos;
        _cover.gameObject.SetActive(false);
        _cancelButton.SetActive(false);
        if (smokeObject != null) smokeObject.SetActive(false);
        _button.SetActive(false);
    }

    private void CompleteCooking()
    {
        foreach (ChillBreadTle chillBreadTle in chillBreads)
            chillBreadTle.ResetPlate();
        foreach (ChillBreadTle chillBread in InChillBreads)
        {
            StageTrigger.Instance.BreadPut(chillBread.putIdx, MiniBreadState.None);
            FinishBread bread = Instantiate(finishBread, chillBread.transform);
            bread.transform.SetParent(GetComponentsInChildren<Yeeee>().FirstOrDefault().GetComponent<RectTransform>());
            bread.Initialized(BreadType.Cook, chillBread.CurrentPipping);
        }
        InChillBreads.Clear();
        isOneTime = false;
    }

    private void Burned()
    {
        Debug.Log("음식이 탔습니다!");
        InGameUIManager.Instance.ShowFloatingText("음식이 탔습니다!", Color.red);

        foreach (ChillBreadTle chillBreadTle in chillBreads)
            chillBreadTle.ResetPlate();
        foreach (ChillBreadTle chillBread in InChillBreads)
        {
            FinishBread bread = Instantiate(finishBread, chillBread.transform);
            bread.transform.SetParent(GetComponentsInChildren<Yeeee>().FirstOrDefault().GetComponent<RectTransform>());
            bread.Initialized(BreadType.Burn, chillBread.CurrentPipping);
            StageTrigger.Instance.BreadPut(chillBread.putIdx, MiniBreadState.Burn);
        }
        InChillBreads.Clear();
    }
}
