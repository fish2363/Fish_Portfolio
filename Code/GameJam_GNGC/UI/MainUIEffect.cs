using DG.Tweening;
using TMPro;
using UnityEngine;

public class MainUIEffect : MonoBehaviour
{
    private bool isShaking;
    private RectTransform rectTransform;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI HighText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ShakeLeftRightUI()
    {
        rectTransform.DOShakeRotation(0.3f, 1f, 10, 90f).OnComplete(() => isShaking = false);
    }
    public void ShakeUpDownUI()
    {
        rectTransform.DOShakeAnchorPos(
            0.3f,
            new Vector2(0, 10f), // Y축으로만 흔들림
            10,
            90f,
            false,
            true
        ).OnComplete(() => isShaking = false);
    }

    public void SetScore()
    {
        int levelNum = GameManager.Instance.CurrentScore;
        scoreText.text = $"{levelNum}";
        HighText.text = $"{PlayerPrefs.GetInt("stage", levelNum)}";

        if (levelNum > PlayerPrefs.GetInt("stage",0))
            PlayerPrefs.SetInt("stage", levelNum);
    }
}
