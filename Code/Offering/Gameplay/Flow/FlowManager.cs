using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlowManager : MonoBehaviour
{
    [SerializeField, Header("테스트")] private List<Player> _players = new();
    [SerializeField, Header("테스트")] private List<Color> _colors = new();
    [SerializeField, Header("테스트")] private List<string> playerNames;

    [SerializeField] OffscreenArrowIndicator _arrowIndicator;
    [SerializeField] private CountdownTimerView _timer;
    [SerializeField] private MinimapView _minimap;
    [SerializeField] private int NetworkNum;
    [SerializeField] private SacrificeAltar sacrificeAltar;
    [SerializeField] private float sacrificeStartTime = 60f;
    [SerializeField] private float sacrificeDuration = 20f;
    [SerializeField] private float totalSeconds = 300f;  // 5분

    private float _gameStartTime;
    private bool _sacrificeStarted;
    private bool _sacrificeEnded;

    private void Start()
    {
#if false
        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].NetworkPlayerID = i.ToString();

            _arrowIndicator.AddTarget(_players[i].transform, _colors[i], playerNames[i]);
            _minimap.AddTarget(_players[i].transform, _colors[i]);

            if (i == NetworkNum)
            {
                _players[i].EnableLocalInput();

                Bus<SwapTrackingEvent>.Raise(
                    new SwapTrackingEvent().Initialize(_players[i].transform)
                );
            }
            else
            {
                _players[i].DisableLocalInput();
            }
        }

        _gameStartTime = Time.time;

        if (sacrificeAltar != null)
            sacrificeAltar.IsSacrificeTime = false;

        _timer.OnTimerEnded += HandleGameTimeOver;
        _timer.StartTimer(totalSeconds);
#endif
    }
    private void Update()
    {
#if false
        float elapsed = Time.time - _gameStartTime;

        if (!_sacrificeStarted && elapsed >= sacrificeStartTime)
        {
            _sacrificeStarted = true;

            if (sacrificeAltar != null)
                sacrificeAltar.IsSacrificeTime = true;

            Debug.Log("제물 타임 시작");
        }

        if (!_sacrificeEnded && elapsed >= sacrificeStartTime + sacrificeDuration)
        {
            _sacrificeEnded = true;

            if (sacrificeAltar != null)
                sacrificeAltar.IsSacrificeTime = false;

            Debug.Log("제물 타임 종료");
        }
#endif
    }

    private void OnDestroy()
    {
        if (_timer != null)
            _timer.OnTimerEnded -= HandleGameTimeOver;
    }

    private void HandleGameTimeOver()
    {
        Debug.Log("게임 시간 종료!");
        // 게임 종료 처리
    }
}


