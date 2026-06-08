using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttackCompo : MonoBehaviour, IEntityComponent, IAfterInitialize
{
    [Header("Impulse Settings")]
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private bool canImpulseOnlyHit;

    [Header("Attack Direction")]
    [SerializeField] private Transform cameraTransform;

    [Header("attack data"), SerializeField] private AttackDataSO[] attackDataList;

    [SerializeField] private StatSO attackSpeedStat;
    [SerializeField] private StatSO physicalDamageStat;
    [SerializeField] private float comboWindow;

    private Entity _entity;
    private EntityAnimator _entityAnimator;
    private EntityVFX _vfxCompo;
    private EntityAnimatorTrigger _animatorTrigger;
    private EntityStatCompo _statCompo;
    private DamageCompo _damageCompo;
    private PlayerNetworkObject _networkObject;

    private readonly int _attackSpeedHash = Animator.StringToHash("ATTACK_SPEED");
    private readonly int _comboCounterHash = Animator.StringToHash("COMBO_COUNTER");

    private float _attackSpeed = 1f;
    private float _lastAttackTime;
    private Vector3 _currentAttackDirection;
    private bool _hasDamageCasted;
    public int ComboCounter { get; set; } = 0;

    [SerializeField] private DamageCaster damageCaster;

    public float AttackSpeed
    {
        get => _attackSpeed;
        set
        {
            _attackSpeed = value;
            _entityAnimator.SetParam(_attackSpeedHash, _attackSpeed);
        }
    }

    public void Initialize(Entity entity)
    {
        _entity = entity;
        _entityAnimator = entity.GetCompo<EntityAnimator>();
        _vfxCompo = entity.GetCompo<EntityVFX>();
        _animatorTrigger = entity.GetCompo<EntityAnimatorTrigger>();
        _statCompo = entity.GetCompo<EntityStatCompo>();
        _damageCompo = entity.GetCompo<DamageCompo>();
        _networkObject = entity.GetComponent<PlayerNetworkObject>();

        _currentAttackDirection = entity.transform.forward;
    }

    public void AfterInitialize()
    {
        if (damageCaster != null)
            damageCaster.InitCaster(_entity);
        else
            Debug.LogError("PlayerAttackCompo: damageCaster is not assigned");

        _animatorTrigger.AttackStartTrigger += HandleAttackStartTrigger;
        _animatorTrigger.OnAttackVFXTrigger += HandleAttackVFXTrigger;
        _animatorTrigger.OnDamageCastTrigger += HandleDamageCasterTrigger;

        StatSO target = _statCompo.GetStat(attackSpeedStat);
        Debug.Assert(target != null, $"{attackSpeedStat.statName} does not exist");

        target.OnValueChanged += HandleAttackSpeedChange;
        AttackSpeed = target.Value;
    }

    private void OnDestroy()
    {
        _animatorTrigger.AttackStartTrigger -= HandleAttackStartTrigger;
        _animatorTrigger.OnAttackVFXTrigger -= HandleAttackVFXTrigger;
        _animatorTrigger.OnDamageCastTrigger -= HandleDamageCasterTrigger;

        StatSO target = _statCompo.GetStat(attackSpeedStat);
        if (target != null)
        {
            target.OnValueChanged -= HandleAttackSpeedChange;
        }
    }

    private void HandleAttackStartTrigger()
    {
        _currentAttackDirection = GetCameraAttackDirection();
        RotateToDirection(_currentAttackDirection);
    }

    private void HandleDamageCasterTrigger()
    {
        Debug.Log(
            $"damageCaster={damageCaster}, " +
            $"_damageCompo={_damageCompo}, " +
            $"impulseSource={impulseSource}, " +
            $"physicalDamageStat={physicalDamageStat}, " +
            $"attackDataList={attackDataList}, " +
            $"ComboCounter={ComboCounter}"
        );

        AttackDataSO attackData = GetCurrentAttackData();

        Debug.Log($"attackData={attackData}");

        if (_networkObject != null &&
            _networkObject.IsSpawned &&
            _networkObject.IsOwner &&
            !_networkObject.IsServer)
        {
            _networkObject.RequestServerDamageCast(_currentAttackDirection, ComboCounter);
            return;
        }

        CastDamage(_currentAttackDirection, ComboCounter);
    }

    public bool CastDamage(Vector3 attackDirection, int comboCounter)
    {
        ComboCounter = comboCounter;

        AttackDataSO attackData = GetCurrentAttackData();
        DamageData damageData = _damageCompo.CalculateDamage(physicalDamageStat, attackData);

        Vector3 position = damageCaster.transform.position;
        bool isSuccess = damageCaster.CastDamage(damageData, position, attackDirection, attackData);

        if (canImpulseOnlyHit == false || isSuccess)
        {
            impulseSource.GenerateImpulse(attackData.impulseForce);
        }

        return isSuccess;
    }

    public Vector3 GetCameraAttackDirection()
    {
        Transform cam = cameraTransform;

        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        if (cam == null)
            return _entity.transform.forward;

        Vector3 direction = cam.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return _entity.transform.forward;

        return direction.normalized;
    }

    private void RotateToDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        _entity.transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void HandleAttackSpeedChange(StatSO stat, float currentvalue, float previousvalue)
    {
        AttackSpeed = currentvalue;
    }

    private void HandleAttackVFXTrigger()
    {
        _vfxCompo.PlayVfx($"Blade{ComboCounter}", Vector3.zero, Quaternion.identity);
    }

    public void Attack()
    {
        _hasDamageCasted = false;

        _currentAttackDirection = GetCameraAttackDirection();
        RotateToDirection(_currentAttackDirection);

        bool comboCounterOver = ComboCounter > 2;
        bool comboWindowExhaust = Time.time >= _lastAttackTime + comboWindow;

        if (comboCounterOver || comboWindowExhaust)
        {
            ComboCounter = 0;
        }

        _entityAnimator.SetParam(_comboCounterHash, ComboCounter);
    }

    public void EndAttack()
    {
        ComboCounter++;

        if (ComboCounter > 2)
            ComboCounter = 0;

        _lastAttackTime = Time.time;
    }
    
    public void RotateToAttackDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        _entity.transform.rotation = Quaternion.LookRotation(direction.normalized);
    }
    public AttackDataSO GetCurrentAttackData()
    {
        Debug.Assert(attackDataList.Length > ComboCounter, "Combo counter is out of range");
        return attackDataList[ComboCounter];
    }
}
