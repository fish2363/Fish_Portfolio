using UnityEngine;

public abstract class DamageCaster : MonoBehaviour
{
    [SerializeField] protected LayerMask whatIsEnemy;

    protected Entity _owner;

    public virtual void InitCaster(Entity owner)
    {
        _owner = owner;
    }

    public virtual void ApplyDamageAndKnockBack(
        Transform target,
        DamageData damageData,
        Vector3 position,
        Vector3 normal,
        AttackDataSO attackData,
        Vector3 knockBackDirection)
    {
        Entity targetEntity = target.GetComponentInParent<Entity>();

        if (targetEntity == null)
            return;

        if (targetEntity == _owner)
            return;

        IDamageable damageable = targetEntity.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.ApplyDamage(damageData, position, normal, attackData, _owner);
        }

        if (attackData == null || attackData.knockBackMovement == null)
            return;

        IKnockBackable knockBackable = targetEntity.GetComponent<IKnockBackable>();

        if (knockBackable != null)
        {
            knockBackable.KnockBack(knockBackDirection, attackData.knockBackMovement);
        }
    }

    public abstract bool CastDamage(
        DamageData damageData,
        Vector3 position,
        Vector3 direction,
        AttackDataSO attackData);
}
