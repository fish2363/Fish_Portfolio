using VContainer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private PoolItemSO soundPlayer;
    [SerializeField] private SoundSO testSoundClip;
    [SerializeField] private bool isTestCode = true;

    private Dictionary<int, SoundPlayer> _longSoundDict;
    private SoundPlayer _bgmPlayer;
    private IPoolService _poolService;

    private void Awake()
    {
        _longSoundDict = new Dictionary<int, SoundPlayer>();
        Bus<PlaySFXEvent>.OnEvent+=HandlePlaySFX;
        Bus<PlayBGMEvent>.OnEvent += HandlePlayBGM;
        Bus<StopBGMEvent>.OnEvent += HandleStopBGM;
        Bus<PlayLongSFXEvent>.OnEvent+=HandlePlayLongSFX;
        Bus<StopLongSFXEvent>.OnEvent+=HandleStopLongSFX;
    }

    [Inject]
    public void Construct(IPoolService poolService)
    {
        _poolService = poolService;
    }

    private void OnDestroy()
    {
        Bus<PlaySFXEvent>.OnEvent -= HandlePlaySFX;
        Bus<PlayBGMEvent>.OnEvent -= HandlePlayBGM;
        Bus<StopBGMEvent>.OnEvent -= HandleStopBGM;
        Bus<PlayLongSFXEvent>.OnEvent -= HandlePlayLongSFX;
        Bus<StopLongSFXEvent>.OnEvent -= HandleStopLongSFX;
    }

    private void HandlePlayLongSFX(PlayLongSFXEvent evt)
    {
        if (_longSoundDict.TryGetValue(evt.idxNumber, out SoundPlayer player))
        {
            player.StopAndReturnToPool();
            _longSoundDict.Remove(evt.idxNumber);
        }

        SoundPlayer sfxPlayer = _poolService.Pop<SoundPlayer>(soundPlayer);
        sfxPlayer.transform.position = evt.position;
        sfxPlayer.PlaySound(evt.soundClip);

        _longSoundDict.Add(evt.idxNumber, sfxPlayer);
    }

    private void HandleStopLongSFX(StopLongSFXEvent evt)
    {
        if (_longSoundDict.TryGetValue(evt.idxNumber, out SoundPlayer player))
        {
            player.StopAndReturnToPool();
            _longSoundDict.Remove(evt.idxNumber);
        }
    }

    private void HandlePlaySFX(PlaySFXEvent evt)
    {
        SoundPlayer sfxPlayer = _poolService.Pop<SoundPlayer>(soundPlayer);
        sfxPlayer.transform.position = evt.position;
        sfxPlayer.PlaySound(evt.soundClip);
    }

    private void HandlePlayBGM(PlayBGMEvent evt)
    {
        _bgmPlayer = _poolService.Pop<SoundPlayer>(soundPlayer);
        _bgmPlayer.PlaySound(evt.bgmClip);
    }

    private void HandleStopBGM(StopBGMEvent evt)
    {
        _bgmPlayer?.StopAndReturnToPool();
    }
}
