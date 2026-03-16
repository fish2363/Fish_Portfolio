using Unity.Cinemachine;
using UnityEngine;
using System;

[Serializable]
public class CameraSwapEventContext
{
    public CinemachineCamera leftCamera;
    public CinemachineCamera rightCamera;
    public bool isCameraFollowPlayer;
    public bool isForce;
    public bool isOneTime;
}

public class CameraSwapTrigger : TriggerObject
{
    [SerializeField] private CameraSwapEventContext cameraEventContext;
    [SerializeField] private GameEventChannelSO cameraChannel;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (cameraEventContext.leftCamera is null || cameraEventContext.rightCamera is null) return;

        if (other.CompareTag("Player"))
        {
            Vector2 exitDirection = (other.transform.position - transform.position).normalized;
            SwapCameraEvent swapEvt = CameraEvents.SwapCameraEvent;
            swapEvt.isForceSwap = cameraEventContext.isForce;
            swapEvt.leftCamera = cameraEventContext.leftCamera;
            swapEvt.rightCamera = cameraEventContext.rightCamera;
            swapEvt.moveDirection = exitDirection;
            swapEvt.isBattonFollow = cameraEventContext.isCameraFollowPlayer;

            cameraChannel.RaiseEvent(swapEvt);

            if (cameraEventContext.isOneTime)
                gameObject.SetActive(false);
        }
    }
}
