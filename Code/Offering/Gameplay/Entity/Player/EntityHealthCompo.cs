using UnityEngine;

public class EntityHealthCompo : MonoBehaviour, IEntityComponent, IDamageable, IAfterInitialize
{
    private Entity _entity;
    private Player _player;
    private ActionData _actionData;
    private EntityStatCompo _statCompo;

    [SerializeField] private StatSO hpStat;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    public delegate void OnHealthChanged(float current, float max);
    public event OnHealthChanged OnHealthChangeEvent;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    [ContextMenu("Test/Damage 1")]
    private void TestDamage1() => TestDamage(100f);

    private void TestDamage(float amount)
    {
        var data = new DamageData { damage = amount };
        ApplyDamage(data, transform.position, Vector3.up, null, null);
        Debug.Log($"[Test] -{amount} -> HP {currentHealth}/{maxHealth}");
    }

    public void Initialize(Entity entity)
    {
        _entity = entity;
        _player = entity as Player;
        _actionData = entity.GetCompo<ActionData>();
        _statCompo = entity.GetCompo<EntityStatCompo>();
    }

    public void AfterInitialize()
    {
        maxHealth = currentHealth = _statCompo.SubscribeStat(
            hpStat, HandleMaxHPChanged, 10f);

        NotifyHealthChanged();
    }

    private void OnDestroy()
    {
        if (_statCompo != null)
            _statCompo.UnSubscribeStat(hpStat, HandleMaxHPChanged);
    }

    private void RaiseHpChangeEvent()
    {
        if (_player != null && !_player.IsLocalPlayer)
            return;

        Bus<HpChangeEvents>.Raise(new HpChangeEvents(currentHealth, maxHealth));
    }

    public void NotifyHealthChanged()
    {
        OnHealthChangeEvent?.Invoke(currentHealth, maxHealth);
        RaiseHpChangeEvent();
    }

    private void HandleMaxHPChanged(StatSO stat, float currentvalue, float previousvalue)
    {
        float changed = currentvalue - previousvalue;

        maxHealth = currentvalue;

        if (changed > 0)
            currentHealth = Mathf.Clamp(currentHealth + changed, 0, maxHealth);
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        NotifyHealthChanged();
    }

    public void ApplyHeal(InstantHealData data)
    {
        if (_entity != null && _entity.IsDead) return;

        float healValue = data.isPercentage ? maxHealth * data.amount : data.amount;
        if (healValue <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth + healValue, 0f, maxHealth);

        NotifyHealthChanged();
    }

    public void RestoreFullHealth(bool allowDeadHeal = true)
    {
        if (_entity != null && _entity.IsDead && !allowDeadHeal)
            return;

        currentHealth = maxHealth;

        NotifyHealthChanged();
    }

    public void ApplyDamage(
        DamageData damageData,
        Vector3 hitPoint,
        Vector3 hitNormal,
        AttackDataSO attackData,
        Entity dealer)
    {
        if (_entity == null || _entity.IsDead) return;

        Debug.Log($"[DAMAGE BEFORE] target={name}, damage={damageData.damage}, hp={currentHealth}/{maxHealth}");

        if (_actionData != null)
        {
            _actionData.HitNormal = hitNormal;
            _actionData.HitPoint = hitPoint;
            _actionData.HitByPowerAttack = attackData != null && attackData.isPowerAttack;
            _actionData.LastDamageData = damageData;
        }

        currentHealth = Mathf.Clamp(currentHealth - damageData.damage, 0, maxHealth);

        Debug.Log($"[DAMAGE AFTER] target={name}, hp={currentHealth}/{maxHealth}");

        NotifyHealthChanged();

        if (currentHealth <= 0)
            _entity?.OnDeathEvent?.Invoke();

        _entity?.OnHitEvent?.Invoke();
    }
}
