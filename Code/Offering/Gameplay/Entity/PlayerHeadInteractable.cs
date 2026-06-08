using UnityEngine;

public class PlayerHeadInteractable : MonoBehaviour,IEntityComponent, IInteractable
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider headCollider;

    public Transform TargetTransform => transform;
    public Player Owner => owner;
    private Player owner;

    private Player _carrier;

    public void Initialize(Entity entity)
    {
        owner = entity as Player;
    }
    public void EnablePickup()
    {
        _carrier = null;

        if (headCollider != null)
            headCollider.enabled = true;

        if (rb != null)
            rb.isKinematic = false;
    }

    public void DisablePickup()
    {
        if (headCollider != null)
            headCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void OnInteract(Entity interactor)
    {
        Debug.Log($"[Head] OnInteract called. interactor={interactor}, owner={owner}");

        Player carrier = interactor as Player;
        if (carrier == null)
        {
            Debug.Log("[Head] fail: carrier is null");
            return;
        }

        if (owner == null)
        {
            Debug.Log("[Head] fail: owner is null");
            return;
        }

        if (!owner.IsHeadDetached)
        {
            Debug.Log("[Head] fail: owner head is not detached");
            return;
        }

        if (carrier == owner)
        {
            Debug.Log("[Head] fail: owner cannot pick own head");
            return;
        }

        if (_carrier != null)
        {
            Debug.Log("[Head] fail: already carried");
            return;
        }

        PlayerHeadHolder holder = carrier.GetCompo<PlayerHeadHolder>();
        if (holder == null)
        {
            Debug.Log("[Head] fail: carrier has no PlayerHeadHolder");
            return;
        }

        if (holder.HasHead)
        {
            Debug.Log("[Head] fail: carrier already has head");
            return;
        }

        if (!owner.TryPickUpHead(carrier))
        {
            Debug.Log("[Head] fail: owner.TryPickUpHead returned false");
            return;
        }

        Debug.Log("[Head] pickup success");

        ForcePickup(carrier);

        PlayerNetworkObject networkObject = owner.GetComponent<PlayerNetworkObject>();
        if (networkObject != null)
            networkObject.NotifyHeadPickedUp(carrier);
    }

    public bool ForcePickup(Player carrier)
    {
        if (carrier == null) return false;

        PlayerHeadHolder holder = carrier.GetCompo<PlayerHeadHolder>();
        if (holder == null) return false;

        if (owner != null && owner.HeadCarrier != carrier)
        {
            if (!owner.TryPickUpHead(carrier))
                return false;
        }

        _carrier = carrier;

        if (!holder.HasHead)
            holder.PickUp(this);

        if (headCollider != null)
            headCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        return true;
    }


    public void Drop(Vector3 worldPosition)
    {
        _carrier = null;

        transform.SetParent(null, true);
        transform.position = worldPosition;

        if (headCollider != null)
            headCollider.enabled = true;

        if (rb != null)
            rb.isKinematic = false;

        if (owner != null)
            owner.DropFromCarrier();
    }

    public void ClearCarrier()
    {
        _carrier = null;
    }

    public void OnEnterInteractionRange()
    {
    }

    public void OnExitInteractionRange()
    {
    }

}
