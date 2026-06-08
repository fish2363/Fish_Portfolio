using UnityEngine;

public class PlayerHeadController : MonoBehaviour, IEntityComponent
{
    [SerializeField] private float selfAttachDistance = 1.5f;

    private Player _player;
    private CharacterMovement _movement;
    private VisualContainer _visualContainer;
    private PlayerHeadInteractable _headInteractable;

    public bool IsDetached { get; private set; }
    public Player Carrier { get; private set; }
    public bool IsCarried => Carrier != null;

    public void Initialize(Entity entity)
    {
        _player = entity as Player;
        _movement = entity.GetCompo<CharacterMovement>();
        _visualContainer = entity.GetCompo<VisualContainer>();
        _headInteractable = entity.GetCompo<PlayerHeadInteractable>();
    }

    public void Drop()
    {
        if (IsDetached) return;

        IsDetached = true;
        Carrier = null;

        _player.RaiseHeadDropped();
        _player.ChangeState("HEAD_ROLL", true);
    }

    public bool CanSelfAttach()
    {
        if (!IsDetached) return false;
        if (IsCarried) return false;
        if (_visualContainer == null) return false;
        if (_visualContainer.DetachedHeadModel == null) return false;
        if (_visualContainer.DetachedBodyModel == null) return false;

        float sqrDistance = (
            _visualContainer.DetachedHeadModel.transform.position -
            _visualContainer.DetachedBodyModel.transform.position
        ).sqrMagnitude;

        return sqrDistance <= selfAttachDistance * selfAttachDistance;
    }

    public void SelfAttach()
    {
        if (!CanSelfAttach()) return;

        AttachAndRevive();
        _player.RaiseSelfAttached();
    }

    public void AttachAndRevive()
    {
        _player.IsDead = false;
        IsDetached = false;
        Carrier = null;

        if (_headInteractable != null)
            _headInteractable.ClearCarrier();

        if (_visualContainer != null)
            _visualContainer.RestoreAttachedVisual();

        if (_movement != null)
            _movement.SetManualMovement(true);

        _player.RestoreFullHealth();
        _player.ChangeState("IDLE", true);
    }

    public bool TryPickUp(Player carrier)
    {
        if (carrier == null) return false;
        if (carrier == _player) return false;
        if (!IsDetached) return false;
        if (IsCarried) return false;

        Carrier = carrier;
        _player.RaiseHeadPickedUp(carrier);
        return true;
    }

    public void DropFromCarrier()
    {
        if (!IsCarried) return;

        Carrier = null;
        _player.RaiseHeadDroppedByCarrier();
    }

    public void SacrificeBy(Player scorer)
    {
        if (scorer == null) return;
        if (!IsDetached) return;

        Bus<ScoreAddedEvent>.Raise(new ScoreAddedEvent(scorer.NetworkPlayerID, 1));

        _player.RaiseSacrificed(scorer);
        AttachAndRevive();
    }
}
