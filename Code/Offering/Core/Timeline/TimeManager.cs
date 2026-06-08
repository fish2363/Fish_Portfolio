using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public enum StopChannel
{
    UI = 0,    // 최상위
    SelectMenu = 1,
    HIT = 2,   // 중간
    ELSE = 10, // 낮음
}

public class TimeManager : MonoSingleton<TimeManager>
{
    private class ChannelState
    {
        public StopInfoSO Info;
        public float RemainingTime;
    }

    [SerializeField] private StopInfoSO _stopInfoSO;
    private Dictionary<StopChannel, ChannelState> _activeChannels = new();
    private Tween _timeScaleTween;
    private StopChannel? _currentExecutingChannel = null;

    // 물리 업데이트 기본값 (보통 0.02s)
    private const float DefaultFixedDeltaTime = 0.02f;

    private void Update()
    {
        if(Keyboard.current.bKey.wasPressedThisFrame)
        {
            GenerateStop(_stopInfoSO);
        }

        if (_activeChannels.Count == 0) return;

        // 1. 우선순위가 높은 채널(Enum 값이 낮은 순) 추출
        var sortedKeys = _activeChannels.Keys.OrderBy(c => (int)c).ToList();
        StopChannel highestChannel = sortedKeys[0];

        // 2. 최상위 채널의 시간만 소모 (상위가 끝나야 하위가 실행됨)
        var state = _activeChannels[highestChannel];
        if (!state.Info.IsInfinite)
        {
            state.RemainingTime -= Time.unscaledDeltaTime;
            if (state.RemainingTime <= 0)
            {
                ReleaseStop(highestChannel);
                return;
            }
        }

        // 3. 최상위 채널이 변경되었다면 (상위 종료 혹은 더 높은 상위 발생) 효과 적용
        if (_currentExecutingChannel != highestChannel)
        {
            _currentExecutingChannel = highestChannel;
            ApplyTimeScale(state.Info.StopPower, state.Info.EaseType, 0.1f);
        }
    }

    public void GenerateStop(StopInfoSO newInfo)
    {
        // 채널별로 하나의 데이터만 유지 (새로 들어오면 갱신)
        if (_activeChannels.ContainsKey(newInfo.StopChannel))
        {
            _activeChannels[newInfo.StopChannel].Info = newInfo;
            _activeChannels[newInfo.StopChannel].RemainingTime = newInfo.Duration;
        }
        else
        {
            _activeChannels.Add(newInfo.StopChannel, new ChannelState
            {
                Info = newInfo,
                RemainingTime = newInfo.Duration
            });
        }

        UpdatePriority();
    }

    public void ReleaseStop(StopChannel channel)
    {
        if (_activeChannels.ContainsKey(channel))
        {
            _activeChannels.Remove(channel);
            UpdatePriority();
        }
    }

    private void UpdatePriority()
    {
        if (_activeChannels.Count == 0)
        {
            if (_currentExecutingChannel != null)
            {
                _currentExecutingChannel = null;
                ApplyTimeScale(1f, Ease.OutQuad, 0.15f); // 최종 복귀
            }
            return;
        }

        var highest = _activeChannels.Keys.OrderBy(c => (int)c).First();
        if (_currentExecutingChannel != highest)
        {
            _currentExecutingChannel = highest;
            ApplyTimeScale(_activeChannels[highest].Info.StopPower, _activeChannels[highest].Info.EaseType, 0.1f);
        }
    }

    private void ApplyTimeScale(float targetPower, Ease ease, float duration)
    {
        _timeScaleTween?.Kill();

        _timeScaleTween = DOTween.To(() => Time.timeScale, x =>
        {
            Time.timeScale = x;

            // [핵심] 지터링 방지: 물리 연산 주기를 타임스케일에 맞춰 강제로 조정
            // 타임스케일이 0이 될 때를 대비해 아주 작은 값(0.0001f)으로 클램핑
            Time.fixedDeltaTime = DefaultFixedDeltaTime * Mathf.Max(x, 0.0001f);
        }, targetPower, duration)
        .SetUpdate(true) // 타임스케일이 0이어도 Tween이 돌아가게 함
        .SetEase(ease);
    }
}