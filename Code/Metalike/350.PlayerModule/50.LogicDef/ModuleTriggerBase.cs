using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ModuleTriggerDef : IModuleLogicDef
{
    [SerializeReference]
    public List<IModuleEffectDef> effectDefs = new();

    public abstract IModuleLogic CreateLogic();
}

public abstract class ModuleTriggerBase<TDef> : IModuleLogic, IModuleEffectContainer
    where TDef : ModuleTriggerDef
{
    protected readonly TDef _def;
    protected Entity _owner;
    protected readonly List<IModuleEffect> _effects = new();

    protected ModuleTriggerBase(TDef def)
    {
        _def = def;
    }

    public virtual void OnEquip(Entity owner)
    {
        _owner = owner;
        _effects.Clear();

        for (int i = 0; i < _def.effectDefs.Count; i++)
        {
            IModuleEffectDef effectDef = _def.effectDefs[i];
            if (effectDef == null)
                continue;

            IModuleEffect effect = effectDef.CreateEffect();
            if (effect == null)
                continue;
            effect.OnEquip(owner);
            _effects.Add(effect);
        }
    }

    public virtual void OnUnequip()
    {
        for (int i = 0; i < _effects.Count; i++)
            _effects[i].OnUnequip();

        _effects.Clear();
        _owner = null;
    }

    public void CollectEffects(List<IModuleEffect> results)
    {
        results.AddRange(_effects);
    }

    protected void ExecuteAll(EffectContext ctx)
    {
        for (int i = 0; i < _effects.Count; i++)
        {
            if (_effects[i] is IExecutableEffect executable)
                executable.Execute(ctx);
        }
    }
}
