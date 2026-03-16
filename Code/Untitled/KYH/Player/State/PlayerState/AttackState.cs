using UnityEngine;

public class AttackState : EntityState
{
    private Player _player;
    private EntityMover _mover;
    private PlayerAttackCompo _attackCompo;

    private int _comboCounter;
    private float _lastAttackTime;
    private readonly float _comboWindow = 0.8f; //콤보가 이어지도록 하는 시간제한
    private const int MAX_COMBO_COUNT = 2;

    private bool wasMoveLockedBeforeAttack;

    public AttackState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {
        _player = entity as Player;
        _mover = entity.GetCompo<EntityMover>();
        _attackCompo = entity.GetCompo<PlayerAttackCompo>();
    }

    public override void Enter()
    {
        base.Enter();

        if (_comboCounter > MAX_COMBO_COUNT || Time.time >= _lastAttackTime + _comboWindow)
            _comboCounter = 0;

        _renderer.SetParam(_attackCompo.ComboCounterParam, _comboCounter);

        wasMoveLockedBeforeAttack = !_mover.CanManualMove;
        _mover.CanManualMove = false;
        _mover.StopImmediately(true);

        ExecuteAttack();
    }

    public override void Update()
    {
        base.Update();
        if (_isTriggerCall)
            _player.ChangeState("IDLE");
    }

    public override void Exit()
    {
        base.Exit();

        ++_comboCounter;
        _lastAttackTime = Time.time;

        if (!_dialogueComponent.isDialogue && !wasMoveLockedBeforeAttack)
            _mover.CanManualMove = true;

        wasMoveLockedBeforeAttack = false;
    }

    private void ExecuteAttack()
    {
        float attackDirection = GetAttackDirection();
        AttackDataSO attackData = GetCurrentAttackData();

        PlayAttackEffects();
        ApplyAttackMovement(attackData, attackDirection);

        _attackCompo.SetAttackData(attackData);
    }

    private void ApplyAttackMovement(AttackDataSO attackData, float attackDirection)
    {
        Vector2 movement = attackData.movement;
        movement.x *= attackDirection;
        _mover.AddForceToEntity(movement);
    }
    private float GetAttackDirection()
    {
        float attackDirection = _renderer.FacingDirection;
        float xInput = _player.PlayerInput.InputDirection.x;

        if (!Mathf.Approximately(xInput, 0f))
            attackDirection = Mathf.Sign(xInput);

        return attackDirection;
    }

    private AttackDataSO GetCurrentAttackData()
    {
        return _attackCompo.GetAttackData($"PlayerCombo{_comboCounter}");
    }

    private void PlayAttackEffects()
    {
        _mover.EffectorPlayer.PlayEffect($"Combo{_comboCounter}AttackEffect");

        AudioManager.Instance.PlaySound2D($"SwordAttack{_comboCounter}", 0f, false, SoundType.SfX);

        if (_comboCounter == 2)
            AudioManager.Instance.PlaySound2D("SwordAttack0", 0.5f, false, SoundType.SfX);
    }
}
