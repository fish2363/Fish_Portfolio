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
    [field: SerializeField] public PlayerInputSO PlayerInput { get; private set; }

    [SerializeField] private StateDataSO[] stateDataList;
    [SerializeField] private CharacterData defalutCharacter;
    
    public CharacterData CurrentCharacter { get; private set; }

    public EntityStateMachine _stateMachine;
    public Action<bool> OnStunEvent;
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
        Bus<GameStartEvents>.OnEvent += StartGame;

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
        Bus<GameStartEvents>.OnEvent -= StartGame;

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

    public void OnStun(float value)
    {
        StartCoroutine(StunRoutine(value));
    }

    private IEnumerator StunRoutine(float value)
    {
        OnStunEvent?.Invoke(true);
        yield return new WaitForSeconds(value);
        OnStunEvent?.Invoke(false);
    }

    private void HandleHitEvent()
    {
        //const string hit = "HIT";
        // if (_actionData.HitByPowerAttack)
        //     ChangeState(hit, true);
    }

    public void StartGame(GameStartEvents evt)
    {
        const string idle = "IDLE";
        _stateMachine.ChangeState(idle);
        Bus<ChangeCharacterEvent>.Raise(new ChangeCharacterEvent(defalutCharacter));
    }

    private void Update()
    {
        _stateMachine.UpdateStateMachine();
    }

    public void ChangeState(string newStateName, bool force = false)
        => _stateMachine.ChangeState(newStateName, force);

    public void KnockBack(Vector3 direction, MovementDataSO knockbackMovement)
    {
        _movement.BackStep(direction, knockbackMovement);
    }
}