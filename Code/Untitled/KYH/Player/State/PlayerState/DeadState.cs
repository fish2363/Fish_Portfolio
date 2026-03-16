using UnityEngine;

public class DeadState : EntityState
{
    public DeadState(Entity entity, AnimParamSO animParam) : base(entity, animParam)
    {

    }

    public override void Enter()
    {
        base.Enter();
        _entity.gameObject.layer = _entity.DeadBodyLayer;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
