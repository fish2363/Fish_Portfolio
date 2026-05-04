using Core.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HealthInfo
{
    public float maxHp;
    public float currentHp;

    public HealthInfo(float maxHp)
    {
        this.maxHp = maxHp;
        currentHp = this.maxHp;
    }
}


public class PlayerHealthCompo : EntityHealth, ICharacterChangeReceiver
{
    [field :SerializeField] public HealthInfo CurrentHealthInfo { get; set; }

    private ShieldComponent _shieldCompo;
    private Player _player;
    protected ModuleController _inventory;


    #region Test
    [ContextMenu("Test/Damage 20")]
    private void TestDamage20() => TestDamage(20f);

    private void TestDamage(float amount)
    {
        var data = new DamageData { damage = amount };
        ApplyDamage(data, transform.position, Vector3.up, null, null);
        Debug.Log($"[Test] -{amount} -> {CurrentHealthInfo.currentHp}/{CurrentHealthInfo.maxHp}");
    }
    #endregion

    public override void Initialize(Entity entity)
    {
        base.Initialize(entity);
        Bus<PlayerReviveEvents>.OnEvent += OnRevivePlayer;

        _player = entity as Player;
        _shieldCompo = entity.GetCompo<ShieldComponent>();
        _inventory = entity.GetCompo<ModuleController>();
    }

    private void OnRevivePlayer(PlayerReviveEvents evt)
    {
        float healValue = CurrentHealthInfo.maxHp * (evt.reviveHealth / 100);
        if (healValue <= 0f) return;

        CurrentHealthInfo.currentHp = Mathf.Clamp(CurrentHealthInfo.currentHp + healValue, 0f, CurrentHealthInfo.maxHp);

        Bus<HpChangeEvents>.Raise(new HpChangeEvents(CurrentHealthInfo));
    }

    public override void ApplyHeal(InstantHealData data)
    {
        if (_entity != null && _entity.IsDead) return;

        float healValue = data.isPercentage ? CurrentHealthInfo.maxHp * data.amount : data.amount;
        if (healValue <= 0f) return;

        float prev = CurrentHealthInfo.currentHp;
        CurrentHealthInfo.currentHp = Mathf.Clamp(CurrentHealthInfo.currentHp + healValue, 0f, CurrentHealthInfo.maxHp);

        if (!Mathf.Approximately(prev, CurrentHealthInfo.currentHp))
            Bus<HpChangeEvents>.Raise(new HpChangeEvents(CurrentHealthInfo));
    }

    public override void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, AttackDataSO attackData, Entity dealer)
    {
        if (_entity.IsDead || _entity.IsInvin) return;

        _inventory.OnBeforeDamage(ref damageData, dealer);

        float finalDamage = damageData.damage;

        DamageText text = UIPoolManager.Instance.Pop<DamageText>(damageTextSO, UILayer.World);
        text.TakeDamage(transform, Camera.current, finalDamage);

        if (_shieldCompo != null)
            finalDamage = _shieldCompo.AbsorbDamage(finalDamage);

        CurrentHealthInfo.currentHp = Mathf.Clamp(CurrentHealthInfo.currentHp - finalDamage, 0, CurrentHealthInfo.maxHp);

        Bus<HpChangeEvents>.Raise(new HpChangeEvents(CurrentHealthInfo));

        if (CurrentHealthInfo.currentHp <= 0)
        {
            _entity?.OnDeathEvent?.Invoke();
        }

        IKnockBackable knockBackable = _entity.GetComponentInChildren<IKnockBackable>();

        if (knockBackable != null && attackData != null && dealer != null)
        {
            Vector3 dir = (_entity.gameObject.transform.position - dealer.gameObject.transform.position).normalized;
            knockBackable.BackStep(dir, attackData.knockBackMovement);
        }

       
        _entity?.OnHitEvent?.Invoke();
    }
    public void OnCharacterChanged(CharacterData info)
    {
        StatOverride stat = info.unitStat.statOverrides.FirstOrDefault(x => x.Stat.statName == hpStat.statName);
        CurrentHealthInfo = new HealthInfo(stat.CreateStat().Value);

        Bus<HpChangeEvents>.Raise(new HpChangeEvents(CurrentHealthInfo));
    }
}
