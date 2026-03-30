using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDefault : MonoBehaviour
{
    [SerializeField] private RectTransform timeRT;
    [SerializeField] private TextMeshProUGUI timeTxt;
    [SerializeField] private TextMeshProUGUI scoreTxt;

    int remain = 0;
    int score = 0;
    int wave = 0;

    readonly float timeInterval = 10f;
    bool isWaveOver = false;

    public void Update()
    {
        int curTime = Mathf.CeilToInt(GameManager.Instance.curTime);
        if (remain != curTime)
        {
            remain = curTime;
            int mm = remain / 60;
            int ss = remain % 60;
            timeTxt.text = $"{mm:00}:{ss:00}";

            if (curTime <= GameManager.Instance.waveTime[wave] + timeInterval)
            {
                StartCoroutine(WaveTime());
                isWaveOver = wave % 2 != 0;
                wave++;
            }
        }
        Debug.Log(GameManager.Instance.CurScore);
        if (score != GameManager.Instance.CurScore)
        {
            score = GameManager.Instance.CurScore;
            scoreTxt.text = $"{score}";
        }
    }

    public void RecipeBtn()
    {
        UIManager.Show<UIPopupRecipe>();
    }

    private IEnumerator WaveTime()
    {
        yield return TimeFlash();
        
        timeTxt.color = (!isWaveOver) ? Color.red : Color.black;
        if (!isWaveOver)
        {
            StartCoroutine(TimeShake());
        }
    }

    private IEnumerator TimeFlash()
    {
        float tempTime = 0;
        bool isRed = false;
        while (tempTime < timeInterval)
        {
            tempTime += 0.2f;
            timeTxt.color = isRed ? Color.black : Color.red;
            isRed = !isRed;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator TimeShake()
    {
        yield return new WaitForSeconds(0.5f);
        var seq = UIShakeUtil.ShakeUI(timeRT);
        yield return new WaitUntil(() => isWaveOver);
        yield return new WaitForSeconds(timeInterval);
        seq.Kill();
    }
}
