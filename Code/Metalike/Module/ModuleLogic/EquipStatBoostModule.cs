using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

[Serializable]
public class EquipStatBoostModuleDef : IModuleLogicDef
{
    [Range(0f, 1f)] public float cooldownReducePercent = 0.2f;
    public PoolItemSO particlePrefab;
    public StatSO statSO;

    public IModuleLogic CreateLogic() => new EquipStatBoostModule(this);
}

public class EquipStatBoostModule : IModuleLogic
{
    private readonly EquipStatBoostModuleDef _def;

    private Entity _owner;
    private EntityStatCompo _statCompo;
    private ModuleController _controller;
    private VisualContainer _visualContainer;

    private PoolingEffect _particleInstance;

    public EquipStatBoostModule(EquipStatBoostModuleDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _statCompo = owner.GetCompo<EntityStatCompo>();
        _controller = owner.GetCompo<ModuleController>();
        _visualContainer = owner.GetCompo<VisualContainer>();

        float delta = _statCompo.GetStat(_def.statSO).Value * _def.cooldownReducePercent;

        _statCompo.AddModifier(_def.statSO,this, delta);

        OnEffects();
    }

    private void OnEffects()
    {
        _particleInstance = _controller.poolManager.Pop<PoolingEffect>(_def.particlePrefab);
        _particleInstance.PlayVFX(_owner.transform.position, Quaternion.identity);
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_particleInstance == null || _visualContainer == null) return;
        
        _particleInstance.transform.position = _visualContainer.CurrentVisual.transform.position;
    }

    public void OnUnequip()
    {
        _statCompo.RemoveModifier(_def.statSO, this);
        _particleInstance.Push();
    }
}
