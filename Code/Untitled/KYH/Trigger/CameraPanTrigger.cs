using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[Serializable]
public class CameraPanEventContext
{
    public PanDirection panDirection;
    public float panDistance = 3f;
    public float panTime = 0.35f;

    [Tooltip("사라지는 시간,0초면 벗어나면 사라짐")]
    public int time;
}

public class CameraPanTrigger : TriggerObject
{
    [SerializeField] private CameraPanEventContext cameraEventContext;
    [SerializeField] private GameEventChannelSO cameraChannel;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SendPanEvent(false);
            if(cameraEventContext.time != 0)
            {
                StartSafeCoroutine("CameraPan",DestroyRoutine());
            }    
        }
    }
    public IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(cameraEventContext.time);
        SendPanEvent(true);
        gameObject.GetComponent<CameraPanTrigger>().enabled = false;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            SendPanEvent(true);
    }

    private void SendPanEvent(bool isRewind)
    {
        PanEvent evt = CameraEvents.PanEvent;
        evt.panTime = cameraEventContext.panTime;
        evt.distance = cameraEventContext.panDistance;
        evt.direction = cameraEventContext.panDirection;
        evt.isRewindToStart = isRewind;

        cameraChannel.RaiseEvent(evt);
    }
}
