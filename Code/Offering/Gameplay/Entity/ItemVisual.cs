//using UnityEngine;

//public class ItemVisual : MonoBehaviour
//{
//    [SerializeField] private Collider itemCollider;
//    [SerializeField] private Rigidbody itemRigidbody;

//    private void Awake()
//    {
//        // 장착 중일 때는 물리 연산을 끕니다.
//        itemCollider.enabled = false;
//        itemRigidbody.isKinematic = true;
//    }

//    public void Drop()
//    {
//        // 땅에 떨어질 때 물리 연산을 켭니다.
//        transform.SetParent(null);
//        itemCollider.enabled = true;
//        itemRigidbody.isKinematic = false;

//        // (선택) 바닥에 떨어지면 다시 주울 수 있게 상호작용 레이어로 변경하거나
//        // DroppedItem 컴포넌트를 활성화하는 로직이 들어갈 수 있습니다.
//    }
//}
