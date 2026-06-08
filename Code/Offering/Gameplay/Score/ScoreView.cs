using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] scoreTexts;

    private readonly Dictionary<string, TextMeshProUGUI> scoreTextMap = new();
    private IGameNetworkService gameNetworkService;

    [Inject]
    public void Construct(IGameNetworkService gameNetworkService)
    {
        this.gameNetworkService = gameNetworkService;
        RefreshAll();
    }

    private void OnEnable()
    {
        Bus<ScoreChangedEvent>.OnEvent += OnScoreChanged;
        RefreshAll();
    }

    private void OnDisable()
    {
        Bus<ScoreChangedEvent>.OnEvent -= OnScoreChanged;
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (string.IsNullOrEmpty(evt.playerNames)) return;

        if (scoreTextMap.TryGetValue(evt.playerNames, out TextMeshProUGUI text))
        {
            text.text = evt.newScore.ToString();
        }
    }

    private void RefreshAll()
    {
        scoreTextMap.Clear();

        if (gameNetworkService == null || scoreTexts == null)
            return;

        int index = 0;
        foreach (string sessionId in gameNetworkService.SessionDataDic.Keys)
        {
            if (index >= scoreTexts.Length)
                break;

            TextMeshProUGUI text = scoreTexts[index];
            if (text != null)
            {
                text.text = "0";
                scoreTextMap[sessionId] = text;
            }

            index++;
        }
    }
}
