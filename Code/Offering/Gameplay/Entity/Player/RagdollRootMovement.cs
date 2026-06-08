using UnityEngine;

public class RagdollRootMovement : MonoBehaviour
{
    [Tooltip("작성하신 CharacterMovement가 달려있는 마스터 오브젝트의 Transform")]
    public Transform masterTarget;
    public float moveForce = 5000f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (masterTarget == null) return;

        // 마스터와 현재 래그돌 루트 간의 거리와 방향 계산
        Vector3 targetPosition = masterTarget.position;
        Vector3 direction = targetPosition - transform.position;

        // 마스터의 위치로 물리적인 힘(속도)을 가해서 쫓아감
        _rb.linearVelocity = direction * (moveForce * Time.fixedDeltaTime);

        // 회전도 마스터를 따라가게 설정 (원한다면 AddTorque나 MoveRotation 사용 가능)
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, masterTarget.rotation, Time.fixedDeltaTime * 10f));
    }
}