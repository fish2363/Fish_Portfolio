using Core.EventBus;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.RunTime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeModuleInstance
{
    public ModuleSO Data { get; private set; }
    public List<IModuleLogic> Logics { get; private set; }

    public RuntimeModuleInstance(ModuleSO data)
    {
        Data = data;
        Logics = data.CreateAllLogics();
    }

    public void EquipAll(Entity owner)
    {
        foreach (var logic in Logics) logic.OnEquip(owner);
    }

    public void UnequipAll()
    {
        foreach (var logic in Logics) logic.OnUnequip();
    }
}

public class ModuleController : MonoBehaviour, IEntityComponent
{
    [HideInInspector][Inject] public CharacterManager CharacterManager;
    [HideInInspector][Inject] public PoolManagerMono poolManager;
    public VisualContainer Container { get; private set; }

    private Entity _owner;
    private SkillComponent _skillComponent;
    public LayerMask whatIsTarget;

    private readonly List<SynergyToken> _tokenBuffer = new(16);
    private List<RuntimeModuleInstance> _activeModules = new();
    public DamageCompo DamageCompo { get; private set; }
    private Collider[] _colliders;

    public void Initialize(Entity entity)
    {
        _owner = entity;
        Container = entity.GetCompo<VisualContainer>();
        _skillComponent = entity.GetCompo<SkillComponent>();
        DamageCompo = entity.GetCompo<DamageCompo>();

        Bus<PassiveChangeEvent>.OnEvent += ChangeHandle;
        Bus<GetModuleEvents>.OnEvent += GetModuleHandle;
        _skillComponent.OnSkillEvent += TriggerSkillCast;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (var module in _activeModules)
            foreach (var logic in module.Logics)
                logic.ModuleUpdate(dt);
    }

    private void GetModuleHandle(GetModuleEvents evt)
    {
        RuntimeModuleInstance newModule = new RuntimeModuleInstance(evt.passive);
        _activeModules.Add(newModule);
        newModule.EquipAll(_owner);

        RecalculateSynergy();
    }

    private void ChangeHandle(PassiveChangeEvent evt)
    {
        ClearModules();

        for (int i = 0; i < evt.container.ActivePassives.Count; i++)
        {
            RuntimeModuleInstance module = new RuntimeModuleInstance(evt.container.ActivePassives[i]);
            _activeModules.Add(module);
            module.EquipAll(_owner);
        }
        RecalculateSynergy();
    }

    public void TriggerBeforeDamage(ref DamageData damageData, Entity dealer)
    {
        foreach (var module in _activeModules)
            foreach (var logic in module.Logics)
                if (logic is IBeforeDamageModifier modifier)
                    modifier.OnBeforeDamage(ref damageData, dealer);
    }

    public void TriggerSkillCast()
    {
        foreach (var module in _activeModules)
            foreach (var logic in module.Logics)
                if (logic is ISkillCastModifier modifier)
                    modifier.OnSkillCast();
    }

    public void RecalculateSynergy()
    {
        foreach (var module in _activeModules)
            foreach (var logic in module.Logics)
                if (logic is ISynergyReceiver receiver) receiver.ResetSynergy();

        _tokenBuffer.Clear();

        foreach (var module in _activeModules)
            foreach (var logic in module.Logics)
                if (logic is ISynergyProvider provider) provider.CollectTokens(_tokenBuffer);

        if (_tokenBuffer.Count == 0) return;

        Dictionary<SynergyKey, SynergyToken> bestByKey = new();
        for (int i = 0; i < _tokenBuffer.Count; i++)
        {
            SynergyToken token = _tokenBuffer[i];
            if (!bestByKey.TryGetValue(token.key, out SynergyToken current) ||
                token.priority > current.priority)
            {
                bestByKey[token.key] = token;
            }
        }

        foreach (SynergyToken token in bestByKey.Values)
        {
            foreach (var module in _activeModules)
                foreach (var logic in module.Logics)
                    if (logic is ISynergyReceiver receiver) receiver.ApplyToken(token);
        }
    }

    private void ClearModules()
    {
        foreach (var module in _activeModules)
        {
            module.UnequipAll();
        }
        _activeModules.Clear();
    }

    public List<Entity> FindClosestEnemies(Vector3 position, float range, int count)
    {
        int cnt = GetEnemiesInRange(position, range);

        List<(Entity enemy, float distance)> validEnemies = new();

        for (int i = 0; i < cnt; i++)
        {
            if (_colliders[i].TryGetComponent(out Entity enemy) == false || enemy.IsDead)
                continue;

            float dist = Vector3.Distance(position, enemy.transform.position);
            validEnemies.Add((enemy, dist));
        }

        int takeCount = Mathf.Min(count, validEnemies.Count);

        List<Entity> result = validEnemies.OrderBy(x => x.distance).Select(x => x.enemy).Take(count).ToList();
        return result;
    }

    public List<Entity> FindFarthestEnemies(Vector3 position, float range, int count)
    {
        int cnt = GetEnemiesInRange(position, range);

        List<(Entity enemy, float distance)> validEnemies = new();

        for (int i = 0; i < cnt; i++)
        {
            if (_colliders[i].TryGetComponent(out Entity enemy) == false || enemy.IsDead)
                continue;

            float dist = Vector3.Distance(position, enemy.transform.position);
            validEnemies.Add((enemy, dist));
        }

        int takeCount = Mathf.Min(count, validEnemies.Count);

        List<Entity> result = validEnemies.OrderByDescending(x => x.distance).Select(x => x.enemy).Take(count).ToList();
        return result;
    }

    public Entity FindClosestEnemy(Vector3 position, float range)
    {
        Entity findEnemy = null;
        int cnt = GetEnemiesInRange(position, range);

        float closestDistance = float.MaxValue;

        for (int i = 0; i < cnt; i++)
        {
            if (_colliders[i].TryGetComponent(out Entity enemy) == false || enemy.IsDead)
                continue;

            float distanceToEnemy = Vector3.Distance(position, enemy.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                findEnemy = enemy;
            }
        }

        return findEnemy;
    }

    public Entity FindFarthestEnemy(Vector3 position, float range)
    {
        Entity farthestEnemy = null;
        int cnt = GetEnemiesInRange(position, range);

        float farthestDistance = float.MinValue;

        for (int i = 0; i < cnt; i++)
        {
            if (!_colliders[i].TryGetComponent(out Entity enemy) || enemy.IsDead)
                continue;

            float distanceToEnemy = Vector3.Distance(position, enemy.transform.position);
            if (distanceToEnemy > farthestDistance)
            {
                farthestDistance = distanceToEnemy;
                farthestEnemy = enemy;
            }
        }

        return farthestEnemy;
    }

    public int GetEnemiesInRange(Vector3 position, float range)
        => Physics.OverlapSphereNonAlloc(position, range, _colliders, whatIsTarget);

    private void OnDestroy()
    {
        Bus<PassiveChangeEvent>.OnEvent -= ChangeHandle;
        Bus<GetModuleEvents>.OnEvent -= GetModuleHandle;
        _skillComponent.OnSkillEvent -= TriggerSkillCast;
        ClearModules();
    }
}