using System.Collections.Generic;
using UnityEngine;


public class PlayerAttackCompo : MonoBehaviour, IEntityComponent, IAfterInit
{
    [SerializeField] private DamageCaster damageCaster;
    [field: SerializeField] public AnimParamSO ComboCounterParam { get; private set; }
    [SerializeField] private List<AttackDataSO> attackDataList;

    [Header("Counter attack settings")]
    public float counterAttackDuration;
    public AnimParamSO successCounterParam;
    public LayerMask whatIsCounterable;

    private Player _player;
    private EntityRenderer _renderer;
    private EntityMover _mover;
    private EntityAnimationTrigger _triggerCompo;

    public bool CanAttack { get; set; }
    private bool _canJumpAttack;

    private Dictionary<string, AttackDataSO> _attackDataDictionary;
    private AttackDataSO _currentAttackData;

    #region Init section

    public void Initialize(Entity entity)
    {
        _player = entity as Player;
        _renderer = entity.GetCompo<EntityRenderer>();
        _mover = entity.GetCompo<EntityMover>();
        _triggerCompo = entity.GetCompo<EntityAnimationTrigger>();
        damageCaster.InitCaster(entity);

        //리스트를 딕셔너리로 변경한다.
        _attackDataDictionary = new Dictionary<string, AttackDataSO>();
        attackDataList.ForEach(attackData => _attackDataDictionary.Add(attackData.attackName, attackData));
    }


    private void OnDestroy()
    {
        _triggerCompo.OnAttackTrigger -= HandleAttackTrigger;
    }
    #endregion



    public bool CanJumpAttack()
    {
        bool returnValue = _canJumpAttack;
        if (_canJumpAttack)
            _canJumpAttack = false;
        return returnValue;
    }

    private void FixedUpdate()
    {
        if (_canJumpAttack == false && _mover.IsGroundDetected())
            _canJumpAttack = true;
    }

    public AttackDataSO GetAttackData(string attackName)
    {
        AttackDataSO data = _attackDataDictionary.GetValueOrDefault(attackName);
        Debug.Assert(data != null, $"request attack data is not exist : {attackName}");
        return data;
    }

    

    public void SetAttackData(AttackDataSO attackData)
    {
        _currentAttackData = attackData;
    }
    

    private void HandleAttackTrigger()
    {
        float damage = _currentAttackData.attackDamage; //나중에 스탯기반으로 고침. 
        Vector2 knockBackForce = _currentAttackData.knockBackForce;
        bool success = damageCaster.CastDamage(damage, knockBackForce, _currentAttackData.isPowerAttack);

        if (success)
        {
            _mover.EffectorPlayer.PlayEffect("HitEffect");
            AudioManager.Instance.PlaySound2D("HitSuccess", 0,false,SoundType.SfX);
            Debug.Log($"<color=red>Damaged! - {damage}</color>");
        }
    }

    public ICounterable GetCounterableTargetInRadius()
    {
        Vector3 center = damageCaster.transform.position;
        Collider2D collider = damageCaster.GetCounterableTarget(center, whatIsCounterable);
        //Collider2D collider = Physics2D.OverlapCircle(center, damageCaster.GetSize(), whatIsCounterable);
        if (collider != null)
            return collider.GetComponent<ICounterable>();
        return default;
    }

    public void AfterInitialize()
    {
        _triggerCompo.OnAttackTrigger += HandleAttackTrigger;
    }
}
