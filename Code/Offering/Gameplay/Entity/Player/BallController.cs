using UnityEngine;

public class BallController : MonoBehaviour, IEntityComponent, IKnockBackable
{
    [SerializeField] private PlayerInputSO inputReader;
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float turnBrakeStrength = 8f;
    [SerializeField] private float noInputDragMultiplier = 0.98f;

    [SerializeField] private Rigidbody rb;

    private Player _player;
    private Vector2 moveInput;

    private bool isKnockedBack;
    private float knockbackEndTime;

    public void Initialize(Entity entity)
    {
        _player = entity as Player;

        if (_player != null && inputReader == null)
            inputReader = _player.PlayerInput;
    }

    private void OnEnable()
    {
        if (rb != null)
            rb.isKinematic = false;
    }

    private void FixedUpdate()
    {
        if (_player == null) return;
        if (!_player.IsLocalPlayer) return;
        if (_player.IsHeadCarried) return;

        if (isKnockedBack)
        {
            if (Time.time >= knockbackEndTime)
                isKnockedBack = false;
            else
                return;
        }

        moveInput = inputReader.MovementKey;

        Transform cam = cameraTransform;
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            ApplyTurnBrake(moveDir, horizontalVelocity);
            rb.AddForce(moveDir * moveForce, ForceMode.Force);
        }
        else
        {
            horizontalVelocity *= noInputDragMultiplier;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        LimitSpeed();
    }

    public void KnockBack(Vector3 direction, MovementDataSO knockbackMovement)
    {
        if (knockbackMovement == null) return;
        if (rb == null) return;

        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackMovement.duration;

        rb.linearVelocity = Vector3.zero;

        Vector3 knockbackForce = direction.normalized * knockbackMovement.maxSpeed;
        rb.AddForce(knockbackForce, ForceMode.VelocityChange);
    }

    private void ApplyTurnBrake(Vector3 moveDir, Vector3 horizontalVelocity)
    {
        if (horizontalVelocity.sqrMagnitude < 0.001f)
            return;

        float alignment = Vector3.Dot(horizontalVelocity.normalized, moveDir);

        if (alignment < 0.2f)
        {
            float brakeFactor = 1f - Mathf.Clamp01(turnBrakeStrength * Time.fixedDeltaTime);
            horizontalVelocity *= brakeFactor;

            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void LimitSpeed()
    {
        Vector3 horizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontal.magnitude > maxSpeed)
        {
            Vector3 limited = horizontal.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, rb.linearVelocity.z);
        }
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        isKnockedBack = false;
    }
}
