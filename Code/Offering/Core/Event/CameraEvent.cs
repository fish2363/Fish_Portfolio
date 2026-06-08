using Unity.Cinemachine;
using UnityEngine;

public enum PanDirection
{
    Up, Down, Left, Right
}

public struct PanEvent : IEvent
{
    public float distance;
    public float panTime;
    public PanDirection direction;
    public bool isRewindToStart;

    public PanEvent Initialize(float distance, float panTime, PanDirection direction, bool isRewindToStart)
    {
        this.distance = distance;
        this.panTime = panTime;
        this.direction = direction;
        this.isRewindToStart = isRewindToStart;
        return this;
    }
}                             

public struct SwapCameraEvent : IEvent
{
    public CinemachineCamera leftCamera;
    public CinemachineCamera rightCamera;
    public Vector2 moveDirection;

    public SwapCameraEvent Initialize(CinemachineCamera leftCamera, CinemachineCamera rightCamera, Vector2 moveDirection)
    {
        this.leftCamera = leftCamera;
        this.rightCamera = rightCamera;
        this.moveDirection = moveDirection;
        return this;
    }
}

public struct SwapTrackingEvent : IEvent
{
    public Transform target;

    public SwapTrackingEvent Initialize(Transform target)
    {
        this.target = target;
        return this;
    }
}