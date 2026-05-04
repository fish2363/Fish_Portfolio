using Blade.Combat;
using Core.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillComponent : MonoBehaviour, IEntityComponent,ICharacterChangeReceiver
{
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private int maxCheckTargetCount = 5;

    public Collider[] Colliders { get; private set; }

    private Entity _entity;
    private Dictionary<SkillDataSO, Skill> _skillDict;
    [field: SerializeField] public Skill CurrentSkill { get; set; }
    [HideInInspector] public DamageCompo DamageCompo;
    public EntityVFX EntityVFX { get; private set; }

    public Action OnSkillEvent;

    public void Initialize(Entity entity)
    {
        _entity = entity;
        Colliders = new Collider[maxCheckTargetCount];
        _skillDict = new Dictionary<SkillDataSO, Skill>();

        GetComponentsInChildren<Skill>(false).ToList()
            .ForEach(skill => _skillDict.Add(skill.Data, skill));

        _skillDict.Values.ToList()
            .ForEach(skill => skill.InitializeSkill(_entity, this));

        DamageCompo = entity.GetCompo<DamageCompo>();
        EntityVFX = entity.GetCompo<EntityVFX>();
    }

    public Skill GetCurrentSkill()
    {
        Skill skill = CurrentSkill;
        return skill;
    }

    public Skill GetSkill(SkillDataSO skillSO)
    {
        Skill skill = _skillDict.GetValueOrDefault(skillSO);

        return skill;
    }

    public void ChangeSkill(SkillDataSO data)
    {
        if (data == null)
        {
            Debug.LogError("data is null");
            return;
        }

        if (_skillDict.TryGetValue(data, out var skill))
        {
            CurrentSkill = skill;
            return;
        }

        Debug.LogError($"SkillComponent에 해당 스킬 로직이 없음: {data.name}");
    }

    public int GetEnemiesInRange(Vector3 position, float range)
        => Physics.OverlapSphereNonAlloc(position, range, Colliders, whatIsTarget);

    public Entity FindClosestEnemy(Vector3 position, float range)
    {
        Entity findEnemy = null;
        int cnt = GetEnemiesInRange(position, range);

        float closestDistance = float.MaxValue;

        for (int i = 0; i < cnt; i++)
        {
            if (Colliders[i].TryGetComponent(out Entity enemy) == false
                || enemy.IsDead) continue;

            float distanceToEnemy = Vector3.Distance(position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                findEnemy = enemy;
            }
        }

        return findEnemy;
    }

    public void OnCharacterChanged(CharacterData info)
    {
        Debug.Log("스킬 교체됨");
        ChangeSkill(info.defaultSkill);
    }
}