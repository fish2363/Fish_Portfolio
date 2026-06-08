using System.Collections.Generic;
using UnityEngine;

public class SphereDamageCaster : DamageCaster
{
    [SerializeField, Range(0.5f, 3f)] private float castRadius = 1f;
    [SerializeField, Range(0, 3f)] private float castingRange = 1f;
    [SerializeField, Range(0, 2f)] private float startForwardOffset = 0.5f;

    private Vector3 _lastCastDirection = Vector3.forward;

    public override bool CastDamage(DamageData damageData, Vector3 position, Vector3 direction, AttackDataSO attackData)
    {
        Vector3 attackDirection = direction;
        attackDirection.y = 0f;

        if (attackDirection.sqrMagnitude < 0.0001f)
            attackDirection = transform.forward;

        attackDirection.Normalize();
        _lastCastDirection = attackDirection;

        Vector3 startPos = position + attackDirection * startForwardOffset;

        RaycastHit[] hits = Physics.SphereCastAll(
            startPos,
            castRadius,
            attackDirection,
            castingRange,
            whatIsEnemy);

        HashSet<Entity> damagedEntities = new HashSet<Entity>();
        bool isHit = false;

        foreach (RaycastHit hit in hits)
        {
            Entity targetEntity = hit.collider.GetComponentInParent<Entity>();

            if (targetEntity == null)
                continue;

            if (targetEntity == _owner)
                continue;

            if (damagedEntities.Contains(targetEntity))
                continue;

            damagedEntities.Add(targetEntity);

            ApplyDamageAndKnockBack(
                hit.collider.transform,
                damageData,
                hit.point,
                hit.normal,
                attackData,
                attackDirection);

            isHit = true;
        }

        Debug.Log($"SphereCastAll hitCount={hits.Length}, damaged={damagedEntities.Count}");

        return isHit;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 attackDirection = _lastCastDirection;

        if (attackDirection.sqrMagnitude < 0.0001f)
            attackDirection = transform.forward;

        attackDirection.y = 0f;
        attackDirection.Normalize();

        Vector3 startPos = transform.position + attackDirection * startForwardOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPos, castRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos + attackDirection * castingRange, castRadius);
    }
#endif
}
