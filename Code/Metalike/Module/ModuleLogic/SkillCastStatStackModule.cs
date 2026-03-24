using Assets.Work.CDH.Code.Eventss;
using Core.EventBus;
using System;
using UnityEngine;

[Serializable]
public class SkillCastStatStackModuleDef : IModuleLogicDef
{
    [Header("%만큼 증가"),Range(1, 100)] public float percent = 10f;
    public StatSO damageStat;

    public int maxStack = 5;
    public IModuleLogic CreateLogic() => new SkillCastStatStackModule(this);
}

public class SkillCastStatStackModule : IModuleLogic,ISkillCastModifier
{
    private readonly SkillCastStatStackModuleDef _def;
    private EntityStatCompo _statCompo;

    private int _currentStack;
    private float _appliedBonusValue;
    private float _baseStatValue;

    public SkillCastStatStackModule(SkillCastStatStackModuleDef def) => _def = def;

    public void OnEquip(Entity owner)
    {
        _statCompo = owner.GetCompo<EntityStatCompo>();

        _baseStatValue = _statCompo.GetStat(_def.damageStat)?.Value ?? 0f;

        Bus<RoomClearEvent>.OnEvent += ResetStack;
        ResetBuff();
    }

    public void ModuleUpdate(float deltaTime) { }

    public void OnUnequip()
    {
        Bus<RoomClearEvent>.OnEvent -= ResetStack;
        ResetBuff();
    }

    private void ResetStack(RoomClearEvent evt) => ResetBuff();

    private void ResetBuff()
    {
        _statCompo?.RemoveModifier(_def.damageStat, this);
        _appliedBonusValue = 0f;
        _currentStack = 0;
    }

    public void OnSkillCast()
    {
        if (_currentStack >= _def.maxStack) return;

        float delta = _baseStatValue * (_def.percent / 100f);

        if (delta > 0f)
        {
            _currentStack++;
            _appliedBonusValue += delta;

            _statCompo.RemoveModifier(_def.damageStat, this);
            _statCompo.AddModifier(_def.damageStat, this, _appliedBonusValue);
        }
    }
}