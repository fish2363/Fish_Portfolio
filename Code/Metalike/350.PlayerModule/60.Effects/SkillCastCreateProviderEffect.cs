using System;
using System.Collections.Generic;
using UnityEngine;
using GondrLib.ObjectPool.RunTime;

[ModuleDisplayName("시너지 오브젝트 생성", "시너지를 일으키는 생성 효과를 줍니다.")]
[Serializable]
public class SkillCastCreateProviderEffectDef : IModuleEffectDef
{
    [Header("생성 설정")]
    public int spawnCount = 3;             // 생성 개수 (기존 bombCount)
    public float spawnRadius = 2f;         // 흩뿌릴 반경
    public PoolItemSO providerItemPrefab;  // 폭탄/장판 프리팹 (기존 bombModelPrefab)
    public StatSO damageStat;
    public AttackDataSO attackData;

    [Header("Synergy Payload")]
    public SynergyKeySO synergyKey;
    public PoolItemSO provideBulletItem;
    public int priority = 1;

    public IModuleEffect CreateEffect() => new SkillCastCreateProviderEffect(this);
}

public class SkillCastCreateProviderEffect : IExecutableEffect, ISynergyProvider
{
    private readonly SkillCastCreateProviderEffectDef _def;
    private Entity _owner;
    private ModuleController _controller;
    private EntityStatCompo _statCompo;

    public SkillCastCreateProviderEffect(SkillCastCreateProviderEffectDef def) => _def = def;

    public void OnInitialize(Entity owner)
    {
        _owner = owner;
        _controller = owner.GetCompo<ModuleController>();
        _statCompo = owner.GetCompo<EntityStatCompo>();
    }

    public void OnUnequip() { }

    public void CollectTokens(List<SynergyToken> tokens)
    {
        if (_def.provideBulletItem != null && _def.synergyKey != null)
            tokens.Add(new SynergyToken(_def.synergyKey, _def.priority, _def.provideBulletItem));
    }

    public void Execute(EffectContext ctx)
    {
        if (_def.providerItemPrefab == null) return;

        Vector3 centerPos = _owner.transform.position;
        var damageData = _controller.DamageCompo.CalculateDamage(_statCompo.GetStat(_def.damageStat), _def.attackData);

        for (int i = 0; i < _def.spawnCount; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * _def.spawnRadius;
            Vector3 spawnPos = centerPos + new Vector3(randomCircle.x, 0f, randomCircle.y);
            var providerObj = _controller.PoolManager.Pop<BaseFieldObject>(_def.providerItemPrefab);

            providerObj.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));

            providerObj.Init(damageData, _def.attackData, _owner);
        }
    }
}