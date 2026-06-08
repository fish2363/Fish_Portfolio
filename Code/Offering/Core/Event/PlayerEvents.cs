using UnityEngine;


public struct PlayerDieEvent : IEvent
{
    public PlayerDieEvent Initializer()
    {
        return this;
    }
}

public struct DropHeadEvent : IEvent
{
    public string playerID;
    public DropHeadEvent Initializer(string  targetID)
    {
        playerID = targetID;
        return this;
    }
}