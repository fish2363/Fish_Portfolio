using System.Collections.Generic;
using UnityEngine;

public enum Phase { Play, Rewind,Clear, End }

public static class InGameEvent
{
    public static ComeUpCardUIEvent GameStartEvent = new();
    public static ComeDownCardUIEvent GameEndEvent = new();
    public static CinemaUIEvent CinemaUIEvent = new();
    public static ClearGameEvent ClearGameEvent = new();
    public static RunSequenceEvent RunSequenceEvent = new();
    public static SequencePhaseEvent SequencePhaseEvent = new();
    public static ActorExecutingEvent ActorExecutingEvent = new();
    public static TaskCompleteEvent TaskCompleteEvent = new();
}

public class RunSequenceEvent : GameEvent
{
    public List<ESequenceCharacter> SelectedOrder;

    public RunSequenceEvent Initialize(List<ESequenceCharacter> characters)
    {
        SelectedOrder = characters;
        return this;
    }
}

public class SequencePhaseEvent : GameEvent
{
    public Phase CurrentPhase;

    public SequencePhaseEvent Initialize(Phase currentPhase)
    {
        CurrentPhase = currentPhase;
        return this;
    }
}

public class ActorExecutingEvent : GameEvent
{
    public int ActorIndex;

    public ActorExecutingEvent Initialize(int index)
    {
        ActorIndex = index;
        return this;
    }
}

public class ClearGameEvent : GameEvent
{
    
}

public class ComeUpCardUIEvent : GameEvent
{
    
}
public class TaskCompleteEvent : GameEvent
{
    public Task TaskType;
    public Color TargetColor;

    public TaskCompleteEvent Initialize(Task task, Color color)
    {
        TaskType = task;
        TargetColor = color;
        return this;
    }
}
public class ComeDownCardUIEvent : GameEvent
{
    
}

public class CinemaUIEvent : GameEvent
{
    public bool IsRunning;
    public CinemaUIEvent Initialize(bool isRunning)
    {
        IsRunning = isRunning;
        return this;
    }
}
