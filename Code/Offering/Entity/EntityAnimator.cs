using System;
using UnityEngine;
using UnityEngine.Events;

public class EntityAnimator : MonoBehaviour, IEntityComponent
{
    public AnimationEventHandler Handler { get; private set; }

    public UnityEvent<Vector3, Quaternion> OnAnimatorMoveEvent;
    [SerializeField] private Animator _animator;

    public bool ApplyRootMotion
    {
        get => _animator.applyRootMotion;
        set => _animator.applyRootMotion = value;
    }


    private Entity _entity;

    public void Initialize(Entity entity)
    {
        _entity = entity;
    }

    private void OnAnimatorMove()
    {
        OnAnimatorMoveEvent?.Invoke(_animator.deltaPosition, _animator.deltaRotation);
    }

    public void SetParam(int hash, float value) => _animator.SetFloat(hash, value);
    public void SetParam(int hash, bool value) => _animator.SetBool(hash, value);
    public void SetParam(int hash, int value) => _animator.SetInteger(hash, value);
    public void SetParam(int hash) => _animator.SetTrigger(hash);

    public void SetParam(int hash, float value, float dampTime)
        => _animator.SetFloat(hash, value, dampTime, Time.deltaTime);

    public void SetAnimatorOff()
    {
        _animator.enabled = false;
    }
}