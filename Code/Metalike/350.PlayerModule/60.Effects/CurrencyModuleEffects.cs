using Code.Events;
using Code.Item;
using Core.EventBus;
using GondrLib.ObjectPool.RunTime;
using PJS.Managers;
using System;
using UnityEngine;

[Serializable]
[ModuleDisplayName("재화 획득 증가", "장착 중 얻는 재화의 양이 증가합니다.")]
public class CurrencyGainBoostEffectDef : IModuleEffectDef
{
    public CurrencyTypeSO currencyType;

    [Min(1f)]
    public float gainMultiplier = 2f;

    public IModuleEffect CreateEffect()
    {
        return new CurrencyGainBoostEffect(this);
    }
}

public class CurrencyGainBoostEffect : IModuleEffect
{
    private readonly CurrencyGainBoostEffectDef _def;
    private bool _isApplyingBonus;

    public CurrencyGainBoostEffect(CurrencyGainBoostEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        Bus<CurrencyIncreaseEvent>.OnEvent += OnCurrencyIncrease;
    }

    public void OnUnequip()
    {
        Bus<CurrencyIncreaseEvent>.OnEvent -= OnCurrencyIncrease;
    }

    private void OnCurrencyIncrease(CurrencyIncreaseEvent evt)
    {
        if (_isApplyingBonus)
            return;

        if (_def.currencyType != null && evt.currencyType != _def.currencyType)
            return;

        if (_def.gainMultiplier <= 1f)
            return;

        float bonusAmount = evt.amount * (_def.gainMultiplier - 1f);
        if (bonusAmount <= 0f)
            return;

        _isApplyingBonus = true;
        Bus<CurrencyIncreaseEvent>.Raise(
            new CurrencyIncreaseEvent(evt.currencyType, bonusAmount, evt.targetTransform)
        );
        _isApplyingBonus = false;
    }
}

[Serializable]
[ModuleDisplayName("피격 시 재화 드랍", "피격 시 보유 재화를 일정량 잃습니다.")]
public class DropCurrencyOnHitEffectDef : IModuleEffectDef
{
    public CurrencyTypeSO currencyType;

    [Min(0f)]
    public float dropAmount = 10f;

    public bool isPercentage = false;

    [Range(0f, 1f)]
    public float dropPercent = 0.1f;
    public PoolItemSO moneySO;

    [Range(0f, 1f)]
    public float dropReturnPercent = 0.5f;

    [Min(1)]
    public int maxDropCount = 12;

    [Min(1f)]
    public float amountPerDrop = 1f;

    [Min(0f)]
    public float scatterPower = 4f;

    [Min(0f)]
    public float upwardWeight = 0.7f;

    public float spawnHeight = 0.5f;

    public IModuleEffect CreateEffect()
    {
        return new DropCurrencyOnHitEffect(this);
    }
}

public class DropCurrencyOnHitEffect : IExecutableEffect
{
    private readonly DropCurrencyOnHitEffectDef _def;
    private Entity _owner;

    public DropCurrencyOnHitEffect(DropCurrencyOnHitEffectDef def)
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
        if (_def.currencyType == null || _def.moneySO == null)
            return;

        float currentAmount = CurrencyManager.Instance.GetAmount(_def.currencyType);
        float decreaseAmount = _def.isPercentage
            ? currentAmount * _def.dropPercent
            : _def.dropAmount;

        decreaseAmount = Mathf.Min(decreaseAmount, currentAmount);
        if (decreaseAmount <= 0f)
            return;

        Bus<CurrencyDecreaseEvent>.Raise(
            new CurrencyDecreaseEvent(_def.currencyType, decreaseAmount)
        );

        float dropTotalAmount = decreaseAmount * _def.dropReturnPercent;
        if (dropTotalAmount <= 0f)
            return;

        ScatterDroppedCurrency(ctx, dropTotalAmount);
    }

    private void ScatterDroppedCurrency(EffectContext ctx, float totalAmount)
    {
        float amountPerDrop = Mathf.Max(1f, _def.amountPerDrop);
        int dropCount = Mathf.CeilToInt(totalAmount / amountPerDrop);
        dropCount = Mathf.Clamp(dropCount, 1, Mathf.Max(1, _def.maxDropCount));

        float remainingAmount = totalAmount;
        Transform spawnTransform = ctx.Owner != null
            ? ctx.Owner.transform
            : _owner != null
                ? _owner.transform
                : null;

        Vector3 spawnPosition = spawnTransform != null
            ? spawnTransform.position + Vector3.up * _def.spawnHeight
            : Vector3.up * _def.spawnHeight;

        for (int i = 0; i < dropCount; i++)
        {
            float rewardAmount = i == dropCount - 1
                ? remainingAmount
                : Mathf.Min(amountPerDrop, remainingAmount);

            remainingAmount -= rewardAmount;

            DropableItem money = CurrencyManager.Instance.GetPoolManager().Pop<DropableItem>(_def.moneySO);
            if (money == null)
                continue;

            money.ResetItem();
            money.transform.position = spawnPosition;
            money.SetCurrencyReward(_def.currencyType, rewardAmount);

            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle;
            if (randomCircle.sqrMagnitude <= Mathf.Epsilon)
                randomCircle = Vector2.right;

            Vector3 dropDirection = new Vector3(randomCircle.x, _def.upwardWeight, randomCircle.y).normalized;
            money.DropItem(dropDirection, _def.scatterPower, false);
        }
    }
}
