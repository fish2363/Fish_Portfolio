using UnityEngine;

public class PlayerHeadHolder : MonoBehaviour, IEntityComponent
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private StatSO moveSpeedStat;
    [SerializeField] private float carrySlowAmount = -2f;

    private EntityStatCompo _statCompo;
    private PlayerHeadInteractable _carriedHead;
    private Entity _owner;
    public bool HasHead => _carriedHead != null;
    public PlayerHeadInteractable CarriedHead => _carriedHead;

    public void Initialize(Entity entity)
    {
        _statCompo = entity.GetCompo<EntityStatCompo>();
        _owner = entity;
        _owner.OnHitEvent.AddListener(Drop);
    }

    public bool PickUp(PlayerHeadInteractable head)
    {
        if (head == null) return false;
        if (_carriedHead != null) return false;

        if (holdPoint == null)
        {
            Debug.LogWarning("[HeadHolder] holdPoint is null", this);
            return false;
        }

        _carriedHead = head;

        Transform headTransform = head.transform;

        Vector3 worldScale = headTransform.lossyScale;

        headTransform.SetParent(holdPoint, true);
        headTransform.position = holdPoint.position;
        headTransform.rotation = holdPoint.rotation;
        SetWorldScale(headTransform, worldScale);

        ApplySlow();
        return true;
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        Transform parent = target.parent;

        if (parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        Vector3 parentScale = parent.lossyScale;

        target.localScale = new Vector3(
            parentScale.x != 0f ? worldScale.x / parentScale.x : worldScale.x,
            parentScale.y != 0f ? worldScale.y / parentScale.y : worldScale.y,
            parentScale.z != 0f ? worldScale.z / parentScale.z : worldScale.z
        );
    }

    public void Drop()
    {
        if (_carriedHead == null) return;

        Vector3 dropPosition = transform.position + transform.forward * 1.2f + Vector3.up * 0.5f;

        PlayerHeadInteractable head = _carriedHead;
        _carriedHead = null;

        RemoveSlow();
        head.Drop(dropPosition);
    }
    private void OnDestroy()
    {
        _owner.OnHitEvent.RemoveListener(Drop);
    }
    public PlayerHeadInteractable RemoveForSacrifice()
    {
        if (_carriedHead == null) return null;

        PlayerHeadInteractable head = _carriedHead;
        _carriedHead = null;

        RemoveSlow();
        return head;
    }

    private void ApplySlow()
    {
        if (_statCompo == null || moveSpeedStat == null) return;

        _statCompo.AddModifier(_statCompo.GetStat(moveSpeedStat), this, carrySlowAmount);
    }

    private void RemoveSlow()
    {
        if (_statCompo == null || moveSpeedStat == null) return;

        _statCompo.RemoveModifier(_statCompo.GetStat(moveSpeedStat), this);
    }
}
