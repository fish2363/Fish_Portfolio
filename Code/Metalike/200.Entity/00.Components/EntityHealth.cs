using System;
using Core.EventBus;
using GondrLib.ObjectPool.RunTime;
using Public.Core.Events;
using System.Collections.Generic;
using UnityEngine;
using Work.SB._01.Scripts.Enemy.Interface;
using Work.SB._01.Scripts.Enemy.Script;

public class EntityHealth : MonoBehaviour, IEntityComponent, IDamageable, IAfterInitialize
{
    public delegate void OnHealthChanged(float current, float max);
    public OnHealthChanged OnHealthChangeEvent;
    public OnHealthChanged OnSpareHealthChangeEvent;

    [SerializeField] protected StatSO hpStat;
    [SerializeField] protected PoolItemSO damageTextSO;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;

    protected float _maxHealth;
    protected float _currentHealth;

    protected Entity _entity;
    protected EntityStatCompo _statCompo;
    protected Vector3 hitDir;

    public virtual void Initialize(Entity entity)
    {
        _entity =  entity;
        _statCompo = entity.GetCompo<EntityStatCompo>();
    }

    public virtual void AfterInitialize()
    {
        hpStat = _statCompo.GetStat(hpStat);
        _maxHealth = _currentHealth = hpStat.BaseValue;
    }

    private void OnDestroy()
    {
        //_statCompo.UnSubscribeStat(hpStat, HandleMaxHPChanged);
    }

    private void HandleMaxHPChanged(StatSO stat, float currentvalue, float previousvalue)
    {
        float changed = currentvalue - previousvalue;
        _maxHealth = currentvalue;

        if (changed > 0)
            _currentHealth = Mathf.Clamp(_currentHealth + changed, 0, _maxHealth);
        else
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
    }

    public virtual void ApplyDamage(
        DamageData damageData,
        Vector3 hitPoint,
        Vector3 hitNormal,
        AttackDataSO attackData,
        Entity dealer)
    {
        if (_entity.IsDead || _entity.IsInvin) return;

        float finalDamage = damageData.damage;
        if (finalDamage <= 0f) return;

       
        if (_entity is Enemy enemy)
        {
            Bus<HitDealtEvent>.Raise(new HitDealtEvent(dealer,enemy,damageData));
        }

        ISpareHealth spare = null;
        if (_entity != null)
        {
            spare = _entity.GetComponentInChildren<ISpareHealth>();
        }
        
        DamageText text = UIPoolManager.Instance.Pop<DamageText>(damageTextSO, UILayer.World);
        Camera cam = Camera.current != null ? Camera.current : Camera.main;
        text.TakeDamage(transform, cam, finalDamage);

        if (spare != null && spare.IsIntact)
        {
            
            OnHealthChangeEvent?.Invoke(spare.Current,spare.Max);
            float absorbed = spare.Absorb(finalDamage);
            if (absorbed >= finalDamage - 0.0001f)
                return;
            finalDamage -= absorbed;
       

            
            if (finalDamage <= 0f)
                return;
        }
        
        IKnockBackable knockBackable = _entity.GetComponentInChildren<IKnockBackable>();
        if (knockBackable != null && _currentHealth - finalDamage > 0)
        {
            Vector3 dir = (_entity.transform.position - dealer.transform.position).normalized;
            knockBackable.BackStep(dir, attackData.knockBackMovement);
        } 
        hitDir = hitPoint - dealer.transform.position;
        ApplyFinalDamege(finalDamage);
        _entity?.OnHitEvent?.Invoke();
    }

    public void ApplyFinalDamege(float finalDamage)
    {
        if (_entity == null || _entity.IsDead)
            return;

        _currentHealth = Mathf.Clamp(_currentHealth - finalDamage, 0, _maxHealth);
        OnHealthChangeEvent?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            print("¾È³ç");
            _entity.IsDead = true;
            _entity.OnDeathEvent?.Invoke();
        }
    }

    public void Suicide()
    {
        if (_entity == null || _entity.IsDead) return;

        ApplyFinalDamege(_maxHealth);
    }

 

    //public float GetTypeAdvantage(CharacterType attacker, CharacterType defender)
    //{
    //    return (attacker, defender) switch
    //    {
    //        (CharacterType.Hack, CharacterType.Metal) => 1.5f,
    //        (CharacterType.Metal, CharacterType.Normal) => 2f,
    //        (CharacterType.Normal, CharacterType.Hack) => 1.5f,
    //        _ => 1
    //    };
    //}

    #region Heal

    public virtual void ApplyHeal(InstantHealData data)
    {
        if (_entity != null && _entity.IsDead) return;

        float healValue = data.isPercentage ? _maxHealth * data.amount : data.amount;
        if (healValue <= 0f) return;

        float prev = _currentHealth;
        _currentHealth = Mathf.Clamp(_currentHealth + healValue, 0f, _maxHealth);

        if (!Mathf.Approximately(prev, _currentHealth))
            OnHealthChangeEvent?.Invoke(_currentHealth, _maxHealth);
    }

    #endregion
}