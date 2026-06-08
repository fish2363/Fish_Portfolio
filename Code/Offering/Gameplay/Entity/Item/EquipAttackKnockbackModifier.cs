using System;
using UnityEngine;

[Flags]
public enum EEquipAttackModifier
{
    KNone = 0,               
    KnockbackEnemy = 1 << 0, 
    KnockbackMySelf = 1 << 1,
    KnockbackNearEnemies = 1 << 2
}

[Serializable]
public class EquipAttackKnockbackModifierDef : IEquipItemDef
{
    public EEquipAttackModifier attackTargets;
    public MovementDataSO knockData;

    [Header("광역 넉백용 반경 (KnockbackNearEnemies 체크 시 사용)")]
    public float splashRadius = 3f;
    public IEquipItem CreateLogic() => new EquipAttackKnockbackModifier(this);
}


public class EquipAttackKnockbackModifier : IEquipItem,IEquipAttackModifierItem
{
    private readonly EquipAttackKnockbackModifierDef _def;
    private Player _owner;

    public EquipAttackKnockbackModifier(EquipAttackKnockbackModifierDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner as Player;
    }

    public void OnAttack(Player target)
    {
        if (_owner == null || target == null || _owner == target) return;

        Vector3 dirToTarget = (target.transform.position - _owner.transform.position).normalized;

        dirToTarget.y = 0;
        dirToTarget.Normalize();

        if (_def.attackTargets.HasFlag(EEquipAttackModifier.KnockbackEnemy))
        {
            target.KnockBack(dirToTarget, _def.knockData);
        }

        if (_def.attackTargets.HasFlag(EEquipAttackModifier.KnockbackMySelf))
        {
            Vector3 recoilDir = -dirToTarget;
            _owner.KnockBack(recoilDir, _def.knockData);
        }

        if (_def.attackTargets.HasFlag(EEquipAttackModifier.KnockbackNearEnemies))
        {
            Collider[] colliders = Physics.OverlapSphere(target.transform.position, _def.splashRadius);

            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent(out Player nearEnemy) && nearEnemy != _owner && nearEnemy != target)
                {
                    Vector3 splashDir = (nearEnemy.transform.position - target.transform.position).normalized;
                    splashDir.y = 0;
                    splashDir.Normalize();

                    nearEnemy.KnockBack(splashDir, _def.knockData);
                }
            }
        }
    }

    public void OnUnequip()
    {
        _owner = null;
    }
}
