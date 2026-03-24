using System;
using System.Collections.Generic;
using UnityEngine;
using GondrLib.ObjectPool.RunTime;

[Serializable]
public class SkillCastCreateProviderModuleDef : IModuleLogicDef
{
    public float cooldownTime = 5f;

    [Header("»ý¼º ¼³Á¤")]
    public int spawnCount = 3;             // »ý¼º °³¼ö (±âÁ¸ bombCount)
    public float spawnRadius = 2f;         // Èð»Ñ¸± ¹Ý°æ


    public PoolItemSO providerItemPrefab;  // ÆøÅº/ÀåÆÇ ÇÁ¸®ÆÕ (±âÁ¸ bombModelPrefab)
    public StatSO damageStat;
    public AttackDataSO attackData;

    [Header("Synergy Payload")]
    public PoolItemSO provideBulletItem;
    public int priority = 1;

    public IModuleLogic CreateLogic() => new SkillCastCreateProviderModule(this);
}

public class SkillCastCreateProviderModule : IModuleLogic, ISynergyProvider, ISkillCastModifier
{
    private readonly SkillCastCreateProviderModuleDef _def;

    private Entity _owner;
    private ModuleController _controller;
    private EntityStatCompo _statCompo;
    private float _cooldownTimer;


    public SkillCastCreateProviderModule(SkillCastCreateProviderModuleDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _controller = owner.GetCompo<ModuleController>();
        _statCompo = owner.GetCompo<EntityStatCompo>();
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_cooldownTimer > 0) _cooldownTimer -= deltaTime;
    }

    public void OnUnequip() { }

    public void CollectTokens(List<SynergyToken> tokens)
    {
        if (_def.provideBulletItem != null)
            tokens.Add(new SynergyToken(SynergyKey.ProjectileItemOverride, _def.priority, _def.provideBulletItem));
    }

    public void OnSkillCast()
    {
        if (_cooldownTimer > 0 || _def.providerItemPrefab == null) return;

        SpawnProviders();
        _cooldownTimer = _def.cooldownTime;
    }

    private void SpawnProviders()
    {
        Vector3 centerPos = _owner.transform.position;
        var damageData = _controller.DamageCompo.CalculateDamage(_statCompo.GetStat(_def.damageStat), _def.attackData);

        for (int i = 0; i < _def.spawnCount; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * _def.spawnRadius;
            Vector3 spawnPos = centerPos + new Vector3(randomCircle.x, 0f, randomCircle.y);
            var providerObj = _controller.poolManager.Pop<BaseFieldObject>(_def.providerItemPrefab);

            providerObj.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));

            providerObj.Init(damageData, _def.attackData, _owner);
        }
    }
}