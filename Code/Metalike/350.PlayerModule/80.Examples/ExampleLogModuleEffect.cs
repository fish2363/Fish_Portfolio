using System;
using UnityEngine;

[Serializable]
[ModuleDisplayName("Example/Log Message", "Prints a message when the trigger runs.")]
public class ExampleLogModuleEffectDef : IModuleEffectDef
{
    [TextArea]
    public string message = "Example module triggered.";

    public bool includeOwnerName = true;

    public IModuleEffect CreateEffect()
    {
        return new ExampleLogModuleEffect(this);
    }
}

public class ExampleLogModuleEffect : IExecutableEffect
{
    private readonly ExampleLogModuleEffectDef _def;
    private Entity _owner;

    public ExampleLogModuleEffect(ExampleLogModuleEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        _owner = owner;
    }

    public void OnUnequip()
    {
        _owner = null;
    }

    public void Execute(EffectContext ctx)
    {
        string ownerName = _owner != null ? _owner.name : "Unknown";
        string suffix = _def.includeOwnerName ? $" (Owner: {ownerName})" : string.Empty;
        Debug.Log($"[Module Example] {_def.message}{suffix}");
    }
}
