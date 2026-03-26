using System.Collections.Generic;
using UnityEngine;

public enum SynergyKey
{
    ProjectileItemOverride,

}

public readonly struct SynergyToken
{
    public readonly SynergyKey key;
    public readonly int priority;
    public readonly Object payload;

    public SynergyToken(SynergyKey key, int priority, Object payload)
    {
        this.key = key;
        this.priority = priority;
        this.payload = payload;
    }
}

public interface ISynergyProvider
{
    void CollectTokens(List<SynergyToken> tokens);
}

public interface ISynergyReceiver
{
    void ResetSynergy();
    void ApplyToken(SynergyToken token);
}