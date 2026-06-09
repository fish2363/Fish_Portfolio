using System;
using UnityEngine;

[Serializable]
public class StatOverride
{
    [SerializeField] private StatSO stat;
    [SerializeField] private bool isUseOverride;
    [SerializeField] private float overrideBaseValue;

    public StatSO Stat => stat;
    public StatOverride(StatSO stat) => this.stat = stat;

    public StatSO CreateStat() //스탯 복제후 오버라이드 값을 넣어서 리턴해준다.
    {
        StatSO newStat = stat.Clone() as StatSO;
        Debug.Assert(newStat != null, $"{nameof(newStat)} stat clone failed");

        if (isUseOverride)
        {
            newStat.BaseValue = overrideBaseValue;
        }

        return newStat;
    }
}
