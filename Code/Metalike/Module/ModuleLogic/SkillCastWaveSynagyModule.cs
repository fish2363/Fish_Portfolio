using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

[Serializable]
public class SkillCastWaveSynagyModuleDef : IModuleLogicDef
{
    public float cooldownTime = 2f;
    public int shootCnt = 1;
    public int bulletCnt = 8;
    public float yOffset = 1f;
    public float bulletSpeed = 10f;
    public float shootDelay = 0.2f;

    public PoolItemSO bulletItem;
    public StatSO damageStat;
    public AttackDataSO attackData;

    public IModuleLogic CreateLogic() => new SkillCastWaveSynagyModule(this);
}

public class SkillCastWaveSynagyModule : IModuleLogic, ISynergyReceiver, ISkillCastModifier
{
    private readonly SkillCastWaveSynagyModuleDef _def;
    private Entity _owner;
    private float _cooldownTimer;
    private PoolItemSO _overrideBulletItem;
    private ModuleController _controller;
    private EntityStatCompo _entityStatCompo;

    private int _remainingShoots;
    private float _shootDelayTimer;

    private PoolItemSO CurrentBulletItem => _overrideBulletItem != null ? _overrideBulletItem : _def.bulletItem;

    public SkillCastWaveSynagyModule(SkillCastWaveSynagyModuleDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _controller = owner.GetCompo<ModuleController>();
        _entityStatCompo = owner.GetCompo<EntityStatCompo>();
    }
    public void ModuleUpdate(float deltaTime)
    {
        if (_cooldownTimer > 0) _cooldownTimer -= deltaTime;

        if (_remainingShoots > 0)
        {
            _shootDelayTimer -= deltaTime;
            if (_shootDelayTimer <= 0f)
            {
                FireWave();
                _remainingShoots--;
                _shootDelayTimer = _def.shootDelay;
            }
        }
    }

    public void OnUnequip() { }

    public void ResetSynergy() => _overrideBulletItem = null;
    public void ApplyToken(SynergyToken token)
    {
        if (token.key == SynergyKey.ProjectileItemOverride && token.payload is PoolItemSO item)
            _overrideBulletItem = item;
    }

    public void OnSkillCast()
    {
        if (_cooldownTimer > 0 || CurrentBulletItem == null) return;

        _remainingShoots = _def.shootCnt;
        _shootDelayTimer = 0f;

        _cooldownTimer = _def.cooldownTime;
    }

    private void FireWave()
    {
        int count = Mathf.Max(1, _def.bulletCnt);
        var damageData = _controller.DamageCompo.CalculateDamage(_entityStatCompo.GetStat(_def.damageStat), _def.attackData);
        Vector3 origin = _owner.transform.position + Vector3.up * _def.yOffset;

        for (int j = 0; j < count; j++)
        {
            Vector3 dir = Quaternion.Euler(0f, (360f / count) * j, 0f) * Vector3.forward;
            var proj = _controller.poolManager.Pop<Projectile>(CurrentBulletItem);
            proj.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(dir, Vector3.up));
            proj.Fire(damageData, dir, _def.bulletSpeed, _def.attackData, _owner);
        }
    }
}