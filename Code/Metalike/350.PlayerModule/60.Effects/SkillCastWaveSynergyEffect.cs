using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

[ModuleDisplayName("시너지 탄막 뿌리기", "시너지에 따라 변화하는 탄막을 흩뿌립니다.")]
[Serializable]
public class SkillCastWaveSynergyEffectDef : IModuleEffectDef
{
    public float cooldownTime = 2f;

    [Min(1)]
    public int shootCount = 1;

    [Min(1)]
    public int bulletCount = 8;

    public float yOffset = 1f;
    public float bulletSpeed = 10f;
    public float shootDelay = 0.2f;

    public PoolItemSO bulletItem;
    public StatSO damageStat;
    public AttackDataSO attackData;
    public SynergyKeySO synergyKey;

    public IModuleEffect CreateEffect()
    {
        return new SkillCastWaveSynergyEffect(this);
    }
}

public class SkillCastWaveSynergyEffect :
    IExecutableEffect,
    ISynergyReceiver,
    IUpdateModuleLogic
{
    private readonly SkillCastWaveSynergyEffectDef _def;

    private Entity _owner;
    private ModuleController _controller;
    private EntityStatCompo _statCompo;

    private PoolItemSO _overrideBulletItem;

    private float _cooldownTimer;
    private int _remainingShoots;
    private float _shootDelayTimer;

    private PoolItemSO CurrentBulletItem
    {
        get { return _overrideBulletItem != null ? _overrideBulletItem : _def.bulletItem; }
    }

    public SkillCastWaveSynergyEffect(SkillCastWaveSynergyEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        _owner = owner;
        _controller = owner.GetCompo<ModuleController>();
        _statCompo = owner.GetCompo<EntityStatCompo>();
    }

    public void OnUnequip()
    {
        _owner = null;
        _controller = null;
        _statCompo = null;
        _overrideBulletItem = null;

        _cooldownTimer = 0f;
        _remainingShoots = 0;
        _shootDelayTimer = 0f;
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= deltaTime;

        if (_remainingShoots <= 0)
            return;

        _shootDelayTimer -= deltaTime;
        if (_shootDelayTimer > 0f)
            return;

        FireWave();

        _remainingShoots--;
        _shootDelayTimer = _def.shootDelay;
    }

    public void Execute(EffectContext ctx)
    {
        if (_cooldownTimer > 0f)
            return;

        if (CurrentBulletItem == null)
            return;

        if (_owner == null || _controller == null || _statCompo == null)
            return;

        _remainingShoots = Mathf.Max(1, _def.shootCount);
        _shootDelayTimer = 0f;
        _cooldownTimer = _def.cooldownTime;
    }

    public void ResetSynergy()
    {
        _overrideBulletItem = null;
    }

    public void ApplyToken(SynergyToken token)
    {
        if (token.key != _def.synergyKey)
            return;

        if (token.payload is PoolItemSO item)
            _overrideBulletItem = item;
    }

    private void FireWave()
    {
        if (CurrentBulletItem == null || _controller == null || _statCompo == null)
            return;

        int count = Mathf.Max(1, _def.bulletCount);

        var damageStat = _statCompo.GetStat(_def.damageStat);
        if (damageStat == null || _def.attackData == null)
            return;

        DamageData damageData =
            _controller.DamageCompo.CalculateDamage(damageStat, _def.attackData);

        Vector3 origin = _owner.transform.position + Vector3.up * _def.yOffset;

        for (int i = 0; i < count; i++)
        {
            float angle = 360f / count * i;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            var projectile = _controller.PoolManager.Pop<Projectile>(CurrentBulletItem);
            if (projectile == null)
                continue;

            projectile.transform.SetPositionAndRotation(
                origin,
                Quaternion.LookRotation(direction, Vector3.up)
            );

            projectile.Fire(
                damageData,
                direction,
                _def.bulletSpeed,
                _def.attackData,
                _owner
            );
        }
    }
}
