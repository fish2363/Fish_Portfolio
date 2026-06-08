using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IScoreService : IGameService
{
    public int GetScore(string playerNames);
    public string GetWinnerIndex();
}

public class ScoreService : IScoreService
{
    private Dictionary<string,int> _scores = new();
    private readonly IGameNetworkService gameNetworkService;

    public bool IsInitialized { get; private set; }


    public ScoreService(IGameNetworkService gameNetworkService)
    {
        this.gameNetworkService = gameNetworkService;
    }

    public UniTask InitAsync()
    {
        _scores.Clear();

        Bus<ScoreAddedEvent>.OnEvent += HandleScoreAdded;

        foreach (string name in gameNetworkService.SessionDataDic.Keys)
            _scores.Add(name,0);

        return UniTask.CompletedTask;
    }
    public void Release()
    {
        Bus<ScoreAddedEvent>.OnEvent -= HandleScoreAdded;
    }

    public int GetScore(string playerNames)
        => _scores.TryGetValue(playerNames, out int score) ? score : 0;

    private void HandleScoreAdded(ScoreAddedEvent evt)
    {
        if (string.IsNullOrEmpty(evt.playerNames)) return;

        if (!_scores.ContainsKey(evt.playerNames))
            _scores[evt.playerNames] = 0;

        _scores[evt.playerNames] += evt.amount;
        Bus<ScoreChangedEvent>.Raise(new ScoreChangedEvent(evt.playerNames, _scores[evt.playerNames]));
    }

    /// <summary>최고 점수 플레이어 인덱스 반환. 동점이면 낮은 인덱스.</summary>
    public string GetWinnerIndex()
    {
        int maxScore = _scores.Values.Max();
        string[] winners = _scores.Where(pair => pair.Value == maxScore)
                                 .Select(pair => pair.Key)
                                 .ToArray();

        return string.Join(", ", winners);
    }
}
public struct ScoreAddedEvent : IEvent
{
    public string playerNames;  // 0~3
    public int amount;       // 보통 1

    public ScoreAddedEvent(string playerNames, int amount = 1)
    {
        this.playerNames = playerNames;
        this.amount = amount;
    }
}
public struct ScoreChangedEvent : IEvent
{
    public string playerNames;
    public int newScore;

    public ScoreChangedEvent(string playerNames, int newScore)
    {
        this.playerNames = playerNames;
        this.newScore = newScore;
    }
}
