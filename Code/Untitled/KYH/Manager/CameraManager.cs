using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera currentCamera;
    [SerializeField] private int activeCameraPriority = 15;
    [SerializeField] private int disableCameraPriority = 10;
    [SerializeField] private GameEventChannelSO cameraChannel;

    private Vector2 _originalTrackPosition; //�̺κ��� ���� �����մϴ�.
    private CinemachinePositionComposer _positionComposer;

    private Dictionary<PanDirection, Vector2> _panDirections;
    private Tween _panningTween;

    private void Awake()
    {
        _panDirections = new Dictionary<PanDirection, Vector2>
            {
                { PanDirection.Up, Vector2.up },
                { PanDirection.Down, Vector2.down },
                { PanDirection.Left, Vector2.left },
                { PanDirection.Right, Vector2.right },
            };

        cameraChannel.AddListener<PanEvent>(HandleCameraPanning);
        cameraChannel.AddListener<SwapCameraEvent>(HandleSwapCamera);
        cameraChannel.AddListener<PerlinShake>(HandleShakeCamera);
        currentCamera = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None)
                        .FirstOrDefault(cam => cam.Priority == activeCameraPriority);

        Debug.Assert(currentCamera != null, $"Check camera priority, there is no active camera");
        ChangeCamera(currentCamera,true);
    }

    private void HandleShakeCamera(PerlinShake shakeEvt)
    {
        //CinemachineBasicMultiChannelPerlin 
            //DOVirtual.DelayedCall(shakeEvt.second, () => impulseSource.GenerateImpulseWithVelocity(Vector3.zero));
    }

    private void OnDestroy()
    {
        cameraChannel.RemoveListener<PanEvent>(HandleCameraPanning);
        cameraChannel.RemoveListener<PerlinShake>(HandleShakeCamera);
        cameraChannel.RemoveListener<SwapCameraEvent>(HandleSwapCamera);
        KillTweenIfActive();
    }

    public void ChangeCamera(CinemachineCamera newCamera,bool isBatton)
    {
        currentCamera.Priority = disableCameraPriority; //���� ī�޶� ���ְ�
        Transform followTarget = currentCamera.Follow;
        if (followTarget is null)
            followTarget = FindAnyObjectByType<Player>().gameObject.transform;

        currentCamera = newCamera;
        currentCamera.Priority = activeCameraPriority;

        if(isBatton)
            currentCamera.Follow = followTarget;

        _positionComposer = currentCamera.GetComponent<CinemachinePositionComposer>();
        if (_positionComposer != null)
            _originalTrackPosition = _positionComposer.TargetOffset;
    }

    private void HandleSwapCamera(SwapCameraEvent swapEvt)
    {
        if(swapEvt.isForceSwap)
            ChangeCamera(swapEvt.rightCamera, swapEvt.isBattonFollow);
        else if (currentCamera == swapEvt.leftCamera && swapEvt.moveDirection.x > 0)
            ChangeCamera(swapEvt.rightCamera,swapEvt.isBattonFollow);
        else if (currentCamera == swapEvt.rightCamera && swapEvt.moveDirection.x < 0)
            ChangeCamera(swapEvt.leftCamera, swapEvt.isBattonFollow);
    }

    private void HandleCameraPanning(PanEvent evt)
    {
        Vector3 endPosition = evt.isRewindToStart ?
            _originalTrackPosition : _panDirections[evt.direction] * evt.distance + _originalTrackPosition;
        //����ġ�� �����ε� �����ִ� �̺�Ʈ�� ����ġ�� ������, �׷��� �ʴٸ� ������ �̵������ְ�

        KillTweenIfActive();
        _panningTween = DOTween.To(
            () => _positionComposer.TargetOffset,
            value => _positionComposer.TargetOffset = value,
            endPosition, evt.panTime);

    }
    public static  Vector3 GetMousePointerPosition()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, screenWidth), Mathf.Clamp(Input.mousePosition.y, 0, screenHeight)));
        return mousePosition;
    }
    private void KillTweenIfActive()
    {
        if (_panningTween != null && _panningTween.IsActive())
            _panningTween.Kill();
    }
}