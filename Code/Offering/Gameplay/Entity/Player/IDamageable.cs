using UnityEngine;

public interface IDamageable
{
    public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, AttackDataSO attackData, Entity dealer);
}
