using System.Collections.Generic;
using UnityEngine;

public readonly struct SynergyToken
{
    public readonly SynergyKeySO key;
    public readonly int priority;
    public readonly Object payload;

    public SynergyToken(SynergyKeySO key, int priority, Object payload)
    {
        this.key = key;
        this.priority = priority;
        this.payload = payload;
    }
}

public interface ISynergyProvider : IModuleHook
{
    void CollectTokens(List<SynergyToken> tokens);
}

public interface ISynergyReceiver : IModuleHook
{
    void ResetSynergy();
    void ApplyToken(SynergyToken token);
}
