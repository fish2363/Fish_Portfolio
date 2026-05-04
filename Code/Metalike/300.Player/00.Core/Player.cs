using Core.EventBus;
using GondrLib.Dependencies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

[Provide]
public class Player : Entity, IDependencyProvider
{
    [SerializeField] private StateDataSO[] stateDataList;
    [SerializeField] private CharacterData defalutCharacter;
    
    public CharacterData CurrentCharacter { get; private set; }

    public EntityStateMachine _stateMachine;
    public Action OnChangeEvent;

    protected List<ICharacterChangeReceiver> _changeInfos = new();

    private CharacterMovement _movement;

    public void ChangeCharacter(ChangeCharacterEvent info)
    {
        CurrentCharacter = info.info;
        OnChangeEvent?.Invoke();
        foreach (ICharacterChangeReceiver changable in _changeInfos)
        {
            changable.OnCharacterChanged(info.info);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new EntityStateMachine(this, stateDataList);

        _movement = GetCompo<CharacterMovement>();

        OnHitEvent.AddListener(HandleHitEvent);
        OnDeathEvent.AddListener(HandleDeadEvent);

        Bus<ChangeCharacterEvent>.OnEvent += ChangeCharacter;

        GetChangableComponents();
    }

    private void GetChangableComponents()
    {
        GetComponentsInChildren<ICharacterChangeReceiver>().ToList()
            .ForEach(component => _changeInfos.Add(component));
    }

    protected override void OnDestroy()
    {
        Bus<ChangeCharacterEvent>.OnEvent -= ChangeCharacter;

        OnHitEvent.RemoveListener(HandleHitEvent);
        OnDeathEvent.RemoveListener(HandleDeadEvent);
    }

    private void HandleDeadEvent()
    {
        if (IsDead) return;
        IsDead = true;

        Bus<PlayerDeadEvents>.Raise(new PlayerDeadEvents());
        //PlayerChannel?.RaiseEvent(PlayerEvents.PlayerDead);
        //   ChangeState("DEAD", true);
    }

    private void HandleHitEvent()
    {
        //const string hit = "HIT";
        // if (_actionData.HitByPowerAttack)
        //     ChangeState(hit, true);
    }

    private void Update()
    {
        _stateMachine.UpdateStateMachine();
    }

    public void ChangeState(string newStateName)
        => _stateMachine.ChangeState(newStateName);

    public void KnockBack(Vector3 direction, MovementDataSO knockbackMovement)
    {
        _movement.BackStep(direction, knockbackMovement);
    }
}