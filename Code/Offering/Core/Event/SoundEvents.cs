using UnityEngine;


public struct PlaySFXEvent : IEvent
{
    public Vector3 position;
    public SoundSO soundClip;

    public PlaySFXEvent Initializer(Vector3 position, SoundSO soundClip)
    {
        this.position = position;
        this.soundClip = soundClip;
        return this;
    }
}

public struct PlayBGMEvent : IEvent
{
    public SoundSO bgmClip;

    public PlayBGMEvent Initializer(SoundSO bgmClip)
    {
        this.bgmClip = bgmClip;
        return this;
    }
}

public struct StopBGMEvent : IEvent
{
}

public struct PlayLongSFXEvent : IEvent
{
    public Vector3 position;
    public SoundSO soundClip;
    public int idxNumber;

    public PlayLongSFXEvent Initializer(Vector3 position, SoundSO soundClip, int idxNumber)
    {
        this.position = position;
        this.soundClip = soundClip;
        this.idxNumber = idxNumber;
        return this;
    }
}

public struct StopLongSFXEvent : IEvent
{
    public int idxNumber;

    public StopLongSFXEvent Initializer(int idxNumber)
    {
        this.idxNumber = idxNumber;
        return this;
    }
}
