using Core.EventBus;

//public struct AssistGaugeChangedEvent : IEvent
//{
//    public CharacterSO character;
//    public float current;
//    public float max;

//    public AssistGaugeChangedEvent(CharacterSO character, float current, float max)
//    {
//        this.character = character;
//        this.current = current;
//        this.max = max;
//    }
//}
public struct ChangeCharacterEvent : IEvent
{
    public CharacterData info;

    public ChangeCharacterEvent(CharacterData character)
    {
        info = character;
    }
}

//public struct DontChangeCharacterEvent : IEvent
//{
//    public List<CharacterSO> infos;

//    public DontChangeCharacterEvent(List<CharacterSO> character)
//    {
//        infos = character;
//    }
//}

//public struct AssistSwapTriggerEvent : IEvent
//{
//    public CharacterSO from;
//    public CharacterSO to;

//    public AssistSwapTriggerEvent(CharacterSO from, CharacterSO to)
//    {
//        this.from = from;
//        this.to = to;
//    }
//}

//public struct FindFriendEvent : IEvent
//{
//    public CharacterSO info;

//    public FindFriendEvent(CharacterSO character)
//    {
//        info = character;
//    }
//}
//public struct AssistCooldownEvent : IEvent
//{
//    public CharacterSO character;
//    public float currentCooldown; // 현재 남은 쿨타임
//    public float maxCooldown;     // 스킬의 전체 쿨타임

//    public AssistCooldownEvent(CharacterSO character, float currentCooldown, float maxCooldown)
//    {
//        this.character = character;
//        this.currentCooldown = currentCooldown;
//        this.maxCooldown = maxCooldown;
//    }
//}
//public struct AddChangeSkillGauge : IEvent
//{
//    public float damage;
//    public PlayerClass playerClass;
//    public AddChangeSkillGauge(float damage, PlayerClass playerClass)
//    {
//        this.damage = damage;
//        this.playerClass = playerClass;
//    }
//}


//public struct SelectedEvent : IEvent
//{
//    public CharacterSO info;
//    public bool castAssistOnSwap;

//    public SelectedEvent(CharacterSO info, bool castAssistOnSwap)
//    {
//        this.info = info;
//        this.castAssistOnSwap = castAssistOnSwap;
//    }
//}

//public struct SelectedEndEvent : IEvent
//{
//    public CharacterSO info;
//    public bool castAssistOnSwap;

//    public SelectedEndEvent(CharacterSO info, bool castAssistOnSwap)
//    {
//        this.info = info;
//        this.castAssistOnSwap = castAssistOnSwap;
//    }
//}