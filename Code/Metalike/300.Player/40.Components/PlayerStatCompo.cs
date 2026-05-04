using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct StatContainer
{
    public StatOverride[] statOverrides;
}

public class PlayerStatCompo : EntityStatCompo, ICharacterChangeReceiver
{
    [SerializeField] private CharacterData defalutCharacter;
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

    public StatContainer GetAllStats() => _statContainer;

    public void OnCharacterChanged(CharacterData info)
    {
        ClearAllStatModifier();
        InitializeStat(info.unitStat);
        Debug.Log("�̼� ĳ���� ����");
    }
}
