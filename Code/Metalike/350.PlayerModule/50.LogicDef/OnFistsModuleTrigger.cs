using System;
using UnityEngine;

[Serializable]
[ModuleDisplayName("근접 무기 사용 시", "근접 무기 사용 시 발동합니다.")]
public class OnFistsModuleTriggerDef : ModuleTriggerDef
{
    [Header("충전 여부")]
    public bool useCooldownCharge = false;

    [Min(1)]
    public int maxCount = 1;

    [Min(0.01f)]
    public float cooldown = 1f;

    [Header("확률 발동을 쓸지")]
    public bool useProbability = false;

    [Range(0f, 1f)]
    [Header("확률")]
    public float probability = 1f;

    public override IModuleLogic CreateLogic()
    {
        return new OnFistsModuleTrigger(this);
    }
}

public class OnFistsModuleTrigger :
    ModuleTriggerBase<OnFistsModuleTriggerDef>,
    IFistsAttackModifier,
    IUpdateModuleLogic
{
    private int _currentCount;
    private float _timer;

    public OnFistsModuleTrigger(OnFistsModuleTriggerDef def) : base(def)
    {
    }

    public override void OnEquip(Entity owner)
    {
        base.OnEquip(owner);

        _currentCount = Mathf.Max(1, _def.maxCount);
        _timer = Mathf.Max(0.01f, _def.cooldown);
    }

    public void ModuleUpdate(float deltaTime)
    {
        if (!_def.useCooldownCharge)
            return;

        if (_currentCount >= _def.maxCount)
            return;

        _timer -= deltaTime;

        if (_timer > 0f)
            return;

        _currentCount++;
        _timer = Mathf.Max(0.01f, _def.cooldown);
    }

    public void OnFistsAttack()
    {
        if (_def.useCooldownCharge)
        {
            if (_currentCount <= 0)
                return;

            _currentCount--;
        }

        if (_def.useProbability && UnityEngine.Random.value > _def.probability)
            return;

        ExecuteAll(EffectContext.OnFistsAttack());
    }
}