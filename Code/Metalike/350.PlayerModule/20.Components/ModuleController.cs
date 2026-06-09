using Core.EventBus;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.RunTime;
using Public.Core.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ModuleController : MonoBehaviour, IEntityComponent
{
    private Entity _owner;

    private readonly List<IModuleLogic> _logics = new();
    private readonly List<IModuleEffect> _effects = new();

    private readonly Dictionary<Type, List<object>> _featureMap = new();
    private static readonly Dictionary<Type, Type[]> _featureTypeCache = new();
    private readonly List<IUpdateModuleLogic> _updateLogics = new();

    public DamageCompo DamageCompo { get; private set; }
    [Inject]
    public PoolManagerMono PoolManager;
    public LayerMask WhatIsTarget;

    private PetComponent _petComponent;
    private SkillComponent _skillComponent;

    public void Initialize(Entity entity)
    {
        _owner = entity;
        DamageCompo = entity.GetCompo<DamageCompo>();
        _petComponent = entity.GetCompo<PetComponent>();
        _skillComponent = entity.GetCompo<SkillComponent>();

        Bus<ModuleEquipChangedEvent>.OnEvent += HandleModuleEquipChanged;
        Bus<WeaponAttackTriggeredEvent>.OnEvent += OnFistsAttack;
        Bus<HitDealtEvent>.OnEvent += OnHit;
        _skillComponent.OnSkillEvent += OnSkillCast;
    }
    private void OnDestroy()
    {
        Bus<HitDealtEvent>.OnEvent -= OnHit;
        _skillComponent.OnSkillEvent -= OnSkillCast;
        Bus<WeaponAttackTriggeredEvent>.OnEvent -= OnFistsAttack;
        Bus<ModuleEquipChangedEvent>.OnEvent -= HandleModuleEquipChanged;
    }
    private void HandleModuleEquipChanged(ModuleEquipChangedEvent evt)
    {
        UnequipAllModules();

        if (evt.equippedModules == null)
            return;

        for (int i = 0; i < evt.equippedModules.Count; i++)
            EquipModule(evt.equippedModules[i]);
    }
    public void EquipModule(ModuleSO module)
    {
        if (module == null)
            return;

        List<IModuleLogic> newLogics = new();
        module.FillLogics(newLogics);

        for (int i = 0; i < newLogics.Count; i++)
        {
            IModuleLogic logic = newLogics[i];
            if (logic == null)
                continue;

            logic.OnEquip(_owner);
            _logics.Add(logic);
        }

        RebuildRuntimeCaches();
        RefreshSynergy();
    }

    public void UnequipAllModules()
    {
        for (int i = 0; i < _logics.Count; i++)
            _logics[i].OnUnequip();

        _logics.Clear();
        _effects.Clear();
        _featureMap.Clear();
        _updateLogics.Clear();

        _petComponent?.RebuildModules(null);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < _updateLogics.Count; i++)
            _updateLogics[i].ModuleUpdate(deltaTime);
    }

    private void RebuildRuntimeCaches()
    {
        _effects.Clear();
        _featureMap.Clear();
        _updateLogics.Clear();

        for (int i = 0; i < _logics.Count; i++)
        {
            IModuleLogic logic = _logics[i];
            if (logic == null)
                continue;

            CacheRuntimeFeatures(logic);

            if (logic is IModuleEffectContainer effectContainer)
                effectContainer.CollectEffects(_effects);
        }

        for (int i = 0; i < _effects.Count; i++)
        {
            IModuleEffect effect = _effects[i];
            if (effect == null)
                continue;

            CacheRuntimeFeatures(effect);
        }

        RebuildUpdateLogics();
        RebuildPetModules();
    }
    private void RebuildUpdateLogics()
    {
        _updateLogics.Clear();

        if (!_featureMap.TryGetValue(typeof(IUpdateModuleLogic), out List<object> runtimes))
            return;

        for (int i = 0; i < runtimes.Count; i++)
            _updateLogics.Add((IUpdateModuleLogic)runtimes[i]);
    }
    private void RebuildPetModules()
    {
        if (_petComponent == null)
            return;

        _featureMap.TryGetValue(typeof(IPetModule), out List<object> runtimes);
        _petComponent.RebuildModules(runtimes);
    }
    private void CacheRuntimeFeatures(object runtimeObject)
    {
        if (runtimeObject == null)
            return;

        Type objectType = runtimeObject.GetType();
        Type[] featureTypes = GetFeatureTypes(objectType);

        for (int i = 0; i < featureTypes.Length; i++)
        {
            Type featureType = featureTypes[i];

            if (!_featureMap.TryGetValue(featureType, out List<object> runtimes))
            {
                runtimes = new List<object>();
                _featureMap.Add(featureType, runtimes);
            }

            runtimes.Add(runtimeObject);
        }
    }

    private static Type[] GetFeatureTypes(Type objectType)
    {
        if (_featureTypeCache.TryGetValue(objectType, out Type[] cachedTypes))
            return cachedTypes;

        Type[] interfaces = objectType.GetInterfaces();
        List<Type> featureTypes = new();

        for (int i = 0; i < interfaces.Length; i++)
        {
            Type interfaceType = interfaces[i];

            if (interfaceType == typeof(IModuleHook))
                continue;

            if (!typeof(IModuleHook).IsAssignableFrom(interfaceType))
                continue;

            featureTypes.Add(interfaceType);
        }

        Type[] result = featureTypes.ToArray();
        _featureTypeCache.Add(objectType, result);

        return result;
    }

    private void ForEachFeature<T>(Action<T> action)
        where T : class, IModuleHook
    {
        if (action == null)
            return;

        if (!_featureMap.TryGetValue(typeof(T), out List<object> runtimes))
            return;

        for (int i = 0; i < runtimes.Count; i++)
            action((T)runtimes[i]);
    }

    public void OnSkillCast()
    {
        ForEachFeature<ISkillCastModifier>(modifier =>
        {
            modifier.OnSkillCast();
        });
    }

    public void OnHit(HitDealtEvent evt)
    {
        Entity target = evt.target;
        DamageData data = evt.data;

        if (evt.owner != _owner)
            return;

        ForEachFeature<IHitModifier>(modifier =>
        {
            modifier.OnHit(target, data);
        });
    }

    public void OnFistsAttack(WeaponAttackTriggeredEvent evt)
    {
        ForEachFeature<IFistsAttackModifier>(modifier =>
        {
            modifier.OnFistsAttack();
        });
    }

    public void OnBeforeDamage(ref DamageData data, Entity dealer)
    {
        if (!_featureMap.TryGetValue(typeof(IBeforeDamageModifier), out List<object> runtimes))
            return;

        for (int i = 0; i < runtimes.Count; i++)
            ((IBeforeDamageModifier)runtimes[i]).OnBeforeDamage(ref data, dealer);
    }

    private void RefreshSynergy()
    {
        List<SynergyToken> tokens = new();

        ForEachFeature<ISynergyProvider>(provider =>
        {
            provider.CollectTokens(tokens);
        });

        tokens.Sort((a, b) => b.priority.CompareTo(a.priority));

        ApplySynergy(tokens);
    }

    private void ApplySynergy(List<SynergyToken> tokens)
    {
        if (!_featureMap.TryGetValue(typeof(ISynergyReceiver), out List<object> receivers))
            return;

        for (int i = 0; i < receivers.Count; i++)
        {
            ISynergyReceiver receiver = (ISynergyReceiver)receivers[i];

            receiver.ResetSynergy();

            for (int j = 0; j < tokens.Count; j++)
                receiver.ApplyToken(tokens[j]);
        }
    }

    #region 박준서
    public void TriggerProjectileFire(Projectile projectile)
    {
        ForEachFeature<IProjectileFireModuleLogic>(logic =>
        {
            logic.OnProjectileFire(projectile);
        });
    }

    public void TriggerProjectileHit(Projectile projectile, ProjectileHitInfo hitInfo)
    {
        ForEachFeature<IProjectileHitModuleLogic>(logic =>
        {
            logic.OnProjectileHit(projectile, hitInfo);
        });
    }

    public void TriggerProjectileReset(Projectile projectile)
    {
        ForEachFeature<IProjectileResetModuleLogic>(logic =>
        {
            logic.OnProjectileReset(projectile);
        });
    }

    public void TriggerProjectileUpdate(Projectile projectile, float deltaTime)
    {
        if (!_featureMap.TryGetValue(typeof(IProjectileUpdateModuleLogic), out List<object> runtimes))
            return;

        for (int i = 0; i < runtimes.Count; i++)
            ((IProjectileUpdateModuleLogic)runtimes[i]).OnProjectileUpdate(projectile, deltaTime);
    }
    #endregion
}
