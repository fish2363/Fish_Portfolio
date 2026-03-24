using GondrLib.ObjectPool.RunTime;
using System;
using UnityEngine;

[Serializable]
public class SkillCastKnockbackModuleDef : IModuleLogicDef
{
    public float radius = 5f;
    public LayerMask enemyMask;
    public PoolItemSO effects;
    public MovementDataSO knockbackMovement;

    public IModuleLogic CreateLogic() => new SkillCastKnockbackModule(this);
}

public class SkillCastKnockbackModule : IModuleLogic, ISkillCastModifier
{
    private readonly SkillCastKnockbackModuleDef _def;

    private Entity _owner;
    private ModuleController _moduleController;
    private VisualContainer _visualContainer;

    private readonly Collider[] _hits = new Collider[32];

    public SkillCastKnockbackModule(SkillCastKnockbackModuleDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _owner = owner;
        _moduleController = owner.GetCompo<ModuleController>();
        _visualContainer = owner.GetCompo<VisualContainer>();
    }

    public void ModuleUpdate(float deltaTime) { }

    public void OnUnequip() { }

    public void OnSkillCast()
    {
        PoolingEffect effect = _moduleController.poolManager.Pop<PoolingEffect>(_def.effects);
        effect.PlayVFX(_visualContainer.CurrentVisual.transform.position,Quaternion.identity);

        Vector3 origin = _owner.transform.position;
        int count = Physics.OverlapSphereNonAlloc(origin, _def.radius, _hits, _def.enemyMask);

        for (int i = 0; i < count; i++)
        {
            IKnockBackable knockable = _hits[i]?.GetComponent<IKnockBackable>();
            if (knockable == null) continue;

            Vector3 dir = _hits[i].transform.position - origin;
            dir.y = 0f;
            dir = dir.sqrMagnitude < 0.0001f ? -_owner.transform.forward : dir.normalized;
            knockable.BackStep(dir, _def.knockbackMovement);
        }
    }
}