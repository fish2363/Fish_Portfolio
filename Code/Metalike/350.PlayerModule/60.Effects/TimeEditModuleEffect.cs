using Core.EventBus;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
[ModuleDisplayName("게임 시간 편집", "Time.timeScale을 건드립니다.")]
public class TimeEditModuleEffectDef : IModuleEffectDef
{
    public StopInfoSO infoSO;
    public string ppKey = "TimeEditModuleEffect";
    public int ppPriority = 100;

    public IModuleEffect CreateEffect()
    {
        return new TimeEditModuleEffect(this);
    }
}


public class TimeEditModuleEffect : IExecutableEffect
{
    private readonly TimeEditModuleEffectDef _def;
    private Entity _owner;

    public TimeEditModuleEffect(TimeEditModuleEffectDef def)
    {
        _def = def;
    }

    public void Execute(EffectContext ctx)
    {
        if (_def.infoSO == null)
            return;

        if (_owner == null)
            _owner = ctx.Owner;

        TimeManager.Instance.GenerateStop(_def.infoSO);
        Bus<PPDistortionApplyEvent>.Raise(new PPDistortionApplyEvent(GetPPKey(), _def.ppPriority));

        if (!_def.infoSO.IsInfinite && _owner != null)
            _owner.StartSafeCoroutine(this, CancelPPRoutine(_def.infoSO.Duration));
    }

    public void OnInitialize(Entity owner)
    {
        _owner = owner;
    }

    public void OnUnequip()
    {
        if (_owner != null)
            _owner.StopSafeCoroutine(this);

        Bus<PPDistortionCancelEvent>.Raise(new PPDistortionCancelEvent(GetPPKey()));

        if (_def.infoSO != null)
            TimeManager.Instance.ReleaseStop(_def.infoSO.StopChannel);
        else
            TimeManager.Instance.ReleaseStop(StopChannel.Field);
    }

    private IEnumerator CancelPPRoutine(float duration)
    {
        if (duration > 0f)
            yield return new WaitForSecondsRealtime(duration);
        else
            yield return null;

        Bus<PPDistortionCancelEvent>.Raise(new PPDistortionCancelEvent(GetPPKey()));
    }

    private string GetPPKey()
    {
        return string.IsNullOrEmpty(_def.ppKey) ? nameof(TimeEditModuleEffect) : _def.ppKey;
    }
}
