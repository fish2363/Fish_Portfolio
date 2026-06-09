using Core.EventBus;
using System;

[Serializable]
[ModuleDisplayName("총알 모디파이어", "총알에 특수한 효과 부여")]
public class BulletModifierEffectDef : IModuleEffectDef
{
    public string modifierEventName;

    public IModuleEffect CreateEffect()
    {
        return new BulletModifierEffect(this);
    }
}

public class BulletModifierEffect : IModuleEffect
{
    private readonly BulletModifierEffectDef _def;

    public BulletModifierEffect(BulletModifierEffectDef def)
    {
        _def = def;
    }

    public void OnInitialize(Entity owner)
    {
        if (string.IsNullOrWhiteSpace(_def.modifierEventName))
            return;

        Bus<ProjectileModuleEvent>.Raise(
            new ProjectileModuleEvent(_def.modifierEventName)
        );
    }

    public void OnUnequip()
    {
        // TODO: ProjectileModuleEvent 구조가 해제 이벤트를 지원하면 여기서 제거 처리
    }
}
