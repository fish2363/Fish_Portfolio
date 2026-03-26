using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SequenceDirectorController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera playCam;
    [SerializeField] private CinemachineCamera readyCam;

    [Header("Post Processing")]
    [SerializeField] private Volume volume;
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;

    [Header("Timeline (Clear 연출)")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayableAsset clearDirector;

    [Header("Events")]
    [SerializeField] private GameEventChannelSO inGameEvent;

    private const int ActivePriority = 2;
    private const int InactivePriority = 1;
    private Tween _ppTween;

    private void Awake()
    {
        volume.profile.TryGet(out filmGrain);
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out lensDistortion);
    }

    private void OnEnable()
    {
        inGameEvent.AddListener<SequencePhaseEvent>(OnSequencePhaseChanged);
    }

    private void OnDisable()
    {
        inGameEvent.RemoveListener<SequencePhaseEvent>(OnSequencePhaseChanged);
    }


    private void OnSequencePhaseChanged(SequencePhaseEvent evt)
    {
        switch (evt.CurrentPhase)
        {
            case Phase.Play:
                SetCameraFocus(true);
                SetRewindEffect(false);
                break;

            case Phase.Rewind:
                SetCameraFocus(true);
                SetRewindEffect(true);
                break;

            case Phase.End:
                SetCameraFocus(false);
                SetRewindEffect(false);
                break;

            case Phase.Clear:
                PlayClearCutscene();
                break;
        }
    }

    private void SetCameraFocus(bool isPlaying)
    {
        playCam.Priority = isPlaying ? ActivePriority : InactivePriority;
        readyCam.Priority = isPlaying ? InactivePriority : ActivePriority;
    }

    private void SetRewindEffect(bool isActive)
    {
        if (filmGrain != null) filmGrain.active = isActive;
        if (colorAdjustments != null) colorAdjustments.active = isActive;
    }

    public void TriggerLensDistortion()
    {
        if (lensDistortion == null) return;

        _ppTween?.Kill();
        lensDistortion.active = true;

        _ppTween = DOTween.To(
                () => lensDistortion.intensity.value,
                v => lensDistortion.intensity.Override(v),
                -1f,
                3f
            )
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                lensDistortion.active = false;
            });
    }

    private void PlayClearCutscene()
    {
        if (director != null && clearDirector != null)
        {
            director.playableAsset = clearDirector;
            director.Play();
        }
    }
}
