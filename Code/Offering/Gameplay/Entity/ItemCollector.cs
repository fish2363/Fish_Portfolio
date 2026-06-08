using UnityEngine;
using UnityEngine.Events;

public class ItemCollector : MonoBehaviour, IEntityComponent
{
    [Header("Settings")]
    [SerializeField] private Transform checkPosition;
    [field: SerializeField] public float CheckRadius { get; set; } = 2f;
    [SerializeField] private LayerMask pickUpLayer;
    [SerializeField] private float checkInterval = 0.15f;

    [Header("Events")]
    public UnityEvent<IInteractable> OnPickUpWeapon;

    private Entity owner;
    private Player _player;
    private WeaponHolder _weaponHolder;
    private IInteractable currentTarget;
    private readonly Collider[] overlapResults = new Collider[16];
    private float lastCheckTime;

    public void Initialize(Entity entity)
    {
        owner = entity;
        _player = entity as Player;

        if (checkPosition == null)
            checkPosition = transform;

        _weaponHolder = entity.GetCompo<WeaponHolder>();
    }

    public bool TryInteract()
    {
        if (_player != null && !_player.IsLocalPlayer)
            return false;

        if (currentTarget == null)
        {
            Debug.Log("[ItemCollector] currentTarget is null");
            return false;
        }

        Debug.Log($"[ItemCollector] Interact target: {currentTarget}");

        if (currentTarget is ItemVisual weaponItem)
        {
            ItemSO data = weaponItem.GetWeaponData();
            _weaponHolder.Equip(data);
        }

        currentTarget.OnInteract(owner);

        OnPickUpWeapon?.Invoke(currentTarget);
        currentTarget = null;

        return true;
    }

    private void Update()
    {
        if (Time.time < lastCheckTime + checkInterval)
            return;

        UpdateNearestTarget();
        lastCheckTime = Time.time;
    }

    private void UpdateNearestTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            checkPosition.position,
            CheckRadius,
            overlapResults,
            pickUpLayer,
            QueryTriggerInteraction.Collide
        );

        IInteractable closestInteractable = null;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapResults[i];
            if (col == null) continue;

            IInteractable interactable = null;

            if (!col.TryGetComponent(out interactable))
                interactable = col.GetComponentInParent<IInteractable>();

            if (interactable == null)
                interactable = col.GetComponentInChildren<IInteractable>();

            Debug.Log($"[ItemCollector] detected={col.name}, interactable={interactable}");

            if (interactable == null)
                continue;

            float sqrDistance = (checkPosition.position - col.transform.position).sqrMagnitude;

            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable != currentTarget)
        {
            currentTarget?.OnExitInteractionRange();
            currentTarget = closestInteractable;
            currentTarget?.OnEnterInteractionRange();

            Debug.Log($"[ItemCollector] currentTarget changed: {currentTarget}");
        }

        System.Array.Clear(overlapResults, 0, count);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (checkPosition == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(checkPosition.position, CheckRadius);
    }
#endif
}
