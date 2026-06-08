using System;
using DG.Tweening;
using UnityEngine;

public class CharacterMovement : MonoBehaviour, IEntityComponent, IAfterInitialize, IKnockBackable
{
    [SerializeField] private StatSO moveSpeedStat;
    private StatSO _moveStat;
    private EntityStatCompo _statCompo;

    [Header("Movement Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Jump & Cute Settings")]
    [SerializeField] private float jumpForce = 5f;
    [Tooltip("스케일 애니메이션을 줄 캐릭터의 메시(그래픽) 오브젝트.")]
    [SerializeField] private Transform visualTransform;

    // 🌟 추가된 부분: 인스펙터에서 실시간으로 찌그러짐 정도를 조절할 수 있게 변수로 뺌
    [Tooltip("점프할 때 홀쭉해지는 비율 (기본값 1 기준)")]
    [SerializeField] private Vector3 jumpStretchScale = new Vector3(0.9f, 1.1f, 0.9f); // 10%만 변화
    [Tooltip("착지할 때 납작해지는 비율 (기본값 1 기준)")]
    [SerializeField] private Vector3 landSquashScale = new Vector3(1.15f, 0.85f, 1.15f); // 15%만 변화
    [Tooltip("찌그러지는 애니메이션 속도")]
    [SerializeField] private float squashDuration = 0.15f;

    public CharacterController CharacterController;
    [SerializeField] private Transform cameraTransform;

    public bool CanManualMovement { get; set; } = true;
    public bool IsGround => CharacterController != null && CharacterController.isGrounded;
    public Vector3 Velocity => _velocity;

    private Vector3 _autoMovement;
    private float _autoMoveStartTime;
    private MovementDataSO _movementData;
    private Vector3 _velocity;
    private float _verticalVelocity;
    private Vector3 _movementDirection;
    private Player _player;
    private Entity _entity;
    private Vector2 _moveInput;
    private Vector3 _worldMoveDirection;
    private bool _useWorldMoveDirection;

    private bool _wasGrounded;
    private Vector3 _originalScale;

    public void Initialize(Entity entity)
    {
        _entity = entity;
        _player = entity as Player;
        _statCompo = entity.GetCompo<EntityStatCompo>();

        if (CharacterController != null)
            _wasGrounded = CharacterController.isGrounded;

        if (visualTransform != null)
            _originalScale = visualTransform.localScale;

        if (_player != null && _player.PlayerInput != null)
            _player.PlayerInput.OnRunningPressed += HandleSpeedChange;

        if (TestInstance.Instance != null)
            cameraTransform = TestInstance.Instance.cameraTranform;
    }

    private void HandleSpeedChange(bool obj)
    {
        if (!_player.IsLocalPlayer) return;

        if (obj)
            _statCompo.AddModifier(_statCompo.GetStat(moveSpeedStat), this, runSpeed);
        else
            _statCompo.RemoveModifier(_statCompo.GetStat(moveSpeedStat), this);
    }

    public void AfterInitialize() { SubscribeMoveSpeed(); }
    private void SubscribeMoveSpeed()
    {
        _moveStat = _statCompo.GetStat(moveSpeedStat);
        if (_moveStat == null)
        {
            _moveSpeed = 4f;
            return;
        }

        _moveStat.OnValueChanged += HandleMoveSpeedChange;
        _moveSpeed = _moveStat.Value;
    }
    private void OnDestroy()
    {
        if (_player != null && _player.PlayerInput != null)
            _player.PlayerInput.OnRunningPressed -= HandleSpeedChange;

        UnsubscribeMoveSpeed();
    }
    private void UnsubscribeMoveSpeed()
    {
        if (_moveStat == null) return;
        _moveStat.OnValueChanged -= HandleMoveSpeedChange;
        _moveStat = null;
    }
    private void HandleMoveSpeedChange(StatSO stat, float currentvalue, float previousvalue)
    {
        Debug.Log($"[MoveSpeed] t={Time.time:F3} {previousvalue} -> {currentvalue}");
        _moveSpeed = currentvalue;
    }
    public void SetMovementDirection(Vector2 movementInput)
    {
        _moveInput = movementInput.normalized;
        _useWorldMoveDirection = false;
    }

    public void SetWorldMovementDirection(Vector3 movementDirection)
    {
        movementDirection.y = 0f;

        _worldMoveDirection = movementDirection.sqrMagnitude > 0.0001f
            ? movementDirection.normalized
            : Vector3.zero;
        _useWorldMoveDirection = true;
    }

    public void Jump()
    {
        if (_player != null && !_player.IsLocalPlayer) return;
        if (!CanManualMovement) return;

        if (IsGround)
        {
            _verticalVelocity = jumpForce;

            if (visualTransform != null)
            {
                visualTransform.DOKill();
                visualTransform.localScale = _originalScale;

                Vector3 targetScale = new Vector3(
                    _originalScale.x * jumpStretchScale.x,
                    _originalScale.y * jumpStretchScale.y,
                    _originalScale.z * jumpStretchScale.z
                );

                visualTransform.DOScale(targetScale, squashDuration).SetLoops(2, LoopType.Yoyo);
            }
        }
    }

    private void FixedUpdate()
    {
        CalculateMovement();
        ApplyGravity();
        Move();
    }

    private void CalculateMovement()
    {

        if (CanManualMovement)
        {
            Vector3 moveDirection = _useWorldMoveDirection
                ? _worldMoveDirection
                : GetCameraRelativeDirection(_moveInput);
            _velocity = moveDirection * _moveSpeed;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Transform parent = _entity.transform;
                parent.rotation = Quaternion.Lerp(parent.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            }

            return;
        }

        if (_movementData == null)
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
            return;
        }

        float normalizeTime = (Time.time - _autoMoveStartTime) / _movementData.duration;

        if (normalizeTime >= 1f)
        {
            _movementData = null;
            _autoMovement = Vector3.zero;

            _velocity.x = 0f;
            _velocity.z = 0f;
            return;
        }

        float currentSpeed = _movementData.maxSpeed * _movementData.moveCurve.Evaluate(normalizeTime);
        _velocity = _autoMovement * currentSpeed;
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (cameraTransform == null)
            return new Vector3(input.x, 0f, input.y);

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }

    private void ApplyGravity()
    {
        if (IsGround)
        {
            if (_verticalVelocity < 0)
                _verticalVelocity = -2.0f;

            if (!_wasGrounded && _verticalVelocity < 0f)
            {
                if (visualTransform != null)
                {
                    visualTransform.DOKill();
                    visualTransform.localScale = _originalScale;

                    // 🌟 변수를 곱해서 착지 스케일 적용
                    Vector3 targetScale = new Vector3(
                        _originalScale.x * landSquashScale.x,
                        _originalScale.y * landSquashScale.y,
                        _originalScale.z * landSquashScale.z
                    );

                    visualTransform.DOScale(targetScale, squashDuration).SetLoops(2, LoopType.Yoyo);
                }
            }
        }
        else
        {
            _verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        _wasGrounded = IsGround;
        _velocity.y = _verticalVelocity;
    }

    private void Move()
    {
        if (CharacterController == null) return;
        if (!CharacterController.enabled) return;
        CharacterController.Move(_velocity * Time.fixedDeltaTime);
    }

    public void StopImmediately()
    {
        _movementDirection = Vector3.zero;
        _moveInput = Vector2.zero;
        _worldMoveDirection = Vector3.zero;
        _useWorldMoveDirection = false;
        _velocity = Vector3.zero;
        _autoMovement = Vector3.zero;
        _movementData = null;
    }

    public void KnockBack(Vector3 direction, MovementDataSO knockbackMovement)
    {
        if (knockbackMovement == null) return;

        CanManualMovement = false;

        _autoMoveStartTime = Time.time;
        _movementData = knockbackMovement;
        _autoMovement = direction.normalized;
    }

    public void SetManualMovement(bool canMove)
    {
        CanManualMovement = canMove;

        StopImmediately();
    }
    public void ApplyMovementData(Vector3 playerDirection, MovementDataSO movementData)
    {
        if (movementData == null)
        {
            _movementData = null;
            _autoMovement = Vector3.zero;
            _velocity.x = 0f;
            _velocity.z = 0f;
            return;
        }

        _autoMovement = playerDirection.normalized;
        _autoMoveStartTime = Time.time;
        _movementData = movementData;
    }
}
