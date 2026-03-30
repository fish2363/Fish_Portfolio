using System;
using System.Globalization; // ← 추가
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankSlot : MonoBehaviour
{
    [SerializeField] TMP_Text rankTxt;
    [SerializeField] TMP_Text nameTxt;
    [SerializeField] TMP_Text scoreTxt;
    [SerializeField] Button button;

    public void Init(int rank, string name, int score, DateTime endTimeUtc, Action onClick)
    {
        if (!button) button = GetComponent<Button>();
        if (rankTxt) rankTxt.text = rank.ToString();
        if (nameTxt) nameTxt.text = string.IsNullOrWhiteSpace(name) ? "플레이어" : name;
        if (scoreTxt) scoreTxt.text = score.ToString("N0", CultureInfo.InvariantCulture); // 3자리마다 콤마

        button.onClick.RemoveAllListeners();
        if (onClick != null) button.onClick.AddListener(() => onClick());
    }
}
