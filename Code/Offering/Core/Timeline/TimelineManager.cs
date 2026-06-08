using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

public interface ITimelineService : IGameService
{
    TimelineQueueInfo CurrentPlayTimeline { get; }
    bool IsPlay { get; }
    event Action AllTimelineEnd;
    void ShowTimeline(PlayableDirector director, TimelineType type, bool skipOnStart = false, Action onComplete = null);
}

public class TimelineQueueInfo : IPriorityComparer<TimelineQueueInfo>
{
    public readonly PlayableDirector Director;
    public readonly TimelineType AssetType;
    public readonly bool SkipOnStart;
    public readonly Action Callback;

    public TimelineQueueInfo(PlayableDirector director, TimelineType assetType, bool skipOnStart, Action callback)
    {
        Director = director;
        AssetType = assetType;
        SkipOnStart = skipOnStart;
        Callback = callback;
    }

    public bool Compare(TimelineQueueInfo other)
    {
        return !SkipOnStart && other.SkipOnStart;
    }
}

public class TimelineManager : MonoBehaviour, ITimelineService
{
    [SerializeField] private TimelineClipSetList timelineClipSetList;
    [SerializeField] private float speedMultiply = 4f;

    private readonly PriorityQueue<TimelineQueueInfo> _timelineQueue = new();
    private readonly CancellationTokenSource _playCts = new();
    private PlayableDirector _currentDirector;
    private TimelineQueueInfo _currentPlayQueueInfo;
    private bool _isPlayingQueue;

    public TimelineQueueInfo CurrentPlayTimeline => _currentPlayQueueInfo;
    public event Action AllTimelineEnd;
    public bool IsInitialized { get; private set; }

    public bool IsPlay
    {
        get
        {
            if (_currentDirector == null || !_currentDirector.playableGraph.IsValid())
                return false;

            return _currentDirector.state == PlayState.Playing;
        }
    }

    public UniTask InitAsync()
    {
        IsInitialized = true;
        return UniTask.CompletedTask;
    }

    public void Release()
    {
        _playCts.Cancel();
        _currentDirector = null;
        _currentPlayQueueInfo = null;
        _isPlayingQueue = false;
        IsInitialized = false;
    }

    public void ShowTimeline(PlayableDirector director, TimelineType type, bool skipOnStart = false, Action onComplete = null)
    {
        if (director == null)
        {
            Debug.LogWarning("[TimelineManager] PlayableDirector is missing.");
            return;
        }

        TimelineClipSet clipSet = timelineClipSetList != null ? timelineClipSetList.GetClip(type) : null;
        if (clipSet?.asset != null)
            director.playableAsset = clipSet.asset;

        _timelineQueue.Enqueue(new TimelineQueueInfo(director, type, skipOnStart, onComplete));

        if (!_isPlayingQueue)
            PlayQueueAsync(_playCts.Token).Forget();
    }

    public void SpeedUpCurrentTimeline()
    {
        SetDirectorSpeed(speedMultiply);
    }

    public void CancelSpeedUpCurrentTimeline()
    {
        SetDirectorSpeed(1f);
    }

    private void SetDirectorSpeed(float speed)
    {
        if (_currentDirector != null && _currentDirector.playableGraph.IsValid())
            _currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
    }

    private async UniTaskVoid PlayQueueAsync(CancellationToken cancellationToken)
    {
        _isPlayingQueue = true;

        while (_timelineQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _currentPlayQueueInfo = _timelineQueue.Dequeue();
            _currentDirector = _currentPlayQueueInfo.Director;
            _currentDirector.Play();

            await UniTask.WaitUntil(() => !IsPlay, cancellationToken: cancellationToken);

            _currentDirector.Stop();
            _currentPlayQueueInfo.Callback?.Invoke();

            _currentDirector = null;
            _currentPlayQueueInfo = null;
        }

        _isPlayingQueue = false;
        AllTimelineEnd?.Invoke();
    }
}
