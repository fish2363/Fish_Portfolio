using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct StatContainer
{
    public StatOverride[] statOverrides;
}

public class CharacterStatCompo : EntityStatCompo, IChangableInfo
{
    [SerializeField] private CharacterSO defalutCharacter;

    public override void Initialize(Entity _entity)
    {
        Owner = _entity;
        _statContainer = defalutCharacter.unitStat;
        InitializeStat(_statContainer);
    }

    private void InitializeStat(StatContainer statContainer)
    {
        _stats.Clear();
        _statContainer = statContainer;
        _stats = _statContainer.statOverrides.ToDictionary(s => s.Stat.statName, s => s.CreateStat());
    }

    public void Change(CharacterSO info)
    {
        ClearAllStatModifier();
        InitializeStat(info.unitStat);
        Debug.Log("�̼� ĳ���� ����");
    }
}
