using Unity.Cinemachine;
using UnityEngine;
public enum PanDirection
{
    Up, Down, Left, Right
}
public static class CameraEvents
{
    public static PanEvent PanEvent = new();
    public static SwapCameraEvent SwapCameraEvent = new();
    public static PerlinShake CameraShakeEvent = new();
}

public class PanEvent : GameEvent
{
    public float distance;
    public float panTime;
    public PanDirection direction;
    public bool isRewindToStart;
}

public class SwapCameraEvent : GameEvent
{
    public CinemachineCamera leftCamera;
    public CinemachineCamera rightCamera;
    public bool isBattonFollow;
    public Vector2 moveDirection;
    public bool isForceSwap;
}

public class PerlinShake : GameEvent
{
    public float intensity;
    public float second;
}
