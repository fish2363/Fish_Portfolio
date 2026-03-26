using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHeadMoveState : PlayerState
{
    private CharacterMovement _movement;
    private BallController _ballController;
    private VisualContainer _container;
    private WeaponHolder _weaponHolder;

    public PlayerHeadMoveState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _movement = entity.GetCompo<CharacterMovement>();
        _characterController = _movement.CharacterController;
        _ballController = entity.GetCompo<BallController>();
        _weaponHolder = entity.GetCompo<WeaponHolder>();
        _container = entity.GetCompo<VisualContainer>();
    }

    public override void Enter()
    {
        base.Enter();

        Bus<SwapTrackingEvent>.Raise(new SwapTrackingEvent().Initialize(_container.DetachedHeadModel.transform));

        _movement.enabled = false;
        _ballController.enabled = true;

        if(TryHatHead())
            _weaponHolder.DropWeapons();
    }
    public bool TryHatHead()
    {
        return _weaponHolder.CurWeapon != null;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        //Bus<SwapTrackingEvent>.Raise(new SwapTrackingEvent().Initialize(_player.transform));

        //_ballController.enabled = false;

        //_player.transform.rotation = Quaternion.Euler(0f, _player.transform.rotation.eulerAngles.y, 0f);

        //// 2. 걷기 제어 다시 켜기
        //_characterController.enabled = true;
        //_movement.enabled = true;

        //_container.DetachedHeadModel.transform.localPosition = _container._headPoint.localPosition;

        //_container.DetachedHeadModel.SetActive(false);
        //_container.AttachedBodyModel.SetActive(true);
    }
}
