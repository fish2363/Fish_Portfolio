using UnityEngine;

public class BumperObstacle : MonoBehaviour
{
    [Header("TwinKnockback Settings")]
    [Tooltip("플레이어가 날아가는 속도와 시간 곡선 데이터")]
    [SerializeField] private MovementDataSO knockbackData;
    [Tooltip("위로 얼마나 띄울 것인지 (파티 게임 특유의 포물선 날아가기)")]
    [SerializeField] private float upwardForce = 1.0f;

    // 움직이는 물체의 충돌 처리는 주로 OnTriggerEnter를 사용합니다.
    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 대상이 넉백 가능한 객체(IKnockBackable)인지 확인
        if (other.GetComponentInParent<Entity>().GetCompo<BallController>().gameObject.TryGetComponent(out IKnockBackable knockBackable))
        {
            // 1. 날아갈 방향 계산 (장애물 중심 -> 플레이어 방향)
            Vector3 hitDirection = (other.transform.position - transform.position).normalized;

            // 2. 파티 애니멀즈처럼 위로 붕~ 뜨게 만들려면 Y축 값을 더해줍니다.
            hitDirection.y += upwardForce;
            hitDirection = hitDirection.normalized;

            // 3. 플레이어의 KnockBack 함수 호출!
            knockBackable.KnockBack(hitDirection, knockbackData);

            // 4. (선택) 튕기는 이펙트나 사운드 재생
            // Debug.Log("플레이어 홈런!");
        }
    }
}