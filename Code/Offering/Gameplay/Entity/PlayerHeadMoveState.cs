using UnityEngine;

public class PlayerHeadMoveState : PlayerState
{
    private CharacterMovement _movement;
    private CharacterController _characterController;
    private BallController _ballController;
    private VisualContainer _container;
    private WeaponHolder _weaponHolder;
    private Quaternion _originalHeadLocalRotation;
    private Vector3 _originalHeadLocalScale;
    private const float SelfAttachDelay = 3f;
    private float _selfAttachTimer;

    public PlayerHeadMoveState(Entity entity, int animationHash) : base(entity, animationHash)
    {
        _movement = entity.GetCompo<CharacterMovement>();
        _characterController = _movement.CharacterController;
        _ballController = entity.GetCompo<BallController>();
        _weaponHolder = entity.GetCompo<WeaponHolder>();
        _container = entity.GetCompo<VisualContainer>();

        if (_container != null && _container.DetachedHeadModel != null)
        {
            _originalHeadLocalRotation = _container.DetachedHeadModel.transform.localRotation;
            _originalHeadLocalScale = _container.DetachedHeadModel.transform.localScale;
        }
    }

    public override void Enter()
    {
        base.Enter();

        if (_container.AttachedBodyModel != null)
            _container.AttachedBodyModel.SetActive(false);

        if (_container.DetachedBodyModel != null)
            _container.DetachedBodyModel.SetActive(true);

        if (_container.DetachedHeadModel != null)
        {
            Transform headTransform = _container.DetachedHeadModel.transform;

            headTransform.localScale = _originalHeadLocalScale;
            headTransform.SetParent(null, true);
            headTransform.rotation = _originalHeadLocalRotation;

            _container.DetachedHeadModel.SetActive(true);

            PlayerHeadInteractable headInteractable =
                _container.DetachedHeadModel.GetComponent<PlayerHeadInteractable>();

            if (headInteractable != null)
                headInteractable.EnablePickup();
        }
        _selfAttachTimer = SelfAttachDelay;
        if (_movement != null)
            _movement.enabled = false;

        if (_characterController != null)
            _characterController.enabled = false;

        if (_ballController != null)
            _ballController.enabled = true;

        if (_weaponHolder != null && _weaponHolder.GetHoldItem())
            _weaponHolder.DropAllItems();

        if (_player.IsLocalPlayer && _container.DetachedHeadModel != null)
        {
            Bus<SwapTrackingEvent>.Raise(
                new SwapTrackingEvent().Initialize(_container.DetachedHeadModel.transform)
            );
        }
    }

    public override void Update()
    {
        base.Update();

        if (!_player.IsLocalPlayer) return;
        if (_player.IsHeadCarried) return;

        if (_selfAttachTimer > 0f)
        {
            _selfAttachTimer -= Time.deltaTime;
            return;
        }

        if (_player.CanSelfAttach())
            _player.SelfAttachHead();
    }

    public override void Exit()
    {
        base.Exit();

        if (_ballController != null)
            _ballController.enabled = false;

        if (_characterController != null)
            _characterController.enabled = true;

        if (_movement != null)
            _movement.enabled = true;

        if (_container.DetachedHeadModel != null && _container.HeadPoint != null)
        {
            PlayerHeadInteractable headInteractable =
                _container.DetachedHeadModel.GetComponent<PlayerHeadInteractable>();

            if (headInteractable != null)
                headInteractable.DisablePickup();

            Transform headTransform = _container.DetachedHeadModel.transform;

            headTransform.SetParent(_container.HeadPoint, false);
            headTransform.localPosition = Vector3.zero;
            headTransform.localRotation = _originalHeadLocalRotation;
            headTransform.localScale = _originalHeadLocalScale;

            _container.DetachedHeadModel.SetActive(false);
        }

        if (_container.DetachedBodyModel != null)
            _container.DetachedBodyModel.SetActive(false);

        if (_container.AttachedBodyModel != null)
            _container.AttachedBodyModel.SetActive(true);

        _player.transform.rotation = Quaternion.Euler(
            0f,
            _player.transform.rotation.eulerAngles.y,
            0f
        );
    }
}
