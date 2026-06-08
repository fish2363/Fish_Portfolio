using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public interface ICameraService
{
    CinemachineCamera CurrentCamera { get; }
    void ChangeCamera(CinemachineCamera newCamera);
    void SwapTracking(Transform target);
    void Pan(float distance, float panTime, PanDirection direction, bool isRewindToStart);
}

public class CameraManager : MonoBehaviour, ICameraService
{
    [SerializeField] private int activeCameraPriority = 15;
    [SerializeField] private int disableCameraPriority = 5;
    [SerializeField] private CinemachineCamera currentCamera;

    private readonly Dictionary<PanDirection, Vector2> _panDirections = new()
    {
        { PanDirection.Up, Vector2.up },
        { PanDirection.Down, Vector2.down },
        { PanDirection.Left, Vector2.left },
        { PanDirection.Right, Vector2.right }
    };

    private CinemachinePositionComposer _positionComposer;
    private Vector2 _originalTrackPosition;
    private Tween _panningTween;

    public CinemachineCamera CurrentCamera => currentCamera;

    private void Awake()
    {
        Bus<SwapCameraEvent>.OnEvent += HandleSwapCamera;
        Bus<SwapTrackingEvent>.OnEvent += HandleSwapTracking;
        Bus<PanEvent>.OnEvent += HandleCameraPanning;
        RefreshComposer();
    }

    private void OnDestroy()
    {
        Bus<SwapCameraEvent>.OnEvent -= HandleSwapCamera;
        Bus<SwapTrackingEvent>.OnEvent -= HandleSwapTracking;
        Bus<PanEvent>.OnEvent -= HandleCameraPanning;
        KillTweenIfActive();
    }

    public void ChangeCamera(CinemachineCamera newCamera)
    {
        if (newCamera == null || currentCamera == newCamera)
            return;

        Transform followTarget = currentCamera != null ? currentCamera.Follow : null;

        if (currentCamera != null)
            currentCamera.Priority = disableCameraPriority;

        currentCamera = newCamera;
        currentCamera.Priority = activeCameraPriority;

        if (followTarget != null)
            currentCamera.Follow = followTarget;

        RefreshComposer();
    }

    public void SwapTracking(Transform target)
    {
        if (target == null || currentCamera == null)
            return;

        currentCamera.Target.TrackingTarget = target;
    }

    public void Pan(float distance, float panTime, PanDirection direction, bool isRewindToStart)
    {
        if (_positionComposer == null)
            return;

        Vector3 endPosition = isRewindToStart
            ? _originalTrackPosition
            : _panDirections[direction] * distance + (Vector2)_originalTrackPosition;

        KillTweenIfActive();
        _panningTween = DOTween.To(
            () => _positionComposer.TargetOffset,
            value => _positionComposer.TargetOffset = value,
            endPosition,
            panTime);
    }

    private void RefreshComposer()
    {
        if (currentCamera == null)
            return;

        _positionComposer = currentCamera.GetComponent<CinemachinePositionComposer>();
        if (_positionComposer != null)
            _originalTrackPosition = _positionComposer.TargetOffset;
    }

    private void HandleSwapTracking(SwapTrackingEvent evt) => SwapTracking(evt.target);

    private void HandleSwapCamera(SwapCameraEvent swapEvt)
    {
        if (currentCamera == swapEvt.leftCamera && swapEvt.moveDirection.x > 0)
            ChangeCamera(swapEvt.rightCamera);
        else if (currentCamera == swapEvt.rightCamera && swapEvt.moveDirection.x < 0)
            ChangeCamera(swapEvt.leftCamera);
    }

    private void HandleCameraPanning(PanEvent evt)
        => Pan(evt.distance, evt.panTime, evt.direction, evt.isRewindToStart);

    private void KillTweenIfActive()
    {
        if (_panningTween != null && _panningTween.IsActive())
            _panningTween.Kill();
    }
}