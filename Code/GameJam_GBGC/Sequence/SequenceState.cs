using System;
using System.Collections.Generic;

public class SequenceState
{
    public bool IsTropical { get; set; }
    public bool IsIceCreamHeld { get; set; }
    public bool IsTutorial { get; set; }


    public List<ESequenceCharacter> Order { get; private set; }
    private Dictionary<ESequenceCharacter, int> IndexByChar;


    public HashSet<string> Flags { get; private set; } = new HashSet<string>();


    private Action onClearCallback;


    public SequenceState(List<ESequenceCharacter> order, Action onClear)
    {
        Order = order;
        onClearCallback = onClear;

        IndexByChar = new Dictionary<ESequenceCharacter, int>();
        for (int i = 0; i < order.Count; i++)
        {
            IndexByChar[order[i]] = i;
        }

        IsTropical = false;
        IsIceCreamHeld = false;
    }

    public int GetIndex(ESequenceCharacter ch)
    {
        return IndexByChar.TryGetValue(ch, out int idx) ? idx : -1;
    }

    private Dictionary<ESequenceCharacter, SequenceActorBase> actorDirectory;

    public void RegisterActors(Dictionary<ESequenceCharacter, SequenceActorBase> lookup)
    {
        actorDirectory = lookup;
    }

    public bool TryGetActor<T>(ESequenceCharacter id, out T actor) where T : SequenceActorBase
    {
        if (actorDirectory != null && actorDirectory.TryGetValue(id, out var baseActor))
        {
            actor = baseActor as T;
            return actor != null;
        }
        actor = null;
        return false;
    }

    public bool HasFlag(string flag) => Flags.Contains(flag);
    public void SetFlag(string flag) => Flags.Add(flag);
    public void RemoveFlag(string flag) => Flags.Remove(flag);

    public void TriggerClear() => onClearCallback?.Invoke();
}
