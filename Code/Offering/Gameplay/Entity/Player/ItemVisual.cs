using UnityEngine;

/// <summary>
/// 플레이어가 획득할 수 있는 무기 아이템 클래스입니다.
/// IInteractable 인터페이스를 구현하여 ItemCollector와 상호작용합니다.
/// </summary>
[RequireComponent(typeof(Collider))] // 상호작용을 위해 콜라이더 필수
public class ItemVisual : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    [SerializeField] private ItemSO item;
    [SerializeField] private Collider itemCollider;
    [SerializeField] private Rigidbody itemRigidbody;

    public Transform TargetTransform => transform;
    public void SetupAsEquipped()
    {
        itemCollider.enabled = false;
        itemRigidbody.isKinematic = true;
    }

    public void Drop(Vector3 dropScale)
    {
        transform.SetParent(null);

        transform.localScale = dropScale;

        itemCollider.enabled = true;
        itemRigidbody.isKinematic = false;
    }
    public void OnEnterInteractionRange()
    {
    }

    public void OnExitInteractionRange()
    {
    }
    public ItemSO GetWeaponData() => item;

    public void OnInteract(Entity interactor)
    {
        Destroy(gameObject);
    }
}